// jave.lin 2019.08.06
using SoftRenderer.Common.Mathes;
using System;
using System.ComponentModel;

namespace SoftRenderer.SoftRenderer
{
    public class Buffer_Generic<T> : IDisposable
    {
        private T[,] buff;
        public T this[int x, int y] { get => buff[x, y]; set => buff[x, y] = value; }
        public int Len { get => buff.Length; }
        public Buffer_Generic(int w, int h)
        {
            buff = new T[w, h];
        }
        public void SetAll(T v)
        {
            var xLen = buff.GetLength(0);
            var yLen = buff.GetLength(1);
            for (int x = 0; x < xLen; x++)
            {
                for (int y = 0; y < yLen; y++)
                {
                    buff[x, y] = v;
                }
            }
        }
        public void CopyTo(Buffer_Generic<T> to)
        {
            Array.Copy(this.buff, to.buff, this.buff.Length);
        }
        public void CopyFrom(Buffer_Generic<T> from)
        {
            Array.Copy(from.buff, this.buff, from.buff.Length);
        }
        public void Set(int x, int y, T color)
        {
            buff[x, y] = color;
        }
        public T Get(int x, int y)
        {
            return buff[x, y];
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            buff = null;
        }
    }
    public class Buffer_Color : Buffer_Generic<Vector4>
    {
        public Buffer_Color(int w, int h) : base(w, h) { }
    }
    public class Buffer_Depth : Buffer_Generic<float>
    {
        public Buffer_Depth(int w, int h) : base(w, h) { }
    }
    public class Buffer_Stencil : Buffer_Generic<byte>
    {
        public Buffer_Stencil(int w, int h) : base(w, h) { }
    }

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
            get=> clearedColor;
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
            get => ClearedDepth;
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
            get => ClearedStencil;
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
            if ((flag & ClearFlag.DepthBuffer) != 0 && Attachment.StencilBuffer != null)
                Attachment.StencilBuffer.CopyFrom(this.clearedStencilBuff);
        }
        public void AttachColor(Texture v, int localtion = 0) => 
            Attachment.ColorBuffer[localtion] = v.ColorBuffer;
        public void AttachColor(Buffer_Color v, int localtion = 0) =>
            Attachment.ColorBuffer[localtion] = v;
        public void AttachDepth(Buffer_Depth v) =>
            Attachment.DepthBuffer = v;
        public void AttachStencil(Buffer_Stencil v) =>
            Attachment.StencilBuffer = v;

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

        public void WriteColor(int localtion, int x, int y, Vector4 color)
        {
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
