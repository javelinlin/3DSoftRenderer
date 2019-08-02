// jave.lin 2019.07.14

using RendererCommon.SoftRenderer.Common.Shader;
using SoftRenderer.Common.Mathes;
using System.Collections.Generic;
using System.ComponentModel;

namespace SoftRenderer.SoftRenderer.Primitives
{
    [Description("3D的三角面")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct Triangle
    {
        public Vector3 p0;
        public Vector3 p1;
        public Vector3 p2;

        public Triangle(Vector3 p0, Vector3 p1, Vector3 p2)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
        }
        public Triangle(List<Vector3> pos)
        {
            this.p0 = pos[0];
            this.p1 = pos[1];
            this.p2 = pos[2];
        }
        public Triangle(List<Vector3> pos, int startIdx)
        {
            this.p0 = pos[startIdx];
            this.p1 = pos[startIdx + 1];
            this.p2 = pos[startIdx + 2];
        }
        public Triangle(Vector3[] pos)
        {
            this.p0 = pos[0];
            this.p1 = pos[1];
            this.p2 = pos[2];
        }
        public Triangle(Vector3[] pos, int startIdx)
        {
            this.p0 = pos[startIdx];
            this.p1 = pos[startIdx + 1];
            this.p2 = pos[startIdx + 2];
        }
        bool SameSide(Vector3 A, Vector3 B, Vector3 C, Vector3 P)
        {
            Vector3 AB = B - A;
            Vector3 AC = C - A;
            Vector3 AP = P - A;

            Vector3 v1 = AB.Cross(AC);
            Vector3 v2 = AB.Cross(AP);

            // v1 and v2 should point to the same direction
            return v1.Dot(v2) >= 0;
        }
        //https://blog.csdn.net/wkl115211/article/details/80215421
        public bool Contain(Vector3 p)
        {
            return SameSide(p0, p1, p2, p) &&
                SameSide(p1, p2, p0, p) &&
                SameSide(p2, p0, p1, p);
        }

        public bool Validated() // 验证三角形是否有效，在光栅时，或是剔除渲染时使用
        {
            if (this.p0 == this.p1)
            {
                return false;
            } // 共线
            if (this.p1 == this.p2)
            {
                return false;
            } // 共线
            if (this.p2 == this.p0)
            {
                return false;
            } // 共线
            // p0-p1 斜率
            var m1 = (this.p0.y - this.p1.y) / (this.p0.x - p1.x);
            // p0-p2 斜率
            var m2 = (this.p0.y - this.p2.y) / (this.p0.x - p2.x);
            // p1-p2 斜率
            var m3 = (this.p1.y - this.p2.y) / (this.p1.x - p2.x);
            return m1 == m2 || m1 == m3 || m2 == m3 ? false : true;  // 共线
        }

        public override string ToString()
        {
            return $"p0:{p0}, p1:{p1}, p2:{p2}";
        }
    }

