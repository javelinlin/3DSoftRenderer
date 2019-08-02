// jave.lin 2019.07.15
using System;
using System.Drawing;

namespace SoftRenderer.Common.Mathes
{
    public struct ColorNormalized : IConvert<ColorNormalized>
    {
        public static ColorNormalized zero = new ColorNormalized(0, 0, 0, 0);
        public static ColorNormalized black = new ColorNormalized(0, 0, 0, 1);
        public static ColorNormalized white = new ColorNormalized(1, 1, 1, 1);
        public static ColorNormalized red = new ColorNormalized(1, 0, 0, 1);
        public static ColorNormalized green = new ColorNormalized(0, 1, 0, 1);
        public static ColorNormalized blue = new ColorNormalized(0, 0, 1, 1);
        public static ColorNormalized yellow = new ColorNormalized(1, 1, 0, 1);
        public static ColorNormalized pink = new ColorNormalized(1, 0, 1, 1);
        public static ColorNormalized purple = new ColorNormalized(90f/255f, 0, 0.5f, 1);
        public static ColorNormalized gray = new ColorNormalized(0.5f, 0.5f, 0.5f, 1);

        public static ColorNormalized lightRed = new ColorNormalized(1, 0.5f, 0.5f, 1);
        public static ColorNormalized lightGreen = new ColorNormalized(0.5f, 1, 0.5f, 1);
        public static ColorNormalized lightBlue = new ColorNormalized(0, 1, 1, 1);

        public static ColorNormalized darkRed = new ColorNormalized(0.5f, 0, 0, 1);
        public static ColorNormalized darkGreen = new ColorNormalized(0, 0.5f, 0, 1);
        public static ColorNormalized darkBlue = new ColorNormalized(0, 0.5f, 0.5f, 1);


        public static void CopyRGBFrom(ColorNormalized from, ColorNormalized to)
        {
            to.r = from.r;
            to.g = from.g;
            to.b = from.b;
        }

        public float r, g, b, a;
        public Vector3 rgb
        {
            get => new Vector3(r, g, b);
            set
            {
                r = value.x;
                g = value.y;
                b = value.z;
            }
        }
        public ColorNormalized(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }

        public ColorNormalized(Color c)
        {
            this.r = c.R / 255f;
            this.g = c.G / 255f;
            this.b = c.B / 255f;
            this.a = c.A / 255f;
        }

        public void CopyRGBFrom(ColorNormalized other)
        {
            CopyRGBFrom(other, this);
        }
        public void Clamp()
        {
            if (r < 0) r = 0;
            if (r > 1) r = 1;
            if (g < 0) g = 0;
            if (g > 1) g = 1;
            if (b < 0) b = 0;
            if (b > 1) b = 1;
            if (a < 0) a = 0;
            if (a > 1) a = 1;
        }

