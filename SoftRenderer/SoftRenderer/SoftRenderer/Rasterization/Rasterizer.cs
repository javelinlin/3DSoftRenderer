// jave.lin 2019.07.15

#define TEMP_FRAG_FILTER // 临时的片段过滤，如果添加了：剪切功能，就不需要这个临时片段剔除了
//#define SPECULAR // 后面再加，因为需要重构DrawTriangle，该接口不该暴露
//#define PERSPECTIVE_CORRECT

using RendererCommon.SoftRenderer.Common.Shader;
using SoftRenderer.Common.Mathes;
using SoftRenderer.SoftRenderer.Primitives;
using SoftRenderer.SoftRenderer.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SoftRenderer.SoftRenderer.Rasterization
{
    // 栅格器
    [Description("栅格器")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Rasterizer : IDisposable
    {
        private Renderer renderer;
        public Renderer Renderer { get => renderer; }

        private Vector3[] usingTiranglePosHehlper = new Vector3[3];
        private Vector3[] allTrianglePosHelper = new Vector3[3];

        private List<FragInfo> fragInfosHelper1 = new List<FragInfo>();
        private List<FragInfo> fragInfosHelper2 = new List<FragInfo>();

        public Rasterizer(Renderer renderer)
        {
            this.renderer = renderer;
        }
        // https://blog.csdn.net/linjf520/article/details/80184578#t4
        // 判断三角面是否正面
        // 参数点都是属于屏幕空间下的坐标
        public bool IsFrontFace(Vector2 p0, Vector2 p1, Vector2 p2)
        {
            var v1 = p1 - p0;
            var v2 = p2 - p0;
            var crossV = v1.Cross(v2);

            if (Renderer.State.FrontFace == FrontFace.Clock)
                return crossV > 0;
            else
                return crossV < 0;
        }
        // 参数点都是属于屏幕空间下的坐标
        // 三角形的点都是属于屏幕空间下的坐标
        public bool IsFrontFace(Triangle t)
        {
            return IsFrontFace(t.p0, t.p1, t.p2);
        }
        public bool IsFrontFace(Primitive_Triangle t)
        {
            return IsFrontFace(t.f0.p, t.f1.p, t.f2.p);
        }
        // 参数点都是属于屏幕空间下的坐标
        public bool CullingTriangle(Vector2 p0, Vector3 p1, Vector3 p2)
        {
            if (Renderer.State.Cull == FaceCull.Off)
                return false;

            var isFront = IsFrontFace(p0, p1, p2);

            if (Renderer.State.Cull == FaceCull.Back)
                return !isFront;

            return isFront;
        }
        // 判断面是否需要剔除
        // 三角形的点都是属于屏幕空间下的坐标
        public bool CullingTriangle(Triangle t)
        {
            return CullingTriangle(t.p0, t.p1, t.p2);
        }
        public bool CullingTriangle(Primitive_Triangle t)
        {
            return CullingTriangle(t.f0.p, t.f1.p, t.f2.p);
        }
        private void GetPos(
            Vector3 p0, Vector3 p1, Vector3 p2,
            out Vector3 tv, out Vector3 mv, out Vector3 bv,
            out bool topSameAsMiddle, out bool bottomSameAsMiddle)
        {
            tv = Vector3.nan;
            mv = Vector3.nan;
            bv = Vector3.nan;
            topSameAsMiddle = false;
            bottomSameAsMiddle = false;

            for (int i = 0; i < 3; i++)
            {
                usingTiranglePosHehlper[i] = Vector3.nan;
            }

            allTrianglePosHelper[0] = p0;
            allTrianglePosHelper[1] = p1;
            allTrianglePosHelper[2] = p2;

            var top_min_y = float.MaxValue;
            var bottom_max_y = float.MinValue;

            // 找出最高，最低点的值
            for (int i = 0; i < 3; i++)
            {
                var pos = allTrianglePosHelper[i];
                if (pos.y < top_min_y)
                {
                    top_min_y = pos.y;
                }
                if (pos.y > bottom_max_y)
                {
                    bottom_max_y = pos.y;
                }
            }

            // 分配最高，最低点
            var usingIdx = 0;
            for (int i = 0; i < 3; i++)
            {
                var pos = allTrianglePosHelper[i];
                var isUsing = false;
                for (int j = 0; j < 3; j++)
                {
                    if (usingTiranglePosHehlper[j] == pos)
                    {
                        isUsing = true;
                        break;
                    }
                }
                if (!isUsing)
                {
                    if (pos.y == top_min_y && tv.AnyNaN())
                    {
                        // 将最高点分配一下，并记录在使用列表中
                        tv = pos;
                        usingTiranglePosHehlper[usingIdx++] = pos;
                    }
                    else if (pos.y == bottom_max_y && bv.AnyNaN())
                    {
                        // 将最底点分配一下，并记录在使用列表中
                        bv = pos;
                        usingTiranglePosHehlper[usingIdx++] = pos;
                    }
                }
            }

            // 最后不在使用列表的就是中部点
            for (int i = 0; i < 3; i++)
            {
                var found = false;
                var pos = allTrianglePosHelper[i];
                for (int j = 0; j < 3; j++)
                {
                    if (usingTiranglePosHehlper[j] == pos)
                    {
                        found = true;
                        break;
                    }
                }
                // 不在使用列表的，那就是中部点了
                if (!found)
                {
                    mv = pos;
                    break;
                }
            }

            if (mv.y == tv.y) topSameAsMiddle = true;
            if (mv.y == bv.y) bottomSameAsMiddle = true;
        }
        // 获取三角形中的底、中、顶部的点信息
        private void GetPos(Triangle t, out Vector3 tv, out Vector3 mv, out Vector3 bv,
            out bool topSameAsMiddle, out bool bottomSameAsMiddle)
        {
            GetPos(t.p0, t.p1, t.p2, out tv, out mv, out bv, out topSameAsMiddle, out bottomSameAsMiddle);
        }
        private void GetPos(Primitive_Triangle t, out Vector3 tv, out Vector3 mv, out Vector3 bv,
            out bool topSameAsMiddle, out bool bottomSameAsMiddle)
        {
            GetPos(t.f0.p, t.f1.p, t.f2.p, out tv, out mv, out bv, out topSameAsMiddle, out bottomSameAsMiddle);
        }
        // 根据两点生成之间的片段：新版管线
        private void GenLineFrag(FragInfo f0, FragInfo f1, List<FragInfo> fs, bool interpolation = true)
        {
            var p0 = f0.p;
            var p1 = f1.p;
            var dir = p1 - p0;
            var dir_nrl = dir.normalized;

            var minX = 0;
            var minY = 0;
            var maxX = renderer.BackBufferWidth - 1;
            var maxY = renderer.BackBufferHeight - 1;

            if (Renderer.State.Scissor == Scissor.On)
            {
                minX = Renderer.State.ScissorRect.X;
                minY = Renderer.State.ScissorRect.Y;
                maxX = Renderer.State.ScissorRect.Right;
                maxY = Renderer.State.ScissorRect.Bottom;
            }

            var p = f0.p;
            if (p.x >= minX && p.x < maxX && p.y >= minY && p.y < maxY)
            {
                CalculDepth(f0);
                fs.Add(f0);
            }

            int count = 0;
            // 如果x步幅大，就用x来遍历
            if (dir_nrl.x != 0 && (Math.Abs(dir_nrl.y) < Math.Abs(dir_nrl.x)))
            {
                dir_nrl *= Math.Abs(1 / dir_nrl.x);
                dir_nrl.x = dir_nrl.x > 0 ? 1 : -1;
                count = (int)Math.Abs(dir.x);
            }
            // 如果y步幅大，就用y来遍历
            else if (dir_nrl.y != 0)
            {
                dir_nrl *= Math.Abs(1 / dir_nrl.y);
                dir_nrl.y = dir_nrl.y > 0 ? 1 : -1;
                count = (int)Math.Abs(dir.y);
            }
            else if (dir_nrl.z != 0)
            {
                throw new Exception("error");
                // 这里的分支基本不可能
                //dir_nrl *= Math.Abs(1 / dir_nrl.z);
                //count = (int)Math.Abs(dir.z);
            }
            else
            {
                throw new Exception("error");
            }

            var infoCount = 0;
            if (interpolation && f0.ShaderOut != null && f0.ShaderOut.upperStageOutInfos != null)
                infoCount = f0.ShaderOut.upperStageOutInfos.Length;

#if PERSPECTIVE_CORRECT
            var p0InvZ = f0.ClipZ == 0 ? 0 : 1 / f0.ClipZ;
            var p1InvZ = f1.ClipZ == 0 ? 0 : 1 / f1.ClipZ;
#endif
            var maxXp1 = maxX + 1;
            var maxYp1 = maxY + 1;
            for (int i = 0, num = 1; i < count; i++, num++)
            {
                var newP = p0 + dir_nrl * num;
                if (dir_nrl.x == 1) // 如果使用x水平方向来遍历，当前生成的点超出最大值，就直接结束即可
                {
                    if (newP.x > maxXp1) return;
                }
                else if (dir_nrl.y == 1) // 如果使用y垂直方向来遍历，当前生成的点超出最大值，就直接结束即可
                {
                    if (newP.y > maxYp1) return;
                }
                if (newP.x < minX || newP.y < minY) continue;
                FragInfo f = FragInfo.GetFragInfo();
                if (interpolation)
                {
                    ShaderOut shaderOut = new ShaderOut
                    {
                        upperStageOutInfos = new OutInfo[infoCount]
                    };
                    OutInfo[] infos = shaderOut.upperStageOutInfos;
                    var t = (float)i / count;
                    var tt = 1 - t;
#if PERSPECTIVE_CORRECT
                    // 计算出新的插值z方法 1
                    var newZ = 1 / Mathf.Lerp(p0InvZ, p1InvZ, t, tt);
                    newP.z = newZ;
#endif
                    for (int j = 0; j < infoCount; j++)
                    {
                        var f0Info = f0.ShaderOut.upperStageOutInfos[j];
                        var f1Info = f1.ShaderOut.upperStageOutInfos[j];
                        object v = null;
                        if (f0Info.nointerpolation)
                        {
                            v = f0Info.value;
                        }
                        else
                        {
                            if (f0Info.layout == OutLayout.SV_Position)
                            {
                                v = newP;
                            }
                            else
                            {
                                v = Interpolation(f0Info, f1Info, t, tt
#if PERSPECTIVE_CORRECT
                                ,newZ, p0InvZ, p1InvZ
#endif
                                    );
                            }
                        }
                        infos[j] = new OutInfo
                        {
                            layout = f0Info.layout,
                            floatNum = f0Info.floatNum,
                            location = f0Info.location,
                            value = v
                        };
                    }
                    f.Set(shaderOut);
                }
                f.p = newP;
                CalculDepth(f);
                fs.Add(f);
            }
            
            p = f1.p;
            if (p.x >= minX && p.x < maxX && p.y >= minY && p.y < maxY)
            {
                fs.Add(f1);
                CalculDepth(f1);
            }
        }

        // =============== 顶点数据的插值 start ===============
        // 透视校正插值计算方式，但不知道为何没起作用
        // 下面的泛型无法约束T是重写了*运算符的内容，所以只能一个一个类型的写一遍
        //private T PerspectiveInterpolation<T>(T v1, T v2, float newZ, float invZ0, float invZ1, float t, float tt)
        //{
        //    return newZ * Mathf.Lerp(v1 * invZ0, v2 * invZ1, t, tt);
        //}

        private object Interpolation(OutInfo f0Info, OutInfo f1Info, float t, float tt
#if PERSPECTIVE_CORRECT
            , float newZ, float p0InvZ, float  p1InvZ
#endif
            )
        {
            object v = null;
            if (f0Info.floatNum == LayoutFloatNum.F1)
            {
#if PERSPECTIVE_CORRECT
                                    v = PerspectiveInterpolation(
                                        (float)f0Info.value,
                                        (float)f1Info.value,
                                        newZ, p0InvZ, p1InvZ, t, tt);
#else
                v = Mathf.Lerp((float)f0Info.value, (float)f1Info.value, t, tt);
#endif
            }
            else if (f0Info.floatNum == LayoutFloatNum.F2)
            {
#if PERSPECTIVE_CORRECT
                                    v = PerspectiveInterpolation(
                                        (Vector2)f0Info.value,
                                        (Vector2)f1Info.value,
                                        newZ, p0InvZ, p1InvZ, t, tt);
#else
                v = Mathf.Lerp((Vector2)f0Info.value, (Vector2)f1Info.value, t, tt);
#endif
            }
            else if (f0Info.floatNum == LayoutFloatNum.F3)
            {
#if PERSPECTIVE_CORRECT
                                    v = PerspectiveInterpolation(
                                        (Vector3)f0Info.value,
                                        (Vector3)f1Info.value,
                                        newZ, p0InvZ, p1InvZ, t, tt);
#else
                v = Mathf.Lerp((Vector3)f0Info.value, (Vector3)f1Info.value, t, tt);
#endif
            }
            else if (f0Info.floatNum == LayoutFloatNum.F4)
            {
#if PERSPECTIVE_CORRECT
                                    v = PerspectiveInterpolation(
                                        (Vector4)f0Info.value,
                                        (Vector4)f1Info.value,
                                        newZ, p0InvZ, p1InvZ, t, tt);
#else
                v = Mathf.Lerp((Vector4)f0Info.value, (Vector4)f1Info.value, t, tt);
#endif
            }
            else
            {
                throw new Exception($"not implements floatNum:{f0Info.floatNum}");
            }
            return v;
        }
        private float PerspectiveInterpolation(float v1, float v2, float newZ, float invZ0, float invZ1, float t, float tt)
        {
            return newZ * Mathf.Lerp(v1 * invZ0, v2 * invZ1, t, tt);
        }
        private Vector2 PerspectiveInterpolation(Vector2 v1, Vector2 v2, float newZ, float invZ0, float invZ1, float t, float tt)
        {
            return newZ * Mathf.Lerp(v1 * invZ0, v2 * invZ1, t, tt);
        }
        private Vector3 PerspectiveInterpolation(Vector3 v1, Vector3 v2, float newZ, float invZ0, float invZ1, float t, float tt)
        {
            return newZ * Mathf.Lerp(v1 * invZ0, v2 * invZ1, t, tt);
        }
        private Vector4 PerspectiveInterpolation(Vector4 v1, Vector4 v2, float newZ, float invZ0, float invZ1, float t, float tt)
        {
            return newZ * Mathf.Lerp(v1 * invZ0, v2 * invZ1, t, tt);
        }
        // =============== 顶点数据的插值 end===============

        // 计算深度
        private void CalculDepth(FragInfo f)
        {
            // 以下计算方式都不对
            //f.depth = 1 - 1 / f.p.z;
            f.depth = 1 - 1 / (f.p.z - renderer.State.CameraNear * renderer.State.CameraFar);

            //var near = renderer.State.CameraNear;
            //var far = renderer.State.CameraFar;
            //f.depth = 1 / (((far - near) / near) * f.p.w - far / near);

            //f.depth = f.p.w;
        }

        // 生成片段
        public void GenFragInfo(
            Primitive_Triangle triangle, 
            List<FragInfo> shadedResult, 
            List<FragInfo> wireframeResult,
            List<FragInfo> normalLineResult)
        {
            shadedResult.Clear();
            wireframeResult.Clear();
            normalLineResult.Clear();

            var validated = (triangle.Validated() && !CullingTriangle(triangle));
            if (!validated) return;

            GetPos(triangle, out Vector3 tv, out Vector3 mv, out Vector3 bv, out bool topSameAsMiddle, out bool bottomSameAsMiddle);

            FragInfo tvF = null;
            FragInfo mvF = null;
            FragInfo bvF = null;

            if (tv == triangle.f0.p) tvF = triangle.f0;
            if (tv == triangle.f1.p) tvF = triangle.f1;
            if (tv == triangle.f2.p) tvF = triangle.f2;

            if (mv == triangle.f0.p) mvF = triangle.f0;
            if (mv == triangle.f1.p) mvF = triangle.f1;
            if (mv == triangle.f2.p) mvF = triangle.f2;

            if (bv == triangle.f0.p) bvF = triangle.f0;
            if (bv == triangle.f1.p) bvF = triangle.f1;
            if (bv == triangle.f2.p) bvF = triangle.f2;

            fragInfosHelper1.Clear();
            GenLineFrag(tvF, mvF, fragInfosHelper1);
            GenLineFrag(mvF, bvF, fragInfosHelper1);

            fragInfosHelper2.Clear();
            GenLineFrag(tvF, bvF, fragInfosHelper2);

            var minX = 0;
            var minY = 0;
            var maxX = renderer.BackBufferWidth - 1;
            var maxY = renderer.BackBufferHeight - 1;

            if (Renderer.State.Scissor == Scissor.On)
            {
                minX = Renderer.State.ScissorRect.X;
                minY = Renderer.State.ScissorRect.Y;
                maxX = Renderer.State.ScissorRect.Right;
                maxY = Renderer.State.ScissorRect.Bottom;
            }

            // shaded fragments
            if (!triangle.clip && (renderer.State.ShadingMode & ShadingMode.Shaded) != 0)
            {
                Vector2 t2m = tv - mv;
                Vector2 t2b = tv - bv;

                List<FragInfo> leftFrags;
                List<FragInfo> rightFrags;

                var cv = t2b.Cross(t2m);

                if (cv < 0) // t2m正在t2b左手边
                {
                    leftFrags = fragInfosHelper2;
                    rightFrags = fragInfosHelper1;
                }
                else if (cv > 0) // t2m正在t2b右手边
                {
                    leftFrags = fragInfosHelper1;
                    rightFrags = fragInfosHelper2;
                }
                else
                {
                    // 三点相对屏幕空间共线了，该三角形没有意义，不需要花
                    throw new Exception("三点相对屏幕空间共线了，该三角形没有意义，不需要花");
                }

                var ty = (int)(tv.y); // top y
                var by = (int)(bv.y); // bottom y

                var goingon = true;
                // 如果三角形的最底部点比minY还往上，即：最底部点都在屏幕上方
                // 或是三角形的最顶部点比maxY还往下，即：最顶不点都在屏幕下方
                // 那么直接跳过该三角形
                if ((by < minY || ty > maxY)) goingon = false;
                if (goingon)
                {
                    var leftI = 0;
                    var rightI = 0;
                    for (int iy = ty; iy < by; iy++)
                    {
                        // 水平扫描线段的最左端片段
                        FragInfo leftF = null;
                        // 水平扫描线段的最右端片段
                        FragInfo rightF = null;
                        // 如果水平高度超屏幕了，直接跳过
                        if ((iy < minY || iy > maxY)) continue;

                        var count = leftFrags.Count;
                        for (; leftI < count; leftI++)
                        {
                            //if ((int)(leftFrags[leftI].p.y) == iy)
                            if (leftFrags[leftI].p.y >= iy)
                            {
                                //if ((int)leftFrags[leftI].p.y > iy)
                                //{
                                //    //throw new Exception("error");
                                //}
                                if (leftI < count - 1)
                                {
                                    // 如果下一个坐标也是一样的，那么继续跳过，取下一个
                                    if ((int)leftFrags[leftI + 1].p.y == iy) continue;
                                }
                                leftF = leftFrags[leftI];
                                break;
                            }
                        }

                        count = rightFrags.Count;
                        for (; rightI < count; rightI++)
                        {
                            //if ((int)(rightFrags[rightI].p.y) == iy)
                            if (rightFrags[rightI].p.y >= iy)
                            {
                                //if ((int)rightFrags[rightI].p.y > iy)
                                //{
                                //    //throw new Exception("error");
                                //}
                                if (rightI < count - 1)
                                {
                                    // 如果下一个坐标也是一样的，那么继续跳过，取下一个
                                    if ((int)rightFrags[rightI + 1].p.y == iy) continue;
                                }
                                rightF = rightFrags[rightI];
                                break;
                            }
                        }

                        if (leftF == null || rightF == null)
                        {
                            Console.WriteLine("error!!");
                            break;
                            //throw new Exception("error");
                        }

                        var dx = (int)Math.Ceiling((Math.Abs(leftF.p.x - rightF.p.x)));
                        if (dx == 0) continue;
                        // 求出从左往右点的方向
                        var dir = rightF.p - leftF.p;
                        // 方向单位化
                        dir = dir.normalized;
                        // 将x方向单位化的步长
                        dir *= 1 / dir.x;
                        // 确保不需要上、下分量移动
                        dir.y = 0;
                        var infoCount = leftF.ShaderOut.upperStageOutInfos.Length;

#if PERSPECTIVE_CORRECT
                        // 因为透视投影中点的z与投影近平面的交点的x,y不是线性关系的
                        // 但与透视投影中点的1/z与投影近平面的交点的x,y是线性关系的
                        // 所以我们先将左右两边的1/z先求出来，用于插值使用
                        var p0InvZ = leftF.ClipZ == 0 ? 0 : 1 / leftF.ClipZ;
                        var p1InvZ = rightF.ClipZ == 0 ? 0 : 1 / rightF.ClipZ;
#endif
                        var p1top0 = (triangle.f1.p - triangle.f0.p).xyz;
                        var p2top0 = (triangle.f2.p - triangle.f0.p).xyz;
                        var n = p1top0.Cross(p2top0).normalized;

                        for (int i = 1; i < dx; i++)
                        {
                            var newP = leftF.p + dir * i;
                            if (newP.x < minX || newP.x > maxX || newP.y < minY || newP.y > maxY) continue;
                            newP.y = iy;

                            var f = FragInfo.GetFragInfo();
                            f.p = newP;
                            ShaderOut shaderOut = new ShaderOut
                            {
                                upperStageOutInfos = new OutInfo[infoCount]
                            };
                            OutInfo[] infos = shaderOut.upperStageOutInfos;
                            var t = (float)i / dx;
                            var tt = 1 - t;

#if PERSPECTIVE_CORRECT
                            // 计算出新的插值z方法 1
                            var newZ = 1 / Mathf.Lerp(p0InvZ, p1InvZ, t, tt);
                            newP.z = newZ; // 这里其实与newZ 赋值前，其实与newP.z值是一样的，因为我们的newP.xyz也是之前插值向量算出来的
#endif
                            for (int j = 0; j < infoCount; j++)
                            {
                                var f0Info = leftF.ShaderOut.upperStageOutInfos[j];
                                var f1Info = rightF.ShaderOut.upperStageOutInfos[j];
                                object v = null;
                                if (f0Info.nointerpolation)
                                {
                                    v = f0Info.value;
                                }
                                else
                                {
                                    if (f0Info.layout == OutLayout.SV_Position)
                                    {
                                        v = newP;
                                    }
                                    else
                                    {
                                        v = Interpolation(f0Info, f1Info, t, tt
#if PERSPECTIVE_CORRECT
                                        ,newZ, p0InvZ, p1InvZ
#endif
                                            );
                                    }
                                }
                                infos[j] = new OutInfo
                                {
                                    layout = f0Info.layout,
                                    floatNum = f0Info.floatNum,
                                    location = f0Info.location,
                                    value = v
                                };
                            }
                            f.Set(shaderOut);
                            f.p = newP;
                            if (f.p.x < minX || f.p.x > maxX || f.p.y < minY || f.p.y > maxY)
                            {
                                throw new Exception("error");
                            }
                            CalculDepth(f);
                            shadedResult.Add(f);
                        }
                    }
                }

                // 添加回外框线条
                var len = fragInfosHelper1.Count;
                for (int i = 0; i < len; i++)
                {
                    var f = fragInfosHelper1[i];
                    var p = f.p;
                    if (p.x < minX || p.x > maxX || p.y < minY || p.y > maxY) continue;
                    shadedResult.Add(f);
                }
                len = fragInfosHelper2.Count;
                for (int i = 0; i < len; i++)
                {
                    var f = fragInfosHelper2[i];
                    var p = f.p;
                    if (p.x < minX || p.x > maxX || p.y < minY || p.y > maxY) continue;
                    shadedResult.Add(f);
                }
            }
            
            // wireframe fragments
            if (triangle.clip || (renderer.State.ShadingMode & ShadingMode.Wireframe) != 0)
            {
                var len = fragInfosHelper1.Count;
                for (int i = 0; i < len; i++)
                {
                    var f = fragInfosHelper1[i];
                    var p = f.p;
                    if (p.x < minX || p.x > maxX || p.y < minY || p.y > maxY) continue;
                    wireframeResult.Add(f);
                }
                len = fragInfosHelper2.Count;
                for (int i = 0; i < len; i++)
                {
                    var f = fragInfosHelper2[i];
                    var p = f.p;
                    if (p.x < minX || p.x > maxX || p.y < minY || p.y > maxY) continue;
                    wireframeResult.Add(f);
                }
            }

            // debug: show TBN
            if (Renderer.State.DebugShowTBN)
            {
                // tangent
                var from = Vector4.lightRed;
                var to = Vector4.darkRed;
                CollectTBN(triangle, OutLayout.Tangent, 0, from, to, normalLineResult);

                // bitangent
                from = Vector4.lightGreen;
                to = Vector4.green;
                CollectTBN(triangle, OutLayout.Tangent, 1, from, to, normalLineResult);

                // normals
                from = Vector4.lightBlue;
                to = Vector4.blue;
                CollectTBN(triangle, OutLayout.Normal, 0, from, to, normalLineResult);
            }
        }

        // 收集TBN线条的片段
        private void CollectTBN(Primitive_Triangle triangle, OutLayout layout, int location, Vector4 from, Vector4 to, List<FragInfo> normalLineResult)
        {
            var depthBuff = renderer.Per_Frag.DepthBuff;
            var depthInv = 1 / (renderer.State.CameraFar + renderer.State.CameraFar * renderer.State.CameraNear);

            var minX = 0;
            var minY = 0;
            var maxX = renderer.BackBufferWidth - 1;
            var maxY = renderer.BackBufferHeight - 1;

            if (Renderer.State.Scissor == Scissor.On)
            {
                minX = Renderer.State.ScissorRect.X;
                minY = Renderer.State.ScissorRect.Y;
                maxX = Renderer.State.ScissorRect.Right;
                maxY = Renderer.State.ScissorRect.Bottom;
            }

            var triangleVertices = new FragInfo[] { triangle.f0, triangle.f1, triangle.f2 };
            var len = triangleVertices.Length;

            for (int i = 0; i < len; i++)
            {
                var f = triangleVertices[i];
                Vector3? vec = null;
                var jLen = f.ShaderOut.upperStageOutInfos.Length;
                for (int j = 0; j < jLen; j++)
                {
                    var info = f.ShaderOut.upperStageOutInfos[j];
                    if (info.layout == layout && info.location == location)
                    {
                        vec = (Vector3)info.value;
                        break;
                    }
                }
                if (vec.HasValue)
                {
                    var vecPos = f.p + Vector4.Get(vec.Value * renderer.State.DebugTBNlLen, 1);

                    var f0 = FragInfo.GetFragInfo();
                    f0.p = f.p;
                    var f1 = FragInfo.GetFragInfo();
                    f1.p = vecPos;

                    var srcIdx = normalLineResult.Count;
                    GenLineFrag(f0, f1, normalLineResult, false);

                    var count = normalLineResult.Count;
                    var count1 = normalLineResult.Count - srcIdx;
                    for (int newI = srcIdx; newI < count; newI++)
                    {
                        var vecF = normalLineResult[newI];
                        if (vecF.p.x < minX || vecF.p.x > maxX || vecF.p.y < minY || vecF.p.y > maxY)
                        {
                            vecF.discard = true;
                            continue;
                        }
                        var t = (float)(newI - srcIdx) / count1;
                        vecF.normalLineColor = Mathf.Lerp(from, to, t);
                    }
                }
            }
        }

        // 销毁资源
        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (fragList != null)
            {
                fragList.Clear();
                fragList = null;
            }
            if (fragsHelper1 != null)
            {
                fragsHelper1.Clear();
                fragsHelper1 = null;
            }
            if (fragsHelper2 != null)
            {
                fragsHelper2.Clear();
                fragsHelper2 = null;
            }
            if (fragPool != null)
            {
                fragPool.Dispose();
                fragPool = null;
            }

            usingTiranglePosHehlper = null;
            allTrianglePosHelper = null;
            renderer = null;

        }

        // =============== 旧版 start===============
        private List<FragData> fragList = new List<FragData>();
        private List<FragData> fragsHelper1 = new List<FragData>();
        private List<FragData> fragsHelper2 = new List<FragData>();
        private Pool<FragData> fragPool = new Pool<FragData>(0);//100000);
        private void ToPool(FragData f)
        {
            /*
            不知为何，这里的f.datas为null，只有dispose过才能为null
            dispose过的我是不会给外部使用的，所以回收的时候也就不会有dispose过的元素
            有可能DotNet的List<T>.Clear()之后，相同类型对象应用密集时，底层有处理缓存对象的复用
            本想减少GC卡顿的（不过我的手提上可能性能好一些，没发现有卡顿）
            现在我也不想去验证这块，所以这里注释掉下面的代码
            */
            //f.datas.Clear();
            //f.discard = false;
            //f.depth = 0;
            //fragPool.To(f);
        }
        private void ToPool(List<FragData> fs, bool autoClear = true)
        {
            //var len = fs.Count;
            //for (int i = 0; i < len; i++)
            //{
            //    ToPool(fs[i]);
            //}
            if (autoClear) fs.Clear();
        }
        private void ToPool(FragData[] fs)
        {
            //var len = fs.Length;
            //for (int i = 0; i < len; i++)
            //{
            //    ToPool(fs[i]);
            //}
        }
        private FragData GetFrag()
        {
            return fragPool.Get() as FragData;
        }
        // 根据两点生成之间的片段：旧版管线
        private void GenLineFrag(Vector3 p0, Vector3 p1, List<FragData> fs)
        {
            var dir = p1 - p0;
            var dir_nrl = dir.normalized;

            var f = GetFrag();
            f.p = p0;

            fs.Add(f);

            int count = 0;

            if (dir_nrl.x != 0 && (Math.Abs(dir_nrl.y) < Math.Abs(dir_nrl.x)))
            {
                dir_nrl *= Math.Abs(1 / dir_nrl.x);
                count = (int)Math.Abs(dir.x);
            }
            else if (dir_nrl.y != 0)
            {
                dir_nrl *= Math.Abs(1 / dir_nrl.y);
                count = (int)Math.Abs(dir.y);
            }
            else if (dir_nrl.z != 0)
            {
                dir_nrl *= Math.Abs(1 / dir_nrl.z);
                count = (int)Math.Abs(dir.z);
            }
            else
            {
                throw new Exception("error");
            }

            for (int i = 0, num = 1; i < count; i++, num++)
            {
                f = GetFrag();
                f.p = p0 + dir_nrl * num;
                fs.Add(f);
            }

            f = GetFrag();
            f.p = p1;
            fs.Add(f);
        }
        // 绘制三角形：旧版管线中的绘制三角
        public void DrawTriangle(Triangle t, Vector4 triangleColor, Vector4 wireFrameColor)
        {
            var validated = (t.Validated() && !CullingTriangle(t));
            if (!validated) return;

            GetPos(t, out Vector3 tv, out Vector3 mv, out Vector3 bv, out bool topSameAsMiddle, out bool bottomSameAsMiddle);

            ToPool(fragsHelper1);
            GenLineFrag(tv, mv, fragsHelper1);
            GenLineFrag(mv, bv, fragsHelper1);

            ToPool(fragsHelper2);
            GenLineFrag(tv, bv, fragsHelper2);

            var minX = 0;
            var minY = 0;
            var maxX = renderer.BackBufferWidth - 1;
            var maxY = renderer.BackBufferHeight - 1;

            if (Renderer.State.Scissor == Scissor.On)
            {
                minX = Renderer.State.ScissorRect.X;
                minY = Renderer.State.ScissorRect.Y;
                maxX = Renderer.State.ScissorRect.Right;
                maxY = Renderer.State.ScissorRect.Bottom;
            }

            ToPool(fragList);
            // Generate fragments
            if ((renderer.State.ShadingMode & ShadingMode.Shaded) != 0)
            {
                Vector2 t2m = tv - mv;
                Vector2 t2b = tv - bv;

                List<FragData> leftFrags;
                List<FragData> rightFrags;

                if (t2b.Cross(t2m) < 0) // t2m正在t2b左手边
                {
                    leftFrags = fragsHelper2;
                    rightFrags = fragsHelper1;
                }
                else if (t2b.Cross(t2m) > 0) // t2m正在t2b右手边
                {
                    leftFrags = fragsHelper1;
                    rightFrags = fragsHelper2;
                }
                else
                {
                    // 三点相对屏幕空间共线了，该三角形没有意义，不需要花
                    throw new Exception("三点相对屏幕空间共线了，该三角形没有意义，不需要花");
                }

                var ty = (int)(tv.y);
                var by = (int)(bv.y);

                var goingon = true;
                if ((by < minY || ty > maxY)) goingon = false;
                if (goingon)
                {
                    var leftI = 0;
                    var rightI = 0;
                    for (int iy = ty; iy < by; iy++)
                    {
                        FragData leftF = null;
                        FragData rightF = null;
                        if ((iy < minY || iy > maxY)) continue;

                        var count = leftFrags.Count;
                        for (; leftI < count; leftI++)
                        {
                            //if ((int)(leftFrags[leftI].p.y) == iy)
                            if (leftFrags[leftI].p.y >= iy)
                            {
                                leftF = leftFrags[leftI];
                                break;
                            }
                        }

                        count = rightFrags.Count;
                        for (; rightI < count; rightI++)
                        {
                            //if ((int)(rightFrags[rightI].p.y) == iy)
                            if (rightFrags[rightI].p.y >= iy)
                            {
                                rightF = rightFrags[rightI];
                                break;
                            }
                        }

                        if (leftF == null || rightF == null)
                        {
                            Console.WriteLine("error!!");
                            break;
                            //throw new Exception("error");
                        }

                        var dx = (int)Math.Ceiling((Math.Abs(leftF.p.x - rightF.p.x)));
                        if (dx == 0)
                        { // leftF与rightF共栅格点了，直接添加其中一个就可以了
                            fragList.Add(leftF);
                            continue;
                        }
                        var dir = rightF.p - leftF.p;
                        dir = dir.normalized;
                        dir *= 1 / dir.x;
                        dir.y = 0;
                        for (int i = 0; i < dx; i++)
                        {
                            var newP = leftF.p + dir * i;
                            if (newP.x < minX || newP.x > maxX) continue;
                            var f = GetFrag();
                            f.p = newP;
                            fragList.Add(f);

                        }
                    }
                }
            }

            var depthbuff = renderer.Per_Frag.DepthBuff;
            var depthwrite = renderer.State.DepthWrite;
            var maxZ = renderer.State.CameraFar;
            maxZ += renderer.State.CameraFar * renderer.State.CameraNear;
            var depthInv = 1 / maxZ;
            // depth offset
            var offsetDepth = 0.0f;
            //if (renderer.State.DepthOffset == DepthOffset.On) // 这里需要优化,法线应该顶点数据中传进来的
            //{
            // https://blog.csdn.net/linjf520/article/details/94596764
            var faceNormal = (t.p1 - t.p0).Cross(t.p2 - t.p0).normalized; // 这里需要优化,法线应该顶点数据中传进来的
                                                                          // 掠射角
            var faceNormalDotForward = 1 - Math.Abs(faceNormal.Dot(Vector3.forward));
            // 我之前翻译的文章：https://blog.csdn.net/linjf520/article/details/94596764
            // 我的理解是上面的这个算法
            offsetDepth = faceNormalDotForward * renderer.State.DepthOffsetFactor
                + depthInv * renderer.State.DepthOffsetUnit;
            //}
            var depthOffset = renderer.State.DepthOffset;

            fragList.AddRange(fragsHelper1);
            fragList.AddRange(fragsHelper2);
            // lambert
            var normal = faceNormal;
            var lightDir = renderer.State.DebugDirectionalLight.normalized;
            var lightDotNormal = lightDir.Dot(normal);
            if (renderer.State.DebugHalfLambertLighting)
                lightDotNormal = lightDotNormal * 0.5f + 0.5f;
            lightDotNormal *= renderer.State.DebugLItensity;
#if SPECULAR
            // specular
            var camPos = new Vector3(
                renderer.State.CamX, 
                renderer.State.CamY, 
                renderer.State.CamZ);
            var enabledSpecular = renderer.State.DebugSpecular;
            var specularColor = renderer.State.DebugSpecularColor;
            var specularItensity = 100 - renderer.State.DebugSpecularItensity;
            var inLightDir = -lightDir;
#endif
            // shaded
            var len = fragList.Count;
            var bmd = renderer.Begin(); // begin
            var finalColor = Vector4.Get();
            for (int i = 0; i < len; i++)
            {
                var f = fragList[i];
                if (f.p.x < minX || f.p.x > maxX || f.p.y < minY || f.p.y > maxY) continue;
                f.depth = 1 - f.p.z * depthInv;
                var testDepth = f.depth;
                if (depthOffset == DepthOffset.On)
                    testDepth += offsetDepth;
                if (depthbuff.Test(renderer.State.DepthTest, (int)f.p.x, (int)f.p.y, testDepth))
                {
                    // 是否开启深度写入
                    if (depthwrite == DepthWrite.On)
                    {
                        depthbuff.Write((int)f.p.x, (int)f.p.y, testDepth);
                    }
                    finalColor = triangleColor * lightDotNormal;
#if SPECULAR
                    if (enabledSpecular)
                    {
                        var viewDir = (camPos - f.p).normalized;
                        var reflectDir = Vector3.Reflect(inLightDir, normal);
                        var reflectNotView = Mathf.Max(0, Vector3.Dot(reflectDir, viewDir));
                        var specular = specularColor * (float)Math.Pow(reflectNotView, specularItensity);
                        finalColor += specular;
                    }
#endif
                    renderer.BeginSetPixel(bmd.Scan0, f.p, finalColor);
                }
            }

            // wireframe
            if ((renderer.State.ShadingMode & ShadingMode.Wireframe) != 0)
            {
                var count = fragsHelper1.Count;
                offsetDepth = faceNormalDotForward * (renderer.State.DepthOffsetFactor)
                + depthInv * (renderer.State.DepthOffsetUnit - 0.01f);
                for (int i = 0; i < count; i++)
                {
                    var f = fragsHelper1[i];
                    if (f.p.x < minX || f.p.x > maxX || f.p.y < minY || f.p.y > maxY) continue;
                    f.depth = 1 - f.p.z * depthInv;
                    var testDepth = f.depth + offsetDepth;
                    if (depthbuff.Test(renderer.State.DepthTest, (int)f.p.x, (int)f.p.y, testDepth))
                    {
                        // 是否开启深度写入
                        if (depthwrite == DepthWrite.On)
                        {
                            depthbuff.Write((int)f.p.x, (int)f.p.y, testDepth);
                        }
                        renderer.BeginSetPixel(bmd.Scan0, f.p, wireFrameColor);
                    }
                }
                count = fragsHelper2.Count;
                for (int i = 0; i < count; i++)
                {
                    var f = fragsHelper2[i];
                    if (f.p.x < minX || f.p.x > maxX || f.p.y < minY || f.p.y > maxY) continue;
                    f.depth = 1 - f.p.z * depthInv + offsetDepth;
                    var testDepth = f.depth + offsetDepth;
                    if (depthbuff.Test(renderer.State.DepthTest, (int)f.p.x, (int)f.p.y, testDepth))
                    {
                        // 是否开启深度写入
                        if (depthwrite == DepthWrite.On)
                        {
                            depthbuff.Write((int)f.p.x, (int)f.p.y, testDepth);
                        }
                        renderer.BeginSetPixel(bmd.Scan0, f.p, wireFrameColor);
                    }
                }
            }

            // debug: show normal line
            if (renderer.State.DebugShowTBN)
            {
                var blueColor = Vector4.Get(0, 0, 1, 1);
                var normalPos0 = t.p0;
                var normalPos1 = normalPos0 + faceNormal.normalized * renderer.State.DebugTBNlLen;
                ToPool(fragsHelper1);
                GenLineFrag(normalPos0, normalPos1, fragsHelper1);
                len = fragsHelper1.Count;
                for (int i = 0; i < len; i++)
                {
                    var f = fragsHelper1[i];
                    if (f.p.x < minX || f.p.x > maxX || f.p.y < minY || f.p.y > maxY) continue;
                    renderer.BeginSetPixel(bmd.Scan0, f.p, blueColor);
                }
            }

            renderer.End(bmd); // end

            ToPool(fragsHelper1);
            ToPool(fragsHelper2);
            ToPool(fragList);
        }
        // =============== 旧版 end===============

#if UNUSE // 暂时使用不上的
        // 填充线框
        private void FillWireframe(List<ScanPos> pos, Color color)
        {
            var minX = 0;
            var minY = 0;
            var maxX = renderer.BackBufferWidth - 1;
            var maxY = renderer.BackBufferHeight - 1;

            if (Renderer.State.Scissor == Scissor.On)
            {
                minX = Renderer.State.ScissorRect.X;
                minY = Renderer.State.ScissorRect.Right;
                maxX = Renderer.State.ScissorRect.Y;
                maxY = Renderer.State.ScissorRect.Bottom;
            }

            var len = pos.Count;
            var bmd = Renderer.Begin();
            for (int i = 0; i < len; i++)
            {
                var p = pos[i];
                if (p.x < minX || p.x > maxX || p.y < minY || p.y > maxY) continue;
                Renderer.BeginSetPixel(bmd.Scan0, p, color);
            }
            Renderer.End(bmd);
        }
        // 是否在ScissorRect内 // 暂时不用到，因为Rectangle.Contains效率太低
        private bool InScissorRect(Frag f)
        {
            return ScissorRect.Contains(f.x, f.y);
        }
        // 临时测试方式，需要重构
        // 暂时用不上
        private Vector2? ComputePos(Triangle t)
        {
            GetPos(t, out Vector3 tv, out Vector3 mv, out Vector3 bv, out bool topSameAsMiddle, out bool bottomSameAsMiddle);

            if (topSameAsMiddle || bottomSameAsMiddle) return null;

            var P0 = tv;
            var P1 = mv;
            var P2 = bv;

            var P0YminusP2Y = P0.y - P2.y; // 平行
            if (P0YminusP2Y == 0) return null;

            var P0XminusP2X = P0.x - P2.x; // 垂直
            if (P0XminusP2X == 0) return new Vector2(tv.x, mv.y);

            float m1 = P0YminusP2Y / P0XminusP2X;
            /* 推导、变换过程
             *  https://blog.csdn.net/linjf520/article/details/48729285
                m1 = (P0.y - middle.y) / (P0.x - P3.x)
                m1*(P0.x - P3.x)= (P0.y - middle.y)
                m1*P0.x - m1*P3.x= (P0.y - middle.y)
                m1*P0.x - m1*P3.x= (P0.y - middle.y)
                 - m1*P3.x= (P0.y - middle.y) - m1*P0.x
                m1*P3.x=  m1*P0.x - (P0.y - middle.y)
                P3.x=  (m1*P0.x - (P0.y - middle.y)) / m1
             */
            var P3X = (m1 * P0.x - (P0.y - mv.y)) / m1;
            var lerpValue = 1.0f; // 这里需要写插值
            throw new Exception("not implememnts");
            return new Vector3(P3X, mv.y, lerpValue);
        }
#endif
    }
}
