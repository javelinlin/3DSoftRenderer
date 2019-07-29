// jave.lin 2019.07.18
using SoftRenderer.Common.Mathes;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace SoftRenderer.SoftRenderer
{
    public class ColorBuffer : IDisposable
    {
        private const float ToNC = 1 / 255f;
#if BUFF_RGBA
        private const PixelFormat pf = PixelFormat.Format32bppArgb;
        private const int numOfPerColor = 4;
#else
        private const PixelFormat pf = PixelFormat.Format24bppRgb;
        private const int numOfPerColor = 3;
#endif

        private Bitmap buffer;
        private Rectangle defaultRect;
        private ColorNormalized clearedColor = new ColorNormalized(.5f, .5f, .5f, 1);
        private bool clearedColorDirty = true;
        private byte[] clearedBGRBuff;
        private int width;

        public ColorNormalized ClearedColor
        {
            get => clearedColor;
            set
            {
                if (clearedColor != value)
                {
                    clearedColor = value;
                    clearedColorDirty = true;
                }
            }
        }

        public ColorBuffer(int w, int h)
        {
            width = w;
            buffer = new Bitmap(w, h, pf);
            defaultRect = new Rectangle(0, 0, w, h);

            var bmd = buffer.LockBits(defaultRect, ImageLockMode.ReadWrite, buffer.PixelFormat);
            buffer.UnlockBits(bmd);
            clearedBGRBuff = new byte[Math.Abs(bmd.Stride) * buffer.Height];
        }

        public void Clear()
        {
            if (clearedColorDirty)
            {
                clearedColorDirty = false;
                Color c = ClearedColor;
                for (int i = 0; i < clearedBGRBuff.Length; i += numOfPerColor)
                {
                    clearedBGRBuff[i] = c.B;
                    clearedBGRBuff[i + 1] = c.G;
                    clearedBGRBuff[i + 2] = c.R;
#if BUFF_RGBA
                    clearedBGRBuff[i + 3] = c.A;
#endif
                }
            }
            var bmd = Begin();
            Marshal.Copy(clearedBGRBuff, 0, bmd.Scan0, clearedBGRBuff.Length);
            End(bmd);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (clearedBGRBuff != null)
            {
                Array.Resize(ref clearedBGRBuff, 0);
                clearedBGRBuff = null;
            }

            if (buffer != null)
            {
                buffer.Dispose();
                buffer = null;
            }
        }

        public BitmapData Begin()
        {
            return buffer.LockBits(defaultRect, ImageLockMode.ReadWrite, buffer.PixelFormat);
        }

        public void BeginSetPixel(IntPtr ptr, int x, int y, ColorNormalized color)
        {
            var offset = (x + y * width) * numOfPerColor;
            Marshal.WriteByte(ptr + offset, (byte)(color.b * 255));
            Marshal.WriteByte(ptr + offset + 1, (byte)(color.g * 255));
            Marshal.WriteByte(ptr + offset + 2, (byte)(color.r * 255));
#if BUFF_RGBA
            //Marshal.WriteByte(ptr, offset + 3, (byte)(color.a * 255));
            Marshal.WriteByte(ptr + offset + 3, (byte)(color.a * 255));
#endif
        }

        public ColorNormalized BeginRead(IntPtr ptr, int x, int y)
        {
            var offset = (x + y * width) * numOfPerColor;
            var b = Marshal.ReadByte(ptr, offset);
            var g = Marshal.ReadByte(ptr, offset + 1);
            var r = Marshal.ReadByte(ptr, offset + 2);
#if BUFF_RGBA
            var a = Marshal.ReadByte(ptr, offset + 3);
            return new ColorNormalized(r * ToNC, g * ToNC, b * ToNC, a * ToNC);
#else
            return new ColorNormalized(r * ToNC, g * ToNC, b * ToNC, 1);
#endif
        }

        public void End(BitmapData bmd)
        {
            buffer.UnlockBits(bmd);
        }

        public void SetPixel(int x, int y, ColorNormalized color)
        {
            buffer.SetPixel(x, y, color);
        }

        public static implicit operator Bitmap(ColorBuffer buff)
        {
            return buff.buffer;
        }
    }
}