        public static implicit operator ColorNormalized(Color c)
        {
            return new ColorNormalized(c);
        }
        public static implicit operator Color(ColorNormalized c)
        {
            return Color.FromArgb((byte)(c.a * 255), (byte)(c.r * 255), (byte)(c.g * 255), (byte)(c.b * 255));
        }
        public static bool operator ==(ColorNormalized a, ColorNormalized b)
        {
            return a.r == b.r && a.g == b.g && a.b == b.b && a.a == b.a;
        }
        public static bool operator !=(ColorNormalized a, ColorNormalized b)
        {
            return a.r != b.r || a.g != b.g || a.b != b.b || a.a != b.a;
        }
        public static ColorNormalized operator -(ColorNormalized a, ColorNormalized b)
        {
            return new ColorNormalized(a.r - b.r, a.g - b.g, a.b - b.b, a.a - b.a);
        }
        public static ColorNormalized operator -(ColorNormalized a, float rgba)
        {
            return new ColorNormalized(a.r - rgba, a.g - rgba, a.b - rgba, a.a - rgba);
        }
        public static ColorNormalized operator +(ColorNormalized a, ColorNormalized b)
        {
            return new ColorNormalized(a.r + b.r, a.g + b.g, a.b + b.b, a.a + b.a);
        }
        public static ColorNormalized operator +(ColorNormalized a, float rgba)
        {
            return new ColorNormalized(a.r + rgba, a.g + rgba, a.b + rgba, a.a + rgba);
        }
        public static ColorNormalized operator +(Vector4 v, ColorNormalized a)
        {
            return new ColorNormalized(a.r + v.x, a.g + v.y, a.b + v.z, a.a + v.w);
        }
        public static ColorNormalized operator +(ColorNormalized a, Vector4 v)
        {
            return new ColorNormalized(a.r + v.x, a.g + v.y, a.b + v.z, a.a + v.w);
        }
        public static ColorNormalized operator +(Vector3 v, ColorNormalized a)
        {
            return new ColorNormalized(a.r + v.x, a.g + v.y, a.b + v.z, a.a);
        }
        public static ColorNormalized operator +(ColorNormalized a, Vector3 v)
        {
            return new ColorNormalized(a.r + v.x, a.g + v.y, a.b + v.z, a.a);
        }
        public static ColorNormalized operator *(ColorNormalized a, ColorNormalized b)
        {
            return new ColorNormalized(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
        }
        public static ColorNormalized operator *(ColorNormalized a, float rgba)
        {
            return new ColorNormalized(a.r * rgba, a.g * rgba, a.b * rgba, a.a * rgba);
        }
        public static ColorNormalized operator *(float rgba, ColorNormalized a)
        {
            return new ColorNormalized(a.r * rgba, a.g * rgba, a.b * rgba, a.a * rgba);
        }
        public static ColorNormalized operator *(Vector4 v, ColorNormalized a)
        {
            return new ColorNormalized(a.r * v.x, a.g * v.y, a.b * v.z, a.a * v.w);
        }
        public static ColorNormalized operator *(ColorNormalized a, Vector4 v)
        {
            return new ColorNormalized(a.r * v.x, a.g * v.y, a.b * v.z, a.a * v.w);
        }
        public static ColorNormalized operator *(Vector3 v, ColorNormalized a)
        {
            return new ColorNormalized(a.r * v.x, a.g * v.y, a.b * v.z, a.a);
        }
        public static ColorNormalized operator *(ColorNormalized a, Vector3 v)
        {
            return new ColorNormalized(a.r * v.x, a.g * v.y, a.b * v.z, a.a);
        }
        public static ColorNormalized operator /(ColorNormalized a, ColorNormalized b)
        {
            return new ColorNormalized(a.r / b.r, a.g / b.g, a.b / b.b, a.a / b.a);
        }
        public static ColorNormalized operator /(ColorNormalized a, float rgba)
        {
            return new ColorNormalized(a.r / rgba, a.g / rgba, a.b / rgba, a.a / rgba);
        }
        public static implicit operator ColorNormalized(float rgba)
        {
            return new ColorNormalized(rgba, rgba, rgba, rgba);
        }
        public static implicit operator ColorNormalized(ColorRGBNormalized c)
        {
            return new ColorNormalized(c.r, c.g, c.b, 1);
        }
        public static implicit operator ColorNormalized(Vector4 v)
        {
            return new ColorNormalized(v.x, v.y, v.z, v.w);
        }
        public override string ToString()
        {
            return $"r:{r.ToString("0.000")}, g:{g.ToString("0.000")}, b:{b.ToString("0.000")}, a:{a.ToString("0.000")}";
        }

        public void ConvertFrom(string str)
        {
            var arr = str.Split(Utils.convert_spliter);
            r = Convert.ToSingle(arr[0]);
            g = Convert.ToSingle(arr[1]);
            b = Convert.ToSingle(arr[2]);
            a = Convert.ToSingle(arr[3]);
        }
    }