    [Description("2D的三角形")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct Triangle2D
    {
        public static implicit operator Triangle(Triangle2D t)
        {
            return new Triangle(t.p0, t.p1, t.p2);
        }
        public static implicit operator Triangle2D(Triangle t)
        {
            return new Triangle2D(t.p0, t.p1, t.p2);
        }

        public Vector2 p0;
        public Vector2 p1;
        public Vector2 p2;

        public Triangle2D(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;
        }

        bool SameSide(Vector2 a, Vector2 b, Vector2 c, Vector2 p)
        {
            Vector3 A = a;
            Vector3 B = b;
            Vector3 C = c;
            Vector3 P = p;
            Vector3 AB = B - A;
            Vector3 AC = C - A;
            Vector3 AP = P - A;

            Vector3 v1 = AB.Cross(AC);
            Vector3 v2 = AB.Cross(AP);

            // v1 and v2 should point to the same direction
            return v1.Dot(v2) >= 0;
        }
        //https://blog.csdn.net/wkl115211/article/details/80215421
        public bool Contain(Vector2 p)
        {
            return SameSide(p0, p1, p2, p) &&
                SameSide(p1, p2, p0, p) &&
                SameSide(p2, p0, p1, p);
        }

        // 自己写的，利用2D向量叉乘
        // https://blog.csdn.net/linjf520/article/details/78718520
        public bool ContainExt(Vector2 p)
        {
            var segmentP0TP1 = p1 - p0;
            var segmentP0TP2 = p2 - p0;

            var crossV = segmentP0TP1.Cross(segmentP0TP2);
            if (crossV == 0) return false; // 三点共线了，无效三角形

            var segmentP0TP = p - p0;
            if (crossV < 0) // 逆时针， p0Tp1在p0Tp2的右手边，p0TP2在p0Tp1的左手边
            {
                // 先确定：p-p0，在p1-p0的左手边
                crossV = segmentP0TP1.Cross(segmentP0TP);
                if (crossV >= 0) return false; // 右边、或共线
                // 再确定：p-p0，在p2-p0的右手边
                crossV = segmentP0TP2.Cross(segmentP0TP);
                if (crossV <= 0) return false; // 左边、或共线
                // 再确定：p-p1，在p2-p1的左手边
                crossV = (p2 - p1).Cross(p - p1);
                if (crossV >= 0) return false; // 右边、或共线
                return true;
            }
            else // 顺时针， 反向位置: // p0Tp1在p0Tp2的左手边，p0TP2在p0Tp1的右手边
            {
                // 先确定：p-p0，在p1-p0的右手边
                crossV = segmentP0TP1.Cross(segmentP0TP);
                if (crossV <= 0) return false; // 左边、或共线
                // 再确定：p-p0，在p2-p0的左手边
                crossV = segmentP0TP2.Cross(segmentP0TP);
                if (crossV >= 0) return false; // 右边、或共线
                // 在确定：p-p1，在p2-p1的右手边
                crossV = (p2 - p1).Cross(p - p1);
                if (crossV <= 0) return false; // 左边、或共线
                return true;
            }
        }

        public bool Validated() // 验证三角形是否有效，在光栅时，或是剔除渲染时使用
        {
            if (this.p0 == this.p1)
            {
                return false;
            } // 共线
            if (this.p1 == this.p2)
            {
                return false;
            } // 共线
            if (this.p2 == this.p0)
            {
                return false;
            } // 共线
            var m1 = (this.p0.y - this.p1.y) / (this.p0.x - p1.x);
            var m2 = (this.p0.y - this.p2.y) / (this.p0.x - p2.x);
            return m1 == m2 ? false : true;  // 共线
        }

        public override string ToString()
        {
            return $"p0:{p0}, p1:{p1}, p2:{p2}";
        }
    }

    [Description("三角图元")]
    public struct Primitive_Triangle
    {
        public FragInfo f0;
        public FragInfo f1;
        public FragInfo f2;

        public Primitive_Triangle(FragInfo f0, FragInfo f1, FragInfo f2)
        {
            this.f0 = f0;
            this.f1 = f1;
            this.f2 = f2;
        }

        public bool Validated() // 验证三角形是否有效，在光栅时，或是剔除渲染时使用
        {
            if (this.f0.p == this.f1.p)
            {
                return false;
            } // 共线
            if (this.f1.p == this.f2.p)
            {
                return false;
            } // 共线
            if (this.f2.p == this.f0.p)
            {
                return false;
            } // 共线
            // p0-p1 斜率
            var m1 = (this.f0.p.y - this.f1.p.y) / (this.f0.p.x - f1.p.x);
            // p0-p2 斜率
            var m2 = (this.f0.p.y - this.f2.p.y) / (this.f0.p.x - f2.p.x);
            // p1-p2 斜率
            var m3 = (this.f1.p.y - this.f2.p.y) / (this.f1.p.x - f2.p.x);
            return m1 == m2 || m1 == m3 || m2 == m3 ? false : true;  // 共线
        }
    }

    [Description("线图元")]
    public struct Primitive_Line
    {
        public FragInfo f1;
        public FragInfo f2;

        public Primitive_Line(FragInfo f1, FragInfo f2)
        {
            this.f1 = f1;
            this.f2 = f2;
        }
    }

    [Description("点图元")]
    public struct Primitive_Point
    {
        public FragInfo f1;

        public Primitive_Point(FragInfo f1)
        {
            this.f1 = f1;
        }
    }
}

