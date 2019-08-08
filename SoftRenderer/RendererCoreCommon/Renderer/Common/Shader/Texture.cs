// jave.lin 2019.07.24
using RendererCoreCommon.Renderer.Common.Mathes;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RendererCoreCommon.Renderer.Common.Shader
{
    [Description("纹理目标类型")]
    public enum TextureTarget // Texture Type
    {
        T1D,                        // 1维纹理，通常只有长度，或是宽度
        T2D,                        // 2维纹理
        T3D,                        // 3维纹理
        T1D_Array,                  // 1维纹理的数组，通常就是一个2维纹理，纵向数量就是1维的数组长度
        T2D_Array,                  // 2维纹理的数据，通常就是一个3维纹理，深度数量就是2维的数据长度
        Rect,                       // 就是一个纹理的矩形内的纹理，没有mipmap，部分wrap模式会失效
        Buff,                       // 就是一个1维缓存数据对象，但可以在任意的着色器阶段可以访问到
        CubeMap,                    // 立方体纹理，由6张纹理组合而成，通常用于天空盒、或是环境映射
        CubeMap_Array,              // 立方体纹理的数组，通常就是一个3维纹理，深度值数量必须是6的倍数整数值
        T2D_Multisample,            // 2维纹理的多重采样纹理
        T2D_Multisample_Array,      // 2维纹理的多重采样纹理数组
    }

    [Description("纹理")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Texture2D : IDisposable
    {
        private Vector4[,] buffer;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Texture2D(string file = null)
        {
            if (!string.IsNullOrEmpty(file))
            {
                using(var bmp = new Bitmap(file))
                    Set(bmp);
            }
        }
        public Texture2D(Bitmap bmp = null)
        {
            Set(bmp);
        }
        public void Set(Bitmap bmp)
        {
            Width = bmp.Width;
            Height = bmp.Height;

            buffer = new Vector4[Width, Height];

            var bmd = bmp.LockBits(new Rectangle(0, 0, Width, Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            var ptr = bmd.Scan0;
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    var offset = (x + y * Width) * 4;
                    var b = Marshal.ReadByte(ptr, offset) / 255f;
                    var g = Marshal.ReadByte(ptr, offset + 1) / 255f;
                    var r = Marshal.ReadByte(ptr, offset + 2) / 255f;
                    var a = Marshal.ReadByte(ptr, offset + 3) / 255f;
                    buffer[x, y] = Vector4.Get(r, g, b, a);
                }
            }
            bmp.UnlockBits(bmd);
        }
        public Vector4 Get(int x, int y)
        { // 获取纹素 texel
            return buffer[x, y];
        }
        public void Dispose()
        {
            GC.SuppressFinalize(this);

            buffer = null;
        }
    }

    [Description("采样器过滤模式")]
    public enum SampleFilterMode
    {
        Point,              // 点采样，直接按UV坐标换算成对应的纹素坐标就采样
        Linear,             // 线性过滤
        Trilinear,          // 三次线性过滤
    }

    [Flags]
    [Description("纹理坐标包裹模式（纹理坐标超过1，或小于0时，如何处理这些边界坐标）")]
    public enum SampleWrapMode
    {
        Clamp = 1,              // Clamp X,Y
        Repeat = 2,             // Repeat X,Y
        Mirror = 4,             // Mirror X,Y
        MirrorOnce = 8,         // Mirror Once X,Y
        ClampX = 16,
        ClampY = 32,
        RepeatX = 64,
        RepeatY = 128,
        MirrorX = 256,
        MirrorY = 512,
        MirrorOnceX = 1024,
        MirrorOnceY = 2048,
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Description("采样器")]
    public struct Sampler2D
    {
        public SampleFilterMode filterMode;
        public SampleWrapMode wrapMode;

        public Vector4 Sample(Texture2D tex, float u, float v)
        {
            Wrap(ref u, ref v);

            var x = u * (tex.Width - 1);
            var y = (1 - v) * (tex.Height - 1);

            return Filter(tex, x, y);
        }

        private void Wrap(ref float u, ref float v)
        {
            WrapU(ref u);
            WrapV(ref v);
        }

        private void WrapU(ref float u)
        {
            if ((wrapMode & SampleWrapMode.Clamp) != 0)
            {
                u = Mathf.Clamp(u, 0, 1);
            }
            else if ((wrapMode & SampleWrapMode.Repeat) != 0)
            {
                u %= 1;
                if (u < 0) u = 1 + u;
            }
            else if ((wrapMode & SampleWrapMode.Mirror) != 0)
            {
                var i = (int)u;
                u %= 1;
                if (u > 0)
                {
                    if (i % 2 == 0)
                    {
                        // noops
                    }
                    else
                    {
                        u = 1 - u;
                    }
                }
                else
                {
                    if (i % 2 == 0)
                    {
                        u = -u;
                    }
                    else
                    {
                        u = -u;
                        u = 1 - u;
                    }
                }
            }
            else if ((wrapMode & SampleWrapMode.MirrorOnce) != 0)
            {
                u = Mathf.Clamp(u, -1, 1);
                if (u < 0) u = -u;
            }
            else if ((wrapMode & SampleWrapMode.ClampX) != 0)
            {
                u = Mathf.Clamp(u, 0, 1);
            }
            else if ((wrapMode & SampleWrapMode.RepeatX) != 0)
            {
                u %= 1;
                if (u < 0) u = 1 + u;
            }
            else if ((wrapMode & SampleWrapMode.MirrorX) != 0)
            {
                var i = (int)u;
                u %= 1;
                if (u > 0)
                {
                    if (i % 2 == 0)
                    {
                        // noops
                    }
                    else
                    {
                        u = 1 - u;
                    }
                }
                else
                {
                    if (i % 2 == 0)
                    {
                        u = -u;
                    }
                    else
                    {
                        u = -u;
                        u = 1 - u;
                    }
                }
            }
            else if ((wrapMode & SampleWrapMode.MirrorOnceX) != 0)
            {
                u = Mathf.Clamp(u, -1, 1);
                if (u < 0) u = -u;
            }
            else
            {
                u = Mathf.Clamp(u, 0, 1);
            }
        }

        private void WrapV(ref float v)
        {
            if ((wrapMode & SampleWrapMode.Clamp) != 0)
            {
                v = Mathf.Clamp(v, 0, 1);
            }
            else if ((wrapMode & SampleWrapMode.Repeat) != 0)
            {
                v %= 1;
                if (v < 0) v = 1 + v;
            }
            else if ((wrapMode & SampleWrapMode.Mirror) != 0)
            {
                var i = (int)v;
                v %= 1;
                if (v > 0)
                {
                    if (i % 2 == 0)
                    {
                        // noops
                    }
                    else
                    {
                        v = 1 - v;
                    }
                }
                else
                {
                    if (i % 2 == 0)
                    {
                        v = -v;
                    }
                    else
                    {
                        v = -v;
                        v = 1 - v;
                    }
                }
            }
            else if ((wrapMode & SampleWrapMode.MirrorOnce) != 0)
            {
                v = Mathf.Clamp(v, -1, 1);
                if (v < 0) v = -v;
            }
            else if ((wrapMode & SampleWrapMode.ClampY) != 0)
            {
                v = Mathf.Clamp(v, 0, 1);
            }
            else if ((wrapMode & SampleWrapMode.RepeatY) != 0)
            {
                v %= 1;
                if (v < 0) v = 1 + v;
            }
            else if ((wrapMode & SampleWrapMode.MirrorY) != 0)
            {
                var i = (int)v;
                v %= 1;
                if (v > 0)
                {
                    if (i % 2 == 0)
                    {
                        // noops
                    }
                    else
                    {
                        v = 1 - v;
                    }
                }
                else
                {
                    if (i % 2 == 0)
                    {
                        v = -v;
                    }
                    else
                    {
                        v = -v;
                        v = 1 - v;
                    }
                }
            }
            else if ((wrapMode & SampleWrapMode.MirrorOnceY) != 0)
            {
                v = Mathf.Clamp(v, -1, 1);
                if (v < 0) v = -v;
            }
            else
            {
                v = Mathf.Clamp(v, 0, 1);
            }
        }

        private Vector4 Filter(Texture2D tex, float u, float v)
        {
            switch (filterMode)
            {
                case SampleFilterMode.Point:
                    return tex.Get((int)u, (int)v);
                case SampleFilterMode.Linear:
                    throw new Exception($"not implements filter mode:{filterMode}");
                case SampleFilterMode.Trilinear:
                    throw new Exception($"not implements filter mode:{filterMode}");
                default:
                    throw new Exception($"not implements filter mode:{filterMode}");
            }
        }

        public Vector4 Sample(Texture2D tex, Vector2 uv)
        {
            return Sample(tex, uv.x, uv.y);
        }
    }

    [Description("纹理单元，一般都有个数限制，如：一个Shader中最多16个，整个底层可能是80个")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct TextureUnit
    {
        // application 层是
        // 先create texture obj(type, idx, resultObjAddress)
        // 再bind texture(
    }


}
