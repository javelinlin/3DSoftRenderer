// jave.lin 2019.07.18
using SoftRenderer.Common.Mathes;
using System;
using System.ComponentModel;

namespace SoftRenderer.SoftRenderer
{
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

        public void Write(ColorNormalized c)
        {
            buff[writePos++] = c.r;
            buff[writePos++] = c.g;
            buff[writePos++] = c.b;
            buff[writePos++] = c.a;
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