    public struct ColorRGBNormalized
    {
        public static ColorRGBNormalized zero = new ColorRGBNormalized(0, 0, 0);
        public static void CopyFrom(ColorRGBNormalized from, ColorRGBNormalized to)
        {
            to.r = from.r;
            to.g = from.g;
            to.b = from.b;
        }

        public float r, g, b;

        public ColorRGBNormalized(float r, float g, float b)
        {
            this.r = r;
            this.g = g;
            this.b = b;
        }

        public ColorRGBNormalized(Color c)
        {
            this.r = c.R / 255f;
            this.g = c.G / 255f;
            this.b = c.B / 255f;
        }
        public ColorRGBNormalized(ColorNormalized c)
        {
            this.r = c.r;
            this.g = c.g;
            this.b = c.b;
        }
        public void CopyFrom(ColorRGBNormalized other)
        {
            CopyFrom(other, this);
        }
        public static implicit operator ColorRGBNormalized(Color c)
        {
            return new ColorRGBNormalized(c);
        }
        public static implicit operator Color(ColorRGBNormalized c)
        {
            return Color.FromArgb(255, (byte)(c.r * 255), (byte)(c.g * 255), (byte)(c.b * 255));
        }
        public static bool operator ==(ColorRGBNormalized a, ColorRGBNormalized b)
        {
            return a.r == b.r && a.g == b.g && a.b == b.b;
        }
        public static bool operator !=(ColorRGBNormalized a, ColorRGBNormalized b)
        {
            return a.r != b.r || a.g != b.g || a.b != b.b;
        }
        public static ColorRGBNormalized operator -(ColorRGBNormalized a, ColorRGBNormalized b)
        {
            return new ColorRGBNormalized(a.r - b.r, a.g - b.g, a.b - b.b);
        }
        public static ColorRGBNormalized operator -(ColorRGBNormalized a, float rgb)
        {
            return new ColorRGBNormalized(a.r - rgb, a.g - rgb, a.b - rgb);
        }
        public static ColorRGBNormalized operator +(ColorRGBNormalized a, ColorRGBNormalized b)
        {
            return new ColorRGBNormalized(a.r + b.r, a.g + b.g, a.b + b.b);
        }
        public static ColorRGBNormalized operator +(ColorRGBNormalized a, float rgb)
        {
            return new ColorRGBNormalized(a.r + rgb, a.g + rgb, a.b + rgb);
        }
        public static ColorRGBNormalized operator *(ColorRGBNormalized a, ColorRGBNormalized b)
        {
            return new ColorRGBNormalized(a.r * b.r, a.g * b.g, a.b * b.b);
        }
        public static ColorRGBNormalized operator *(ColorRGBNormalized a, float rgb)
        {
            return new ColorRGBNormalized(a.r * rgb, a.g * rgb, a.b * rgb);
        }
        public static ColorRGBNormalized operator /(ColorRGBNormalized a, ColorRGBNormalized b)
        {
            return new ColorRGBNormalized(a.r / b.r, a.g / b.g, a.b / b.b);
        }
        public static ColorRGBNormalized operator /(ColorRGBNormalized a, float rgb)
        {
            return new ColorRGBNormalized(a.r / rgb, a.g / rgb, a.b / rgb);
        }
        public static implicit operator ColorRGBNormalized(Vector3 rgb)
        {
            return new ColorRGBNormalized(rgb.x, rgb.y, rgb.z);
        }
        public static implicit operator ColorRGBNormalized(ColorNormalized rgba)
        {
            return new ColorRGBNormalized(rgba.r, rgba.g, rgba.b);
        }
        public static implicit operator ColorRGBNormalized(float rgb)
        {
            return new ColorRGBNormalized(rgb, rgb, rgb);
        }
        public override string ToString()
        {
            return $"r:{r.ToString("0.000")}, g:{g.ToString("0.000")}, b:{b.ToString("0.000")}";
        }
    }
}
