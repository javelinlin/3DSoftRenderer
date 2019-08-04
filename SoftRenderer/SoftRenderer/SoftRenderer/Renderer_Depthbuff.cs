// jave.lin 2019.07.18
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SoftRenderer.SoftRenderer
{
    [Description("深度缓存")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DepthBuff : IDisposable
    {
        private const int PER_VALUE_SIZE = sizeof(float);
        // https://blog.csdn.net/linjf520/article/details/79677906
        public unsafe static void GetBytes(byte[] result, float value)
        {
            fixed (byte* ptr = &result[0])
            {
                *(float*)ptr = value;
            }
        }
        public unsafe static float ToValue(byte[] buff)
        {
            fixed (byte* ptr = &buff[0]) return *(float*)ptr;
        }

        private IntPtr ptr;
        private int w, h;
        private byte[] readHelper;
        private byte[] clearedBuffer;

        private bool clearedBuffDirty = true;
        private float clearedBuffValue = float.NaN;
        public float ClearedBuffValue
        {
            get => clearedBuffValue;
            set
            {
                if (clearedBuffValue != value)
                {
                    clearedBuffValue = value;
                    clearedBuffDirty = true;
                }
            }
        }

        public DepthBuff(int w, int h)
        {
            this.w = w;
            this.h = h;
            this.ptr = Marshal.AllocHGlobal(w * h * PER_VALUE_SIZE);
            this.readHelper = new byte[PER_VALUE_SIZE];
            this.clearedBuffer = new byte[w * h * PER_VALUE_SIZE];
        }

        public void Clear()
        {
            if (clearedBuffDirty)
            {
                clearedBuffDirty = false;
                GetBytes(readHelper, clearedBuffValue);
                var len = clearedBuffer.Length;
                for (int i = 0; i < len; i += PER_VALUE_SIZE)
                {
                    Array.Copy(readHelper, 0, clearedBuffer, i, PER_VALUE_SIZE);
                }
            }
            Marshal.Copy(clearedBuffer, 0, ptr, clearedBuffer.Length);
        }

        public float TestPickup(int x, int y)
        {
            var offset = (x + y * w) * 4; // float 4 bytes
            readHelper[0] = Marshal.ReadByte(ptr, offset);
            readHelper[1] = Marshal.ReadByte(ptr, offset + 1);
            readHelper[2] = Marshal.ReadByte(ptr, offset + 2);
            readHelper[3] = Marshal.ReadByte(ptr, offset + 3);
            return ToValue(readHelper);
        }

        public bool Test(ComparisonFunc comp, int x, int y, float depth)
        {
            if (comp == ComparisonFunc.Always) return true;
            else if (comp == ComparisonFunc.Never) return false;
            var offset = (x + y * w) * 4; // float 4 bytes
            readHelper[0] = Marshal.ReadByte(ptr, offset);
            readHelper[1] = Marshal.ReadByte(ptr, offset + 1);
            readHelper[2] = Marshal.ReadByte(ptr, offset + 2);
            readHelper[3] = Marshal.ReadByte(ptr, offset + 3);
            var buffV = ToValue(readHelper);
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

        public void Write(int x, int y, float z)
        {
            var offset = (x + y * w) * 4; // float 4 bytes
            GetBytes(readHelper, z);
            Marshal.WriteByte(ptr, offset, readHelper[0]);
            Marshal.WriteByte(ptr, offset + 1, readHelper[1]);
            Marshal.WriteByte(ptr, offset + 2, readHelper[2]);
            Marshal.WriteByte(ptr, offset + 3, readHelper[3]);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            Marshal.FreeHGlobal(ptr);
            if (readHelper != null)
            {
                Array.Resize(ref readHelper, 0);
                readHelper = null;
            }
            if (clearedBuffer != null)
            {
                Array.Resize(ref clearedBuffer, 0);
                clearedBuffer = null;
            }
        }
    }
}
