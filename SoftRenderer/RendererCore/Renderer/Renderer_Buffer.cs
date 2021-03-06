﻿// jave.lin 2019.08.06
using RendererCoreCommon.Renderer.Common.Mathes;
using RendererCoreCommon.Renderer.Common.Shader;
using System;
using System.ComponentModel;

namespace RendererCore.Renderer
{
    [Description("帧缓存")]
    public class FrameBuffer : IDisposable
    {
        public class _Attachment : IDisposable
        {
            const int MAX_COLOR_BUFF = 8;

            private FrameBuffer fbo;

            public Buffer_Color[] ColorBuffer = null;       // required
            public Buffer_Depth DepthBuffer;                // required
            public Buffer_Stencil StencilBuffer;            // optional

            public _Attachment(FrameBuffer fbo, bool createDefaultBuff = true)
            {
                this.fbo = fbo;
                if (createDefaultBuff)
                {
                    // required
                    ColorBuffer = new Buffer_Color[MAX_COLOR_BUFF];
                    ColorBuffer[0] = new Buffer_Color(fbo.W, fbo.H);
                    // required
                    DepthBuffer = new Buffer_Depth(fbo.W, fbo.H);
                }
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);

                ColorBuffer = null;
                DepthBuffer = null;
                StencilBuffer = null;
                fbo = null;
            }
        }

        private Vector4 clearedColor = Vector4.gray;
        private float clearedDepth = float.NaN;
        public byte clearedStencil = 0;

        private bool clearedColorDirty = true;
        private bool clearedDepthDirty = true;
        private bool clearedStencilDirty = true;

        private Buffer_Color clearedColorBuff;
        private Buffer_Depth clearedDepthBuff;
        private Buffer_Stencil clearedStencilBuff;

        public Vector4 ClearedColor
        {
            get=> this.clearedColor;
            set
            {
                if (this.clearedColor != value)
                {
                    this.clearedColor = value;
                    this.clearedColorDirty = true;
                }
            }
        }
        public float ClearedDepth
        {
            get => this.clearedDepth;
            set
            {
                if (this.clearedDepth != value)
                {
                    this.clearedDepth = value;
                    this.clearedDepthDirty = true;
                }
            }
        }
        public byte ClearedStencil
        {
            get => this.clearedStencil;
            set
            {
                if (this.clearedStencil != value)
                {
                    this.clearedStencil = value;
                    this.clearedStencilDirty = true;
                }
            }
        }

        public _Attachment Attachment { get; }
        public int W { get; }
        public int H { get; }

        public FrameBuffer(int w, int h, bool createDefaultBuff = true)
        {
            this.W = w;
            this.H = h;
            this.clearedColorBuff = new Buffer_Color(w, h);
            this.clearedDepthBuff = new Buffer_Depth(w, h);
            this.clearedStencilBuff = new Buffer_Stencil(w, h);
            this.Attachment = new _Attachment(this, createDefaultBuff);
        }

        public void Clear(ClearFlag flag)
        {
            if (this.clearedColorDirty)
            {
                this.clearedColorDirty = false;
                this.clearedColorBuff.SetAll(this.clearedColor);
            }
            if (this.clearedDepthDirty)
            {
                this.clearedDepthDirty = false;
                this.clearedDepthBuff.SetAll(this.clearedDepth);
            }
            if (this.clearedStencilDirty)
            {
                this.clearedStencilDirty = false;
                this.clearedStencilBuff.SetAll(this.clearedStencil);
            }
            if ((flag & ClearFlag.ColorBuffer) != 0)
            {
                var len = Attachment.ColorBuffer.Length;
                for (int i = 0; i < len; i++)
                {
                    var buff = Attachment.ColorBuffer[i];
                    if (buff == null) continue;
                    buff.CopyFrom(this.clearedColorBuff);
                }
            }
            if ((flag & ClearFlag.DepthBuffer) != 0 && Attachment.DepthBuffer != null)
                Attachment.DepthBuffer.CopyFrom(this.clearedDepthBuff);
            if ((flag & ClearFlag.StencilBuffer) != 0 && Attachment.StencilBuffer != null)
                Attachment.StencilBuffer.CopyFrom(this.clearedStencilBuff);
        }

        public void AttachColor(Texture2D v, int localtion = 0) => 
            Attachment.ColorBuffer[localtion] = v.ColorBuffer;
        public void AttachColor(Buffer_Color v, int localtion = 0) =>
            Attachment.ColorBuffer[localtion] = v;
        public void AttachDepth(Buffer_Depth v) =>
            Attachment.DepthBuffer = v;
        public void AttachStencil(Buffer_Stencil v) =>
            Attachment.StencilBuffer = v;

        public void CreateStencil(/*params*/)
        {
            if (this.Attachment.StencilBuffer == null)
                this.Attachment.StencilBuffer = new Buffer_Stencil(W, H);
        }

        public bool DepthTest(ComparisonFunc comp, int x, int y, float depth)
        {
            if (comp == ComparisonFunc.Always) return true;
            else if (comp == ComparisonFunc.Never) return false;
            var buffV = Attachment.DepthBuffer[x, y];
            if (float.IsNaN(buffV)) return true;
            switch (comp)
            {
                case ComparisonFunc.LEqual: return depth <= buffV;
                case ComparisonFunc.GEqual: return depth >= buffV;
                case ComparisonFunc.Equal: return depth == buffV;
                case ComparisonFunc.NotEqual: return depth != buffV;
                case ComparisonFunc.Less: return depth < buffV;
                case ComparisonFunc.Greater: return depth > buffV;
                default: throw new Exception("Not implements");
            }
        }

