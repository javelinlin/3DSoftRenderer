// jave.lin 2019.07.15

#define DOUBLE_BUFF // 使用双缓存

using RendererCore.Renderer.Primitives;
using RendererCore.Renderer.Rasterization;
using RendererCoreCommon.Renderer.Common.Mathes;
using RendererCoreCommon.Renderer.Common.Shader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RendererCore.Renderer
{
    [Description("渲染器")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Renderer : IDisposable
    {
        public static Renderer Instance { get; private set; }

        // 混合
        public static Vector4 BlendHandle(
            Vector4 src, Vector4 dst,
            BlendFactor srcColorFactor, BlendFactor dstColorFactor,
            BlendFactor srcAlphaFactor, BlendFactor dstAlphaFactor,
            BlendOp blendColorOp, BlendOp blendAlphaOp
            )
        {
            var sr = src.r;
            var sg = src.g;
            var sb = src.b;
            var sa = src.a;

            var dr = dst.r;
            var dg = dst.g;
            var db = dst.b;
            var da = dst.a;

            var oneMSA = 1 - sa;
            var oneMDA = 1 - da;

            switch (srcColorFactor)
            {
                case BlendFactor.One: /* noops */ break;
                case BlendFactor.Zero: sr = 0; sg = 0; sb = 0; break;
                case BlendFactor.SrcAlpha: sr *= sa; sg *= sa; sb *= sa; break;
                case BlendFactor.OneMinusSrcAlpha: sr *= oneMSA; sg *= oneMSA; sb *= oneMSA; break;
                case BlendFactor.DstAlpha: sr *= da; sg *= da; sb *= da; break;
                case BlendFactor.OneMinusDstAlpha: sr *= oneMDA; sg *= oneMDA; sb *= oneMDA; break;
                default: throw new NotImplementedException($"Not implements");
            }
            switch (dstColorFactor)
            {
                case BlendFactor.One: /* noops */ break;
                case BlendFactor.Zero: dr = 0; dg = 0; db = 0; break;
                case BlendFactor.SrcAlpha: dr *= sa; dg *= sa; db *= sa; break;
                case BlendFactor.OneMinusSrcAlpha: dr *= oneMSA; dg *= oneMSA; db *= oneMSA; break;
                case BlendFactor.DstAlpha: dr *= da; dg *= da; db *= da; break;
                case BlendFactor.OneMinusDstAlpha: dr *= oneMDA; dg *= oneMDA; db *= oneMDA; break;
                default: throw new NotImplementedException($"Not implements");
            }
            switch (srcAlphaFactor)
            {
                case BlendFactor.One: /* noops */ break;
                case BlendFactor.Zero: sa = 0; break;
                case BlendFactor.SrcAlpha: sa *= sa; break;
                case BlendFactor.OneMinusSrcAlpha: sa *= oneMSA; break;
                case BlendFactor.DstAlpha: sa *= da; break;
                case BlendFactor.OneMinusDstAlpha: sa *= oneMDA; break;
                default: throw new NotImplementedException($"Not implements");
            }
            switch (dstAlphaFactor)
            {
                case BlendFactor.One: /* noops */ break;
                case BlendFactor.Zero: da = 0; break;
                case BlendFactor.SrcAlpha: da *= sa; break;
                case BlendFactor.OneMinusSrcAlpha: da *= oneMSA; break;
                case BlendFactor.DstAlpha: da *= da; break;
                case BlendFactor.OneMinusDstAlpha: da *= oneMDA; break;
                default: throw new NotImplementedException($"Not implements");
            }
            switch (blendColorOp)
            {
                case BlendOp.Add: sr += dr; sg += dg; sb += db; break;
                case BlendOp.Sub: sr -= dr; sg -= dg; sb -= db; break;
                case BlendOp.Multiply: sr *= dr; sg *= dg; sb *= db; break;
                case BlendOp.Divide: sr /= dr; sg /= dg; sb /= db; break; // 除法的性能好渣，尽量不用
                default: throw new NotImplementedException($"Not implements");
            }
            switch (blendAlphaOp)
            {
                case BlendOp.Add: sa += da; break;
                case BlendOp.Sub: sa -= da; break;
                case BlendOp.Multiply: sa *= da; break;
                case BlendOp.Divide: sa /= da; break; // 除法的性能好渣，尽量不用
                default: throw new NotImplementedException($"Not implements");
            }
            return Vector4.Get(sr, sg, sb, sa);
        }

        private Buffer_Color frontBuffer;           // frontbuffer
        private FrameBuffer defaultFrameBuffer;     // backbuffer
        private List<ShaderOut> vertexshaderOutput = new List<ShaderOut>();
        private List<Primitive_Triangle> trianglePrimitiveHelper = new List<Primitive_Triangle>();
        private List<Primitive_Line> linePrimitiveHelper = new List<Primitive_Line>();
        private List<Primitive_Point> pointPrimitiveHelper = new List<Primitive_Point>();
        private List<FragInfo> genShadedFragHelper = new List<FragInfo>();  // shaded 的片段
        private List<FragInfo> genWireframeFragHelper = new List<FragInfo>();  // wireframe 的片段
        private List<FragInfo> genNormalLineFragHelper = new List<FragInfo>();  // normal line 的片段

        private VertexBuffer curVertexBuffer;       // 当前处理的顶点数据
        private IndexBuffer curIndexBuffer;         // 当前处理的索引数据

        internal SubShader UsingSubShader;          // 当前使用的subshader
        internal Pass UsingPass;                    // 当前使用的pass

        internal Rasterizer Rasterizer { get; private set; }              // 光栅器

#if DOUBLE_BUFF
        private bool bufferDirty = false;
#endif
        public FrameBuffer FrameBuffer { get; set; }                    // 当前使用的帧缓存对象: fbo
        public ShaderBase Shader { get; set; }                          // 当前使用的shader集
        public ShaderLoadMgr ShaderMgr { get; private set; }            // Shader加载管理类
        public BasicShaderData ShaderData { get; set; }                 // 当前shader的uniform block数据
        public RendererState State { get; private set; }                // 当前的渲染器的参数
        public GlobalRenderSstate GlobalState { get; private set; }     // 当前全局状态 - 目前测试用，后面需要重构的
        public int BackBufferWidth { get; }
        public int BackBufferHeight { get; }

        public PixelFormat OutPxielFormat { get; }                      // 当前输出的画布像素格式
        public int OutPixelFormatSize { get; }                          // 当前输出的画布的像素单精度浮点数个数

        public Renderer(int bufferW = 512, int bufferH = 512, PixelFormat outPixelFormat = PixelFormat.Format24bppRgb)
        {
            this.BackBufferWidth = bufferW;
            this.BackBufferHeight = bufferH;

            this.OutPxielFormat = outPixelFormat;
            this.OutPixelFormatSize = GetDevicePixelSize(outPixelFormat);

            this.defaultFrameBuffer = new FrameBuffer(bufferW, bufferH);
            this.FrameBuffer = this.defaultFrameBuffer;     // using default buffer

            this.frontBuffer = new Buffer_Color(bufferW, bufferH);

            State = new RendererState(this);
            GlobalState = new GlobalRenderSstate();
            Rasterizer = new Rasterizer(this);
            ShaderData = new ShaderData(1);
            ShaderMgr = new ShaderLoadMgr(this);

            if (Instance == null) Instance = this;
        }

        public void Clear(ClearFlag flag = ClearFlag.ColorBuffer | ClearFlag.DepthBuffer | ClearFlag.StencilBuffer)
        {
            FrameBuffer.Clear(flag);
        }

        public void SetBackbuffer()
        {
            FrameBuffer = defaultFrameBuffer;
        }

        public void Present()
        {
            // draw call
            if (curVertexBuffer == null)
                throw new Exception("current vertex buffer not binding.");
            if (curIndexBuffer == null)
                throw new Exception("current index buffer not binding.");

            var subShaderCount = Shader.SubShaderList.Count;
            for (int i = 0; i < subShaderCount; i++)
            {
                var subshader = Shader.SubShaderList[i];
                if (!subshader.IsSupported) continue;           // 找到第一个可支持的sub shader，并使用它

                UsingSubShader = subshader;
                break; // Is Supported == true;
            }

            var jLen = UsingSubShader.passList.Count;
            for (int j = 0; j < jLen; j++)                      // 遍历所有的pass
            {
                if (UsingSubShader.passList[j].Actived)
                {
                    UsingPass = UsingSubShader.passList[j];
                    Rasterizer.DrawState = UsingPass.State;
                    Pipeline();                                 // 单次绘制管线
                }
            }

#if DOUBLE_BUFF
            bufferDirty = true;
#endif
        }

        private void Pipeline()
        {
            // vertex shader
            VertexShader();
            // 如果需要clip处理的话，一般需要在这里就先PrimitiveAssebly
            // 然后再VS PostProcessing
            //PrimitiveAssembly();
            // 这里我就暂时没做剪切处理
            // 比较简单的处理是，先图元装配好，在对整个图元所有相关的顶点判断
            // 是否都在 -clipPos.w < clipPos.xyz < clipPos.w
            // 如果都不在，那么整个图元剔除

            // vertex shader post-processing
            VertexShader_PostProcessing();

            // primitive assembly
            PrimitiveAssembly();

            // tessellation(control/evaluate) shader - not implements
            // geometry shader - not implements

            // rasterizer & fragment shader
            // 为了节省内存，我就将每个图元Rasterizer光栅出来的片段就马上处理shader了
            // 而不必等到所有的所有图元都光栅完再处理FragmentShader
            // 那个时候，片段列表会非常大，因为片段多
            RasterizeAndFragmentShader();
        }

        // 后效
        public void PostProcess()
        {
            // 抗锯齿处理，这里的抗锯齿算法有问题，有空再去研究真的MSAA算法
            AAHandle();
            // 全屏模糊
            FullScreenBlurHandle();

#if DOUBLE_BUFF
            bufferDirty = true;
#endif
        }

        public void BindVertexBuff(VertexBuffer buffer)
        {
            curVertexBuffer = buffer;
        }
        public void BindIndexBuff(IndexBuffer buffer)
        {
            curIndexBuffer = buffer;
        }
        public void BackbuffSaveAs(string path)
        {
            using (var bmp = new Bitmap(BackBufferWidth, BackBufferHeight, OutPxielFormat))
            {
                SwapBuffer(bmp);
                bmp.Save(path);
            }
        }
        public void BackbuffSaveAs(Bitmap result)
        {
            SwapBuffer(result);
        }
        public Buffer_Color SwapBuffer()
        {
#if DOUBLE_BUFF
            if (bufferDirty)
            {
                bufferDirty = false;
                var t = FrameBuffer.Attachment.ColorBuffer[0];
                FrameBuffer.Attachment.ColorBuffer[0] = frontBuffer;
                frontBuffer = t;
            }
            return frontBuffer;
#else
            return FrameBuffer.Attachment.ColorBuffer[0];
#endif
        }
        public void SwapBuffer(Bitmap result)
        {
            var targetPixelSize = GetDevicePixelSize(result.PixelFormat);
            if (targetPixelSize != 3 && targetPixelSize != 4)
                throw new Exception($"not support device pixel size:{targetPixelSize}, pixel format:{result.PixelFormat}");

            var buff = SwapBuffer();

            var w = BackBufferWidth;
            var h = BackBufferHeight;

            var bmd = result.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, result.PixelFormat);
            var ptr = bmd.Scan0;
            if (OutPixelFormatSize == 4)
            {
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        var writev = buff[x, y];
                        var offset = (x + y * w) * 4;
                        Marshal.WriteByte(ptr, offset, (byte)(writev.b * 255));
                        Marshal.WriteByte(ptr, offset + 1, (byte)(writev.g * 255));
                        Marshal.WriteByte(ptr, offset + 2, (byte)(writev.r * 255));
                        Marshal.WriteByte(ptr, offset + 3, (byte)(writev.a * 255));
                    }
                }
            }
            else if (OutPixelFormatSize == 3)
            {
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        var writev = buff[x, y];
                        var offset = (x + y * w) * 3;
                        Marshal.WriteByte(ptr, offset, (byte)(writev.b * 255));
                        Marshal.WriteByte(ptr, offset + 1, (byte)(writev.g * 255));
                        Marshal.WriteByte(ptr, offset + 2, (byte)(writev.r * 255));
                    }
                }
            }
            else
            {
                throw new Exception($"not supports out pixel format size:{OutPixelFormatSize}");
            }
            result.UnlockBits(bmd);
        }

        public void TargetWrite(Bitmap result, int targetLocal = 0)
        {
            var targetPixelSize = GetDevicePixelSize(result.PixelFormat);
            if (targetPixelSize != 3 && targetPixelSize != 4)
                throw new Exception($"not support device pixel size:{targetPixelSize}, pixel format:{result.PixelFormat}");

            var buff = FrameBuffer.Attachment.ColorBuffer[targetLocal];

            var w = BackBufferWidth;
            var h = BackBufferHeight;

            var bmd = result.LockBits(new Rectangle(0, 0, w, h), ImageLockMode.WriteOnly, result.PixelFormat);
            var ptr = bmd.Scan0;
            if (OutPixelFormatSize == 4)
            {
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        var writev = buff[x, y];
                        var offset = (x + y * w) * 4;
                        Marshal.WriteByte(ptr, offset, (byte)(writev.b * 255));
                        Marshal.WriteByte(ptr, offset + 1, (byte)(writev.g * 255));
                        Marshal.WriteByte(ptr, offset + 2, (byte)(writev.r * 255));
                        Marshal.WriteByte(ptr, offset + 3, (byte)(writev.a * 255));
                    }
                }
            }
            else if (OutPixelFormatSize == 3)
            {
                for (int x = 0; x < w; x++)
                {
                    for (int y = 0; y < h; y++)
                    {
                        var writev = buff[x, y];
                        var offset = (x + y * w) * 3;
                        Marshal.WriteByte(ptr, offset, (byte)(writev.b * 255));
                        Marshal.WriteByte(ptr, offset + 1, (byte)(writev.g * 255));
                        Marshal.WriteByte(ptr, offset + 2, (byte)(writev.r * 255));
                    }
                }
            }
            else
            {
                throw new Exception($"not supports out pixel format size:{OutPixelFormatSize}");
            }
            result.UnlockBits(bmd);
        }

        private int GetDevicePixelSize(PixelFormat format)
        {
            if (format == PixelFormat.Format24bppRgb) return 3;
            if (format == PixelFormat.Format32bppArgb) return 4;
            else throw new Exception($"not supporting out pixel size:{format}");
        }

        #region 测试post-process代码，需要重构的
        // 为了外部测试用，所以我这里公开了AA的函数
        private void AAHandle()
        {
            if (GlobalState.AA == AA.Off) return;
            switch (GlobalState.AAType)
            {
                case AAType.MSAA: // 这个会很卡
                    AAEdgePickup();              // 提取边缘
                    MSAAHandle();              // 抗锯齿
                    break;
                    throw new Exception($"not implements error, aa type:{GlobalState.AAType}");
                default:
                    throw new Exception($"not implements error, aa type:{GlobalState.AAType}");
            }
        }

        private int[] AA_edgePosList;
        private int AA_edge_count;

        private void AAEdgePickup()
        {
            var depthbuff = FrameBuffer.Attachment.DepthBuffer;
            var w = BackBufferWidth;
            var h = BackBufferHeight;

            if (AA_edgePosList == null ||
                AA_edgePosList.Length != w * h)
            {
                AA_edgePosList = new int[w * h];
            }
            AA_edge_count = 0;

            var sampleOffset = new int[]
                { //x, y
                    // 1
                    //-1, -1,
                    //1, -1,
                    -1, 1,
                    1, 1,
                };
            var sampleCount = sampleOffset.Length;

            var maxOX = w - 1;
            var maxOY = h - 1;

            var edgeColor = Vector4.red;
            var edge_thresold = GlobalState.edge_thresold;
            var show_edge = GlobalState.show_edge;

            var backbuffer = FrameBuffer.Attachment.ColorBuffer[0];

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    var depth = depthbuff.Get(x, y);

                    var isEdge = false;
                    for (int i = 0; i < sampleCount; i += 2)
                    {
                        var ox = sampleOffset[i] + x;
                        var oy = sampleOffset[i + 1] + y;
                        if (ox < 0) ox = 0;
                        if (ox > maxOX) ox = maxOX;
                        if (oy < 0) oy = 0;
                        if (oy > maxOY) oy = maxOY;
                        var offsetDepth = depthbuff.Get(ox, oy);
                        if (float.IsNaN(depth))
                        {
                            if (float.IsNaN(offsetDepth))
                            {
                                continue;
                            }
                            else
                            {
                                isEdge = true;
                                break;
                            }
                        }
                        else
                        {
                            if (float.IsNaN(offsetDepth))
                            {
                                isEdge = true;
                                break;
                            }
                        }

                        if (Math.Abs(depth - offsetDepth) > edge_thresold)
                        {
                            isEdge = true;
                            break;
                        }
                    }
                    if (isEdge)
                    {
                        if (show_edge)
                        {
                            FrameBuffer.WriteColor(0, x, y, edgeColor);
                        }
                        AA_edgePosList[AA_edge_count] = x;
                        AA_edgePosList[AA_edge_count + 1] = y;
                        AA_edge_count += 2;
                    }
                }
            }
        }

        private Buffer_Color AA_src_buffer;

        private void MSAAHandle()
        {
            var w = BackBufferWidth;
            var h = BackBufferHeight;
            var backbuffer = FrameBuffer.Attachment.ColorBuffer[0];
            if (AA_src_buffer == null ||
                AA_src_buffer.Len != w * h)
            {
                AA_src_buffer = new Buffer_Color(w, h);
            }
            backbuffer.CopyTo(AA_src_buffer);
            var sampleOffset = new int[]
                { //x, y
                    // 1
                    -1,0,
                    1,0,
                    //0,1,
                    //0,-1,
                    //-1, -1,
                    //1, -1,
                    //-1, 1,
                    //1, 1,
                };
            var sampleCount = sampleOffset.Length;
            var half_sampleCount = sampleCount / 2;

            var maxOX = w - 1;
            var maxOY = h - 1;

            var resampleCount = GlobalState.aa_resample_count;

            for (int i = 0; i < AA_edge_count; i += 2)
            {
                var x = AA_edgePosList[i];
                var y = AA_edgePosList[i + 1];
                var srcPosColor = backbuffer.Get(x, y);
                for (int rs = 0; rs < resampleCount; rs++)
                {
                    for (int j = 0; j < sampleCount; j += 2)
                    {
                        var ox = sampleOffset[j] * (rs + 1) + x;
                        var oy = sampleOffset[j + 1] * (rs + 1) + y;
                        if (ox < 0) ox = 0;
                        if (ox > maxOX) ox = maxOX;
                        if (oy < 0) oy = 0;
                        if (oy > maxOY) oy = maxOY;
                        srcPosColor += AA_src_buffer[ox, oy];
                    }
                }
                srcPosColor /= half_sampleCount * resampleCount + 1;
                FrameBuffer.WriteColor(0, x, y, srcPosColor);
            }
        }

        private Buffer_Color blur_src_buffer;

        private void FullScreenBlurHandle()
        {
            if (!GlobalState.fullscreen_blur) return;
            var w = BackBufferWidth;
            var h = BackBufferHeight;
            var backbuffer = FrameBuffer.Attachment.ColorBuffer[0];
            if (blur_src_buffer == null ||
                blur_src_buffer.Len != w * h)
            {
                blur_src_buffer = new Buffer_Color(w, h);
            }
            backbuffer.CopyTo(blur_src_buffer);
            var sampleOffset = new int[]
                { //x, y
                    // 1
                    -1,0,
                    1,0,
                    0,1,
                    0,-1,
                    -1, -1,
                    1, -1,
                    -1, 1,
                    1, 1,
                };
            var sampleCount = sampleOffset.Length;
            var half_sampleCount = sampleCount / 2;

            var maxOX = w - 1;
            var maxOY = h - 1;

            var resampleCount = GlobalState.fullscreen_blur_resample_count;

            for (int x = 0; x < w; x++)
            {
                for (int y = 0; y < h; y++)
                {
                    var srcPosColor = backbuffer.Get(x, y);
                    for (int rs = 0; rs < resampleCount; rs++)
                    {
                        for (int i = 0; i < sampleCount; i += 2)
                        {
                            var ox = sampleOffset[i] * (rs + 1) + x;
                            var oy = sampleOffset[i + 1] * (rs + 1) + y;
                            if (ox < 0) ox = 0;
                            if (ox > maxOX) ox = maxOX;
                            if (oy < 0) oy = 0;
                            if (oy > maxOY) oy = maxOY;
                            srcPosColor += blur_src_buffer[ox, oy];
                        }
                    }
                    srcPosColor /= half_sampleCount * resampleCount + 1;
                    FrameBuffer.WriteColor(0, x, y, srcPosColor);
                }
            }
        }
        #endregion

        private void VertexShader()
        {
            var buffer = curVertexBuffer;
            var floatBuff = buffer.buff;

            var passProperties = UsingPass.VertField.Properties;

            vertexshaderOutput.Clear();
            for (int i = 0; i < floatBuff.Length; i += buffer.floatNumPerVertice)
            {
                foreach (var format in buffer.Formats)
                {
                    var offset = i + format.offset;
                    switch (format.type)
                    {
                        case VertexDataType.Position:
                            var pos = Vector4.Get(
                                floatBuff[offset + 0],
                                floatBuff[offset + 1],
                                floatBuff[offset + 2],
                                1
                                );
                            passProperties.SetIn(InLayout.Position, pos);
                            break;
                        case VertexDataType.Color:
                            var color = Vector4.Get(
                                floatBuff[offset + 0],
                                floatBuff[offset + 1],
                                floatBuff[offset + 2],
                                floatBuff[offset + 3]
                                );
                            passProperties.SetIn(InLayout.Color, color);
                            break;
                        case VertexDataType.UV:
                            var uv = new Vector2(
                                floatBuff[offset + 0],
                                floatBuff[offset + 1]
                                );
                            passProperties.SetIn(InLayout.Texcoord, uv);
                            break;
                        case VertexDataType.Normal:
                            var normal = new Vector3(
                                floatBuff[offset + 0],
                                floatBuff[offset + 1],
                                floatBuff[offset + 2]
                                );
                            passProperties.SetIn(InLayout.Normal, normal);
                            break;
                        case VertexDataType.Tangent:
                            var tangent = new Vector3(
                                floatBuff[offset + 0],
                                floatBuff[offset + 1],
                                floatBuff[offset + 2]
                                );
                            passProperties.SetIn(InLayout.Tangent, tangent);
                            break;
                        default:
                            break;
                    }
                }
                UsingPass.Vert();

                var outs = passProperties.GetVertexOuts();
                vertexshaderOutput.Add(new ShaderOut {  upperStageOutInfos = outs });
            }
        }

        private void VertexShader_PostProcessing()
        {
            var len = vertexshaderOutput.Count;
            var cx = State.CameraViewport.X;
            var cy = State.CameraViewport.Y;
            var cw = State.CameraViewport.Width;
            var ch = State.CameraViewport.Height;
            var f = State.CameraFar;
            var n = State.CameraNear;
            var isOrtho = State.IsOrtho;
            for (int i = 0; i < len; i++)
            {
                var shaderOut = vertexshaderOutput[i];
                var outInfos = shaderOut.upperStageOutInfos;
                var jLen = outInfos.Length;
                for (int j = 0; j < jLen; j++)
                {
                    if (outInfos[j].layout == OutLayout.SV_Position)
                    {
                        Vector4 ndcPos = Vector4.Get();
                        // clip here
                        // 这儿本应该处理剪切
                        // 不过如果要在这儿剪切的话，那前提是先图元装配好
                        // 因为没有图元装配好的话，这些离散的顶点，是不知道组合关系的
                        // 不知道组合关系就根本不知道如何剪切
                        // 我看有些博客是直接在这阶段就剪切了
                        // 应该不可能吧，如果我有理解错误，麻烦给我邮件一起讨论：
                        // linjf_008@126.com，谢谢

                        // clip 2 ndc
                        var clipPos = (Vector4)outInfos[j].value;
                        if (isOrtho)
                        {
                            ndcPos = clipPos;
                        }
                        else
                        {
                            var invW = 1 / clipPos.w;
                            // 这里为何要取负才对呢？因为投影矩阵是网上参考copy过来的
                            // 发现是因为矩阵的第三行的，第三，第四个参考导致的，因为第三第四个参数都是负数
                            ndcPos.x = clipPos.x * invW;
                            ndcPos.y = clipPos.y * invW;
                            // ndcPos.z 这里是不是有BUG？因为z应该是-1~1的值，但是结果一直都是小于-1的值
                            // 影响ndcPos.z 值只有MVP的第3行的3,4列数据，还有第4行的第3列-1，取-clipPos.z值，赋值到clipPos.w
                            // 而|clipPos.w|一直都比|clipPos.z|小，所以|clipPos.z /= clipPos.w| > 1
                            // 网上查过好多资料，每个都是这样处理: ndcPos = clipPos/clipPos.w，但是结果都不尽人意
                            // TODO 先放着吧，实在没有精力去研究这个了，坑了好久了。
                            // 网络上也了至少上50篇，没有一个可以解决问题的
                            ndcPos.z = clipPos.z * invW;
                            ndcPos.w = ndcPos.z * 0.5f + 0.5f;
                            //ndcPos = clipPos / clipPos.w;
                        }
                        // ndc 2 win
                        var wposX = cx + cw * (ndcPos.x * 0.5f + 0.5f);
                        var wposY = cy + ch * (ndcPos.y * 0.5f + 0.5f);
                        //var wposX = cw * 0.5f * ndcPos.x + (cx + cw * 0.5f);
                        //var wposY = ch * 0.5f * ndcPos.y + (cy + ch * 0.5f);
                        var wposZ = (f - n) * 0.5f * ndcPos.z + (f + n) * 0.5f;
                        var winPos = Vector4.Get(wposX, wposY, wposZ, ndcPos.w);
                        outInfos[j].value = winPos;
                        shaderOut.clip = ShoudClip(ndcPos);
                        break;
                    }
                }
            }
        }

        private void PrimitiveAssembly()
        {
            var usingPass = UsingPass;
            var drawState = usingPass.State;
            switch (drawState.PolygonMode)
            {
                case PolygonMode.Triangle:
                    var len = curIndexBuffer.Buffer.Length;
                    var buff = curIndexBuffer.Buffer;
                    trianglePrimitiveHelper.Clear();
                    for (int i = 0; i < len; i += 3)
                    {
                        var f0 = FragInfo.GetFragInfo(vertexshaderOutput[buff[i + 0]]);
                        var f1 = FragInfo.GetFragInfo(vertexshaderOutput[buff[i + 1]]);
                        var f2 = FragInfo.GetFragInfo(vertexshaderOutput[buff[i + 2]]);
                        var clip = (f0.ShaderOut.clip || f1.ShaderOut.clip || f2.ShaderOut.clip);
                        trianglePrimitiveHelper.Add(new Primitive_Triangle(f0, f1, f2, clip));
                    }
                    var allPrimitiveClip = true;
                    len = trianglePrimitiveHelper.Count;
                    for (int i = 0; i < len; i++)
                    {
                        var p = trianglePrimitiveHelper[i];
                        if (!p.f0.ShaderOut.clip ||
                            !p.f1.ShaderOut.clip ||
                            !p.f2.ShaderOut.clip)
                        {
                            allPrimitiveClip = false;
                            break;
                        }
                    }
                    if (allPrimitiveClip)
                    {
                        trianglePrimitiveHelper.Clear();
                    }
                    break;
                case PolygonMode.Line:
                    throw new Exception($"not implements polygonMode:{drawState.PolygonMode}");
                case PolygonMode.Point:
                    throw new Exception($"not implements polygonMode:{drawState.PolygonMode}");
                default:
                    throw new Exception($"not implements polygonMode:{drawState.PolygonMode}");
            }
        }

        private bool ShoudClip(Vector4 pos)
        {
            return pos.x < -1 || pos.x > 1 || pos.y < -1 || pos.y > 1 || pos.z < -1 || pos.z > 1;
        }

        private void RasterizeAndFragmentShader()
        {
            var usingPass = UsingPass;
            var drawState = usingPass.State;
            switch (drawState.PolygonMode)
            {
                case PolygonMode.Triangle:
                    var len = trianglePrimitiveHelper.Count;
                    for (int i = 0; i < len; i++)
                    {
                        var t = trianglePrimitiveHelper[i];
                        //if (t.clip) continue;
                        // 光栅化成片段
                        Rasterizer.GenFragInfo(t, genShadedFragHelper, genWireframeFragHelper, genNormalLineFragHelper);
                        // 处理片段
                        InnerFragmentShader(genShadedFragHelper, genWireframeFragHelper, genNormalLineFragHelper);
                    }
                    break;
                case PolygonMode.Line:
                    throw new Exception($"not implements polygonMode:{drawState.PolygonMode}");
                case PolygonMode.Point:
                    throw new Exception($"not implements polygonMode:{drawState.PolygonMode}");
                default:
                    throw new Exception($"not implements polygonMode:{drawState.PolygonMode}");
            }
        }

        private void InnerFragmentShader(
            List<FragInfo> shadedResult, 
            List<FragInfo> wireframeResult,
            List<FragInfo> normalLineResult)
        {
            var usingPass = UsingPass;
            var usingSubShader = UsingSubShader;
            var properties = usingPass.FragField.Properties;

            var drawState = usingPass.State;
            /* ======depth start====== */
            var framebuff = FrameBuffer;
            var depthbuff = FrameBuffer.Attachment.DepthBuffer;
            var depthwrite = drawState.DepthWrite;
            //var maxZ = State.CameraFar;
            //maxZ += State.CameraFar * State.CameraNear;
            //var depthInv = 1 / maxZ;
            // depth offset
            var offsetDepth = 0.0f;
            //if (renderer.State.DepthOffset == DepthOffset.On) // 这里需要优化,法线应该顶点数据中传进来的
            //{
            //// https://blog.csdn.net/linjf520/article/details/94596764
            //var faceNormal = (triangle.f1.p - triangle.f0.p).Cross(triangle.f2.p - triangle.f0.p).normalized; // 这里需要优化,法线应该顶点数据中传进来的
            //                                                                                                  // 掠射角
            //var faceNormalDotForward = 1 - Math.Abs(faceNormal.Dot(Vector3.forward));
            // 我之前翻译的文章：https://blog.csdn.net/linjf520/article/details/94596764
            // 我的理解是上面的这个算法
            //offsetDepth = 
            //    //faceNormalDotForward * renderer.State.DepthOffsetFactor + 
            //    depthInv * State.DepthOffsetUnit;
            //}
            var depthOffset = drawState.DepthOffset;
            /* ======depth end====== */

            /* ======alpha test start====== */
            //var alphaTest = State.AlphaTest;
            //var alphaTestComp = State.AlphaTestComp;
            //var alphaTestRef = State.AlphaTestRef;
            /* ======alpha test start====== */

            /* ======blend start====== */
            var blend = drawState.Blend;
            var srcColorFactor = drawState.BlendSrcColorFactor;
            var dstColorFactor = drawState.BlendDstColorFactor;
            var srcAlphaFactor = drawState.BlendSrcAlphaFactor;
            var dstAlphaFactor = drawState.BlendDstAlphaFactor;
            var colorOp = drawState.BlendColorOp;
            var alphaOp = drawState.BlendAlphaOp;
            /* ======blend end====== */

            // shaded
            var len = shadedResult.Count;
            var backbuffer = FrameBuffer.Attachment.ColorBuffer[0];
            for (int i = 0; i < len; i++)
            {
                var f = shadedResult[i];
                var testDepth = f.depth;
                if (depthOffset == DepthOffset.On)
                    testDepth += offsetDepth;

                // 深度测试
                if (framebuff.DepthTest(drawState.DepthTest, (int)f.p.x, (int)f.p.y, testDepth))
                {
                    // 执行fragment shader
                    var jLen = f.ShaderOut.upperStageOutInfos.Length;
                    for (int j = 0; j < jLen; j++)
                    {
                        var info = f.ShaderOut.upperStageOutInfos[j];
                        properties.SetInWithOut(info.layout, info.value, info.location);
                    }
                    usingPass.f = f;
                    usingPass.Reset();
                    usingPass.Frag();
                    // 丢弃片段
                    if (usingPass.discard) continue;

                    // 是否开启深度写入
                    if (depthwrite == DepthWrite.On)
                    {
                        testDepth = Mathf.Clamp(testDepth, 0, 1);
                        depthbuff.Set((int)f.p.x, (int)f.p.y, testDepth);
                    }

                    var targets = properties.GetTargetOut();
                    var tLen = targets.Length;
                    for (int ti = 0; ti < tLen; ti++)
                    {
                        var tInfo = targets[ti];
                        if (tInfo.localtion == 0)
                        {
                            var srcColor = properties.GetOut<Vector4>(OutLayout.SV_Target); // 目前值处理SV_Target0
                                                                                                     // 是否开启混合
                            if (blend == Blend.On)
                            {
                                var dstColor = backbuffer.Get((int)f.p.x, (int)f.p.y);
                                srcColor = BlendHandle(srcColor, dstColor, srcColorFactor, dstColorFactor, srcAlphaFactor, dstAlphaFactor, colorOp, alphaOp);
                            }
                            framebuff.WriteColor(0, (int)f.p.x, (int)f.p.y, srcColor);
                        }
                        else
                        {
                            framebuff.WriteColor(tInfo.localtion, (int)f.p.x, (int)f.p.y, tInfo.data);
                        }
                    }
                }
            }

            // wireframe
            len = wireframeResult.Count;
            //offsetDepth = 
            //    //faceNormalDotForward * (renderer.State.DepthOffsetFactor) +
            //    depthInv * (State.DepthOffsetUnit - 0.01f);
            var wireframeColor = State.WireframeColor;
            offsetDepth = -0.0000009f;
            for (int i = 0; i < len; i++)
            {
                var f = wireframeResult[i];
                var testDepth = f.depth + offsetDepth;
                var c = wireframeColor;
                if (framebuff.DepthTest(ComparisonFunc.Less, (int)f.p.x, (int)f.p.y, testDepth))
                {
                    // 是否开启深度写入
                    if (depthwrite == DepthWrite.On)
                    {
                        depthbuff.Set((int)f.p.x, (int)f.p.y, testDepth);
                    }
                    backbuffer.Set((int)f.p.x, (int)f.p.y, c);
                }
            }

            // debug: show normal line
            if (State.DebugShowTBN)
            {
                var blueColor = Vector4.Get(0, 0, 1, 1);
                len = normalLineResult.Count;
                for (int i = 0; i < len; i++)
                {
                    var f = normalLineResult[i];
                    if (f.discard) continue;
                    backbuffer.Set((int)f.p.x, (int)f.p.y, f.normalLineColor);
                }
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (frontBuffer != null)
            {
                frontBuffer.Dispose();
                frontBuffer = null;
            }
            if (FrameBuffer != null)
            {
                FrameBuffer.Dispose();
                FrameBuffer = null;
            }
            if (Rasterizer != null)
            {
                Rasterizer.Dispose();
                Rasterizer = null;
            }
            if (ShaderData != null)
            {
                ShaderData.Dispose();
                ShaderData = null;
            }
            if (ShaderMgr != null)
            {
                ShaderMgr.Dispose();
                ShaderMgr = null;
            }

            Shader = null;
            UsingSubShader = null;
            UsingPass = null;

        }
    }
}
