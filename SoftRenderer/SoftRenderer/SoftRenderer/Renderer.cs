﻿// jave.lin 2019.07.15

//#define DOUBLE_BUFF // 使用双缓存
//#define BUFF_RGBA // 使用4通道缓存，不开使用3通道，没有Alpha

using RendererCommon.SoftRenderer.Common.Shader;
using SoftRenderer.Common.Mathes;
using SoftRenderer.SoftRenderer.Primitives;
using SoftRenderer.SoftRenderer.Rasterization;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Imaging;

namespace SoftRenderer.SoftRenderer
{
    [Description("渲染器")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Renderer : IDisposable
    {
        public static Renderer Instance { get; private set; }

        private int backBufferWidth = 512;
        private int backBufferHeight = 512;
        private PixelFormat pixelFormat;
        internal ColorBuffer frontBuffer;
        internal ColorBuffer backBuffer;
        private List<OutInfo[]> vertexOutput = new List<OutInfo[]>();
        private List<Primitive_Triangle> trianglePrimitiveHelper = new List<Primitive_Triangle>();
        private List<Primitive_Line> linePrimitiveHelper = new List<Primitive_Line>();
        private List<Primitive_Point> pointPrimitiveHelper = new List<Primitive_Point>();
        private List<FragInfo> genShadedFragHelper = new List<FragInfo>();  // shaded 的片段
        private List<FragInfo> genWireframeFragHelper = new List<FragInfo>();  // wireframe 的片段
        private List<FragInfo> genNormalLineFragHelper = new List<FragInfo>();  // normal line 的片段

#if DOUBLE_BUFF
        private bool bufferDirty = false;
#endif
        public ShaderProgram ShaderProgram { get; set; }
        public ShaderLoadMgr ShaderMgr { get; private set; }
        public BasicShaderData ShaderData { get; set; }
        public RenderState State { get; private set; }
        public Rasterizer Rasterizer { get; private set; }
        public int BackBufferWidth { get => backBufferWidth; }
        public int BackBufferHeight { get => backBufferHeight; }

        public Per_Frag Per_Frag { get; private set; }
        public Per_Vertex Per_Vertex { get; private set; }

        public VertexBuffer CurVertexBuffer { get; private set; }
        public IndexBuffer CurIndexBuffer { get; private set; }

        public Renderer(int bufferW = 512, int bufferH = 512, PixelFormat pixelFormat = PixelFormat.Format24bppRgb)
        {
            this.backBufferWidth = bufferW;
            this.backBufferHeight = bufferH;
            this.pixelFormat = pixelFormat;

            frontBuffer = new ColorBuffer(bufferW, bufferH);
            backBuffer = new ColorBuffer(bufferW, bufferH);

            State = new RenderState(this);
            Rasterizer = new Rasterizer(this);
            Per_Frag = new Per_Frag(this);
            Per_Vertex = new Per_Vertex(this);
            ShaderData = new ShaderData(1);
            ShaderMgr = new ShaderLoadMgr(this);
            ShaderProgram = new ShaderProgram(this);

            if (Instance == null) Instance = this;
        }

        public void Clear(ClearFlag flag = ClearFlag.ColorBuffer | ClearFlag.DepthBuffer)
        {
            if ((flag | ClearFlag.ColorBuffer) != 0)
                backBuffer.Clear();
            if ((flag | ClearFlag.DepthBuffer) != 0)
                Per_Frag.DepthBuff.Clear();
        }

        public void Present()
        {
            // draw call

            if (CurVertexBuffer == null)
                throw new Exception("current vertex buffer not binding.");
            if (CurIndexBuffer == null)
                throw new Exception("current index buffer not binding.");

            var vs = ShaderProgram.GetShader(ShaderType.VertexShader);
            var fs = ShaderProgram.GetShader(ShaderType.FragmentShader);

            if (vs == null) throw new Exception("Not setting vs, it is required shader.");
            if (fs == null) throw new Exception("Not setting fs, it is required shader.");

            // vertex shader
            VertexShader(vs);
            // 如果需要clip处理的话，一般需要在这里就先PrimitiveAssebly
            // 然后再VS PostProcessing
            //PrimitiveAssembly();
            // 这里我就暂时没做剪切处理
            // 比较简单的处理是，先图元装配好，在对整个图元所有相关的顶点判断
            // 是否都在 -clipPos.w < clipPos.xyz < clipPos.w
            // 如果都不在，那么整个图元剔除

            // vertex shader post-processing
            VertexShader_PostProcessing();

            // tessellation(control/evaluate) shader - not implements
            // geometry shader - not implements

            // primitive assembly
            PrimitiveAssembly();

            // rasterizer & fragment shader
            // 为了节省内存，我就将每个图元Rasterizer光栅出来的片段就马上处理shader了
            // 而不必等到所有的所有图元都光栅完再处理FragmentShader
            // 那个时候，片段列表会非常大，因为片段多
            RasterizeAndFragmentShader(fs);
        }

        private void VertexShader(ShaderBase vs)
        {
            var buffer = CurVertexBuffer;
            var floatBuff = buffer.buff;
            vertexOutput.Clear();
            for (int i = 0; i < floatBuff.Length; i += buffer.floatNumPerVertice)
            {
                foreach (var format in buffer.Formats)
                {
                    var offset = i + format.offset;
                    switch (format.type)
                    {
                        case VertexDataType.Position:
                            var pos = new Vector4(
                                floatBuff[offset + 0],
                                floatBuff[offset + 1],
                                floatBuff[offset + 2],
                                1
                                );
                            vs.ShaderProperties.SetIn(InLayout.Position, pos);
                            break;
                        case VertexDataType.Color:
                            var color = new ColorNormalized(
                                floatBuff[offset + 0],
                                floatBuff[offset + 1],
                                floatBuff[offset + 2],
                                floatBuff[offset + 3]
                                );
                            vs.ShaderProperties.SetIn(InLayout.Color, color);
                            break;
                        case VertexDataType.UV:
                            var uv = new Vector2(
                                floatBuff[offset + 0],
                                floatBuff[offset + 1]
                                );
                            vs.ShaderProperties.SetIn(InLayout.Texcoord, uv);
                            break;
                        case VertexDataType.Normal:
                            var normal = new Vector3(
                                floatBuff[offset + 0],
                                floatBuff[offset + 1],
                                floatBuff[offset + 2]
                                );
                            vs.ShaderProperties.SetIn(InLayout.Normal, normal);
                            break;
                        case VertexDataType.Tangent:
                            var tangent = new Vector3(
                                floatBuff[offset + 0],
                                floatBuff[offset + 1],
                                floatBuff[offset + 2]
                                );
                            vs.ShaderProperties.SetIn(InLayout.Tangent, tangent);
                            break;
                        default:
                            break;
                    }
                }
                vs.Main();

                var outs = vs.ShaderProperties.GetOuts();
                vertexOutput.Add(outs);
            }
        }

        private void VertexShader_PostProcessing()
        {
            var len = vertexOutput.Count;
            var cx = State.CameraViewport.X;
            var cy = State.CameraViewport.Y;
            var cw = State.CameraViewport.Width;
            var ch = State.CameraViewport.Height;
            var f = State.CameraFar;
            var n = State.CameraNear;
            var isOrtho = State.IsOrtho;
            for (int i = 0; i < len; i++)
            {
                var outInfos = vertexOutput[i];
                var jLen = outInfos.Length;
                for (int j = 0; j < jLen; j++)
                {
                    if (outInfos[j].layout == OutLayout.SV_Position)
                    {
                        Vector4 ndcPos;
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
                            ndcPos.x = clipPos.x / clipPos.w;
                            ndcPos.y = clipPos.y / clipPos.w;
                            ndcPos.z = clipPos.z / clipPos.w;
                            ndcPos.w = clipPos.w;
                            //ndcPos = clipPos / clipPos.w;
                        }
                        // ndc 2 win
                        var wposX = cw * 0.5f * ndcPos.x + (cx + cw * 0.5f);
                        var wposY = ch * 0.5f * ndcPos.y + (cy + ch * 0.5f);
                        var wposZ = (f - n) * 0.5f * ndcPos.z + (f + n) * 0.5f;
                        var winPos = new Vector4(wposX, wposY, wposZ, ndcPos.w);
                        outInfos[j].value = winPos;
                        break;
                    }
                }
            }
        }

        private void PrimitiveAssembly()
        {
            switch (State.PolygonMode)
            {
                case PolygonMode.Triangle:
                    var len = CurIndexBuffer.Buffer.Length;
                    var buff = CurIndexBuffer.Buffer;
                    trianglePrimitiveHelper.Clear();
                    for (int i = 0; i < len; i += 3)
                    {
                        trianglePrimitiveHelper.Add(
                            new Primitive_Triangle(
                                FragInfo.GetFragInfo(vertexOutput[buff[i + 0]]),
                                FragInfo.GetFragInfo(vertexOutput[buff[i + 1]]),
                                FragInfo.GetFragInfo(vertexOutput[buff[i + 2]])
                                ));
                    }
                    break;
                case PolygonMode.Line:
                    throw new Exception($"not implements polygonMode:{State.PolygonMode}");
                case PolygonMode.Point:
                    throw new Exception($"not implements polygonMode:{State.PolygonMode}");
                default:
                    throw new Exception($"not implements polygonMode:{State.PolygonMode}");
            }
        }

        private void RasterizeAndFragmentShader(ShaderBase fs)
        {
            switch (State.PolygonMode)
            {
                case PolygonMode.Triangle:
                    var len = trianglePrimitiveHelper.Count;
                    for (int i = 0; i < len; i++)
                    {
                        var t = trianglePrimitiveHelper[i];
                        // 光栅化成片段
                        Rasterizer.GenFragInfo(t, genShadedFragHelper, genWireframeFragHelper, genNormalLineFragHelper);
                        // 处理片段
                        InnerFragmentShader(fs, genShadedFragHelper, genWireframeFragHelper, genNormalLineFragHelper);
                    }
                    break;
                case PolygonMode.Line:
                    throw new Exception($"not implements polygonMode:{State.PolygonMode}");
                case PolygonMode.Point:
                    throw new Exception($"not implements polygonMode:{State.PolygonMode}");
                default:
                    throw new Exception($"not implements polygonMode:{State.PolygonMode}");
            }
        }

        private void InnerFragmentShader(
            ShaderBase fs, 
            List<FragInfo> shadedResult, 
            List<FragInfo> wireframeResult,
            List<FragInfo> normalLineResult)
        {
            /* ======depth start====== */
            var depthbuff = Per_Frag.DepthBuff;
            var depthwrite = State.DepthWrite;
            var maxZ = State.CameraFar;
            maxZ += State.CameraFar * State.CameraNear;
            var depthInv = 1 / maxZ;
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
            offsetDepth = 
                //faceNormalDotForward * renderer.State.DepthOffsetFactor + 
                depthInv * State.DepthOffsetUnit;
            //}
            var depthOffset = State.DepthOffset;
            /* ======depth end====== */

            /* ======alpha test start====== */
            var alphaTest = State.AlphaTest;
            var alphaTestComp = State.AlphaTestComp;
            var alphaTestRef = State.AlphaTestRef;
            /* ======alpha test start====== */

            /* ======blend start====== */
            var blend = State.Blend;
            var srcColorFactor = State.BlendSrcColorFactor;
            var dstColorFactor = State.BlendDstColorFactor;
            var srcAlphaFactor = State.BlendSrcAlphaFactor;
            var dstAlphaFactor = State.BlendDstAlphaFactor;
            var colorOp = State.BlendColorOp;
            var alphaOp = State.BlendAlphaOp;
            /* ======blend end====== */

            // shaded
            var len = shadedResult.Count;
            var bmd = Begin(); // begin
            for (int i = 0; i < len; i++)
            {
                var f = shadedResult[i];
                f.depth = 1 - f.p.z * depthInv;
                var testDepth = f.depth;
                if (depthOffset == DepthOffset.On)
                    testDepth += offsetDepth;

                // 深度测试
                if (depthbuff.Test(State.DepthTest, (int)f.p.x, (int)f.p.y, testDepth))
                {
                    // 是否开启深度写入
                    if (depthwrite == DepthWrite.On)
                    {
                        depthbuff.Write((int)f.p.x, (int)f.p.y, testDepth);
                    }
                    // 执行fragment shader
                    var jLen = f.UpperStageOutInfos.Length;
                    for (int j = 0; j < jLen; j++)
                    {
                        var info = f.UpperStageOutInfos[j];
                        fs.ShaderProperties.SetInWithOut(info.layout, info.value, info.location);
                    }
                    fs.Main();
                    var srcColor = fs.ShaderProperties.GetOut<ColorNormalized>(OutLayout.SV_Target); // 目前值处理SV_Target0
                    // alpha 测试
                    if (alphaTest == AlphaTest.On)
                    {
                        var srcAlpha = Mathf.Clamp(srcColor.a, 0, 1);
                        if (!Per_Frag.AlphaTest(alphaTestComp, alphaTestRef, srcAlpha))
                        {
                            //f.discard = true; // alpha 测试失败
                            continue;
                        }
                    }

                    // 是否开启混合
                    if (blend == Blend.On)
                    {
                        var dstColor = BeginRead(bmd.Scan0, f.p);
                        srcColor  = Per_Frag.Blend(srcColor, dstColor, srcColorFactor, dstColorFactor, srcAlphaFactor, dstAlphaFactor, colorOp, alphaOp);
                    }
                    BeginSetPixel(bmd.Scan0, f.p, srcColor);
                }
            }

            // wireframe
            len = wireframeResult.Count;
            offsetDepth = 
                //faceNormalDotForward * (renderer.State.DepthOffsetFactor) +
                depthInv * (State.DepthOffsetUnit - 0.01f);
            var wireframeColor = State.WireframeColor;
            for (int i = 0; i < len; i++)
            {
                var f = wireframeResult[i];
                f.depth = 1 - f.p.z * depthInv;
                var testDepth = f.depth + offsetDepth;
                var c = wireframeColor;
                if (depthbuff.Test(State.DepthTest, (int)f.p.x, (int)f.p.y, testDepth))
                {
                    // 是否开启深度写入
                    if (depthwrite == DepthWrite.On)
                    {
                        depthbuff.Write((int)f.p.x, (int)f.p.y, testDepth);
                    }
                    BeginSetPixel(bmd.Scan0, f.p, c);
                }
            }

            // debug: show normal line
            if (State.DebugShowNormal)
            {
                var blueColor = new ColorNormalized(0, 0, 1, 1);
                len = normalLineResult.Count;
                for (int i = 0; i < len; i++)
                {
                    var f = normalLineResult[i];
                    if (f.discard) continue;
                    BeginSetPixel(bmd.Scan0, f.p, blueColor);
                }
            }

            End(bmd); // end
        }

        public void BindVertexBuff(VertexBuffer buffer)
        {
            CurVertexBuffer = buffer;
        }

        public void BindIndexBuff(IndexBuffer buffer)
        {
            CurIndexBuffer = buffer;
        }

        internal BitmapData Begin()
        {
            return backBuffer.Begin();
        }
        internal void BeginSetPixel(IntPtr ptr, Vector3 v, ColorNormalized color)
        {
            //BeginSetPixel(ptr, (int)Math.Round(v.x), (int)Math.Round(v.y), color);
            BeginSetPixel(ptr, (int)(v.x), (int)(v.y), color);
        }
        internal void BeginSetPixel(IntPtr ptr, int x, int y, ColorNormalized color)
        {
            //if (x < 0 || x >= backBufferWidth || y < 0 || y >= backBufferHeight) return;
            color.Clamp();
            backBuffer.BeginSetPixel(ptr, x, y, color);
#if DOUBLE_BUFF
            bufferDirty = true;
#endif
        }
        internal ColorNormalized BeginRead(IntPtr ptr, int x, int y)
        {
            //if (x < 0 || x >= backBufferWidth || y < 0 || y >= backBufferHeight) return;
            return backBuffer.BeginRead(ptr, x, y);
        }
        internal ColorNormalized BeginRead(IntPtr ptr, Vector3 p)
        {
            return BeginRead(ptr, (int)p.x, (int)p.y);
        }
        internal void End(BitmapData bmd)
        {
            backBuffer.End(bmd);
        }

        internal void SetPixel(int x, int y, ColorNormalized color)
        {
            //if (x < 0 || x >= backBufferWidth || y < 0 || y >= backBufferHeight) return;
            backBuffer.SetPixel(x, y, color);
#if DOUBLE_BUFF
            bufferDirty = true;
#endif
        }

        public ColorBuffer SwapBuffer()
        {
#if DOUBLE_BUFF
            if (bufferDirty)
            {
                bufferDirty = false;
                var t = backBuffer;
                backBuffer = frontBuffer;
                frontBuffer = t;
            }
            return frontBuffer;
#endif
            return backBuffer;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (frontBuffer != null)
            {
                frontBuffer.Dispose();
                frontBuffer = null;
            }
            if (backBuffer != null)
            {
                backBuffer.Dispose();
                backBuffer = null;
            }
            if (Rasterizer != null)
            {
                Rasterizer.Dispose();
                Rasterizer = null;
            }
            if (Per_Frag != null)
            {
                Per_Frag.Dispose();
                Per_Frag = null;
            }
            if (Per_Vertex != null)
            {
                Per_Vertex.Dispose();
                Per_Vertex = null;
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
            if (ShaderProgram != null)
            {
                ShaderProgram.Dispose();
                ShaderProgram = null;
            }
            
        }
    }
}