        public bool StencilTest(ComparisonFunc comp, int x, int y, byte refV, byte readkMask)
        {
            if (comp == ComparisonFunc.Always) return true;
            else if (comp == ComparisonFunc.Never) return false;
            var buffV = Attachment.StencilBuffer[x, y] & readkMask;
            switch (comp)
            {
                case ComparisonFunc.LEqual: return refV <= buffV;
                case ComparisonFunc.GEqual: return refV >= buffV;
                case ComparisonFunc.Equal: return refV == buffV;
                case ComparisonFunc.NotEqual: return refV != buffV;
                case ComparisonFunc.Less: return refV < buffV;
                case ComparisonFunc.Greater: return refV > buffV;
                default: throw new Exception("Not implements");
            }
        }

        public void StencilOpHandle(StencilOp op, int x, int y, byte refV, byte writeMask)
        {
            if (op == StencilOp.Keep)/*noops*/ return;
            if(op == StencilOp.Zero) Attachment.StencilBuffer[x, y] = 0;

            var buff = Attachment.StencilBuffer;
            
            switch (op)
            {
                case StencilOp.Replace:
                    buff[x, y] = (byte)(refV & writeMask);
                    break;
                case StencilOp.Incr:
                    buff[x, y] = (byte)Math.Max((buff[x, y] + 1), 0);
                    break;
                case StencilOp.Decr:
                    buff[x, y] = (byte)Math.Min((buff[x, y] - 1), byte.MaxValue);
                    break;
                case StencilOp.Invert:
                    buff[x, y] = (byte)(~(buff[x, y]));
                    break;
                case StencilOp.Incrwrap:
                    {
                        var buffV = buff[x, y] + 1;
                        if (buffV > byte.MaxValue) buff[x, y] = 0;
                        else  buff[x, y] = (byte)buffV;
                    }
                    break;
                case StencilOp.Decrwrap:
                    {
                        var buffV = buff[x, y] - 1;
                        if (buffV < 0) buff[x, y] = byte.MaxValue;
                        else buff[x, y] = (byte)buffV;
                    }
                    break;
                default:throw new Exception($"not implements");
            }
        }

        public void WriteColor(int localtion, int x, int y, Vector4 color, ColorMask colorMask)
        {
            if (colorMask == ColorMask.None)
                return;

            if (colorMask != ColorMask.RGBA)
            {
                if ((colorMask & ColorMask.R) == 0)
                    color.r = 0;
                if ((colorMask & ColorMask.G) == 0)
                    color.g = 0;
                if ((colorMask & ColorMask.B) == 0)
                    color.b = 0;
                if ((colorMask & ColorMask.A) == 0)
                    color.a = 0;
            }

            color.Clamp();
            Attachment.ColorBuffer[localtion].Set(x, y, color);
        }

        public void Dispose()
        {
            
        }
    }

    [Description("顶点数据格式")]
    public struct VertexDataFormat
    {
        public VertexDataType type;
        public byte location;
        public int offset;
        public int count;
    }

    [Description("顶点缓存")]
    public class VertexBuffer : IDisposable
    {
        public int count;
        public int floatNumPerVertice;

        public float[] buff; // 这儿懒得写IntPtr来控制byte的方式了

        public int writePos;

        public VertexDataFormat[] Formats { get; private set; }

        public VertexBuffer(int count, int floatNumPerVertic, float[] srcBuff = null, bool copySrcBuff = false)
        {
            this.count = count;
            this.floatNumPerVertice = floatNumPerVertic;

            if (srcBuff != null)
            {
                if (copySrcBuff)
                {
                    this.buff = new float[count];
                    Array.Copy(srcBuff, this.buff, count);
                }
                else this.buff = srcBuff;
            }
            else
            {
                this.buff = new float[count];
            }
        }

        public void SetFormat(VertexDataFormat[] formats)
        {
            Formats = formats;
        }

        public void Write(float value)
        {
            buff[writePos++] = value;
        }

        public void Write(float[] value, int valueSrcIdx, int valueCount)
        {
            Array.Copy(value, valueSrcIdx, buff, writePos, valueCount);
            writePos += valueCount;
        }

        public void Write(Vector2 v)
        {
            buff[writePos++] = v.x;
            buff[writePos++] = v.y;
        }

        public void Write(Vector3 v)
        {
            buff[writePos++] = v.x;
            buff[writePos++] = v.y;
            buff[writePos++] = v.z;
        }

        public void Write(Vector4 v)
        {
            buff[writePos++] = v.x;
            buff[writePos++] = v.y;
            buff[writePos++] = v.z;
            buff[writePos++] = v.w;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (buff != null)
            {
                Array.Resize(ref buff, 0);
                buff = null;
            }
        }
    }

    [Description("索引缓存")]
    public class IndexBuffer : IDisposable
    {
        public int[] Buffer { get; private set; }

        public IndexBuffer(int count)
        {
            Buffer = new int[count];
        }

        public void Set(int[] srcBuff, int srcBuffStarIdx = 0, int srcBuffCount = 0)
        {
            if (srcBuffCount == 0) srcBuffCount = srcBuff.Length;
            Array.Copy(srcBuff, srcBuffStarIdx, Buffer, 0, srcBuffCount);
        }

        public void Dispose()
        {
            Buffer = null;
        }
    }
}
