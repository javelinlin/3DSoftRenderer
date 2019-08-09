using RendererCoreCommon.Renderer.Common.Mathes;
using System;

namespace RendererCoreCommon.Renderer.Common.Shader
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
}
