// jave.lin 2019.07.18
using RendererCore.Renderer;
using RendererCoreCommon.Renderer.Common.Mathes;
using RendererCoreCommon.Renderer.Common.Shader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;

namespace RendererCore.Games
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Description("网格对象")]
    public class Mesh
    {
        public Vector3[] vertices;                              // 顶点坐标
        public int[] triangles { get; set; }                    // 顶点索引
        public Vector3[] normals { get; set; }                  // 顶点法线
        public Vector3[] tangents { get; set; }                 // 顶点切线
        public Vector2[] uv { get; set; }                       // 顶点uv
        public Vector4[] colors { get; set; }                   // 顶点颜色

        public void CaculateNormalAndTangent() // 计算法线与切线
        {
            if (normals == null || normals.Length != vertices.Length) normals = new Vector3[vertices.Length];
            if (tangents == null || tangents.Length != vertices.Length) tangents = new Vector3[vertices.Length];

            var len = triangles.Length;
            for (int i = 0; i < len; i += 3)
            {
                var idx1 = triangles[i];
                var idx2 = triangles[i + 1];
                var idx3 = triangles[i + 2];
                var v1 = vertices[idx1];
                var v2 = vertices[idx2];
                var v3 = vertices[idx3];

                var tangent = v2 - v1;
                var bitangent = v3 - v1;
                var normal = tangent.Cross(bitangent);

                normal.Normalize();
                tangent.Normalize();

                normals[idx1] = normal;
                normals[idx2] = normal;
                normals[idx3] = normal;

                tangents[idx1] = tangent;
                tangents[idx2] = tangent;
                tangents[idx3] = tangent;
            }
        }
    }

    [Description("网格渲染器")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class MeshRenderer
    {
        public Mesh Mesh;
        public Renderer.Renderer Renderer;

        public MeshRenderer()
        {
            Renderer = RendererCore.Renderer.Renderer.Instance;
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Description("摄像机")]
    public class Camera : IDisposable
    {
        // transform
        [Category("transform")]
        [Description("视图变换")]
        public Matrix4x4 View { get; private set; }
        [Category("transform")]
        [Description("投影变换")]
        public Matrix4x4 Proj { get; private set; }

        // look at
        [Category("look at")]
        [Description("是否有看向的目标点")]
        public Vector3? target { get; set; }
        [Category("look at")]
        [Description("看着目标点时镜头向上的方位")]
        public Vector3 up { get; set; } = Vector3.up;

        [Category("Viewport")]
        [Description("Window视图")]
        public Rectangle viewport { get; set; } = Rectangle.Empty;

        // view
        [Category("view")]
        [Description("位移量")]
        public Vector3 Translate { get; set; }              // 位移量
        [Category("view")]
        [Description("旋转量")]
        public Vector3 Euler { get; set; }                  // 旋转量
                                                            // proj
                                                            // proj-ortho
        [Category("proj-ortho")]
        [Description("屏幕高度的一半")]
        public float size { get; set; } = 5;              // 屏幕高度的一半，单位：像素
                                                          // proj-perspective
        [Category("proj-perspective")]
        [Description("纵向张开角度")]
        public float fov { get; set; } = 60f;               // 纵向张开角度
                                                            // proj-both
        [Category("proj-both")]
        [Description("宽高比")]
        public float aspect { get; set; } = 1f;     // 宽高比
        [Category("proj-both")]
        [Description("近裁面，必须大于0")]
        public float near { get; set; } = 1f;             // 近裁面，必须大于0
        [Category("proj-both")]
        [Description("远裁面")]
        public float far { get; set; } = 1000f;             // 远裁面
        [Category("proj-both")]
        [Description("是否使用正交投影")]
        public bool isOrtho { get; set; } = false;          // 是否使用正交投影

        private Vector3 forward = -Vector3.forward;
        private Vector3 right = Vector3.right;
        [Description("相机当前的正前方")]
        public Vector3 Forward { get; private set; }
        [Description("相机当前的右方")]
        public Vector3 Right { get; private set; }
        [Description("相机当前的上方")]
        public Vector3 Up { get; private set; }

        public Vector3 R_Before_T { get; set; }

        public Camera()
        {
            View = Matrix4x4.Get();
        }
        public void Move(Vector3 t)
        {
            Translate += t;
        }
        public void TranslateTo(Vector3 t)
        {
            Translate = t;
        }
        public void RotateX(float v)
        {
            Euler += new Vector3(v, 0, 0);
        }
        public void RotateY(float v)
        {
            Euler += new Vector3(0, v, 0);
        }
        public void RotateZ(float v)
        {
            Euler += new Vector3(0, 0, v);
        }
        public void Rotate(Vector3 e)
        {
            Euler += e;
        }
        public void RotateTo(Vector3 e)
        {
            Euler = e;
        }
        public void Update(float delaMs)
        {
            // view
            Vector3 t = this.Translate;
            if (target.HasValue)
            {
                // look at
                View = Matrix4x4.GenLookAt(t, target.Value, up);
                // 将view matrix的前三行三列(3x3)，每一行对应：Left, Up, Forward三轴，来求得各个轴的当前角度
                // 因为LookAt实际上是求了view的反向变换：逆矩阵，下面的矩阵的转置为原来的变换矩阵
                /*
                 * view 是右手坐标系
                 * lx,ly,lz是x，左边为正，所以命名为：Left:l
                 * ux,uy,uz是y，上边为正，所以命名为：Up:u
                 * fx,fy,fz是z，向前为正，所以命名为：Forward:f
                 * http://www.songho.ca/opengl/gl_camera.html#lookat
                 |lx|ly|lz|0 |  |1 |0 |0 |-tx|  |lx|ly|lz|lx*(-tx)+ly*(-ty)+lz*(-tz)|
                 |ux|uy|uz|0 |* |0 |1 |0 |-ty|= |ux|uy|uz|ux*(-tx)+uy*(-ty)+uz*(-tz)|
                 |fx|fy|fz|0 |  |0 |0 |1 |-tz|  |fx|fy|fz|fx*(-tx)+fy*(-ty)+fz*(-tz)|
                 |0 | 0| 0|1 |  |0 |0 |0 |1  |  |0 |0 |0 |1                         |
                 * */
                // 后面再实现
            }
            else
            {
                t = -t;
                var r = R_Before_T;
                // translate
                View = Matrix4x4.GenTranslateMat(t.x, t.y, t.z) * Matrix4x4.GenEulerMat(r.x, r.y, r.z);
                // rotate
                View = Matrix4x4.GenEulerMat(Euler.x, Euler.y, Euler.z).MulMat(View);
            }
            var matV = Matrix4x4.GenEulerMat(-Euler.x, -Euler.y, -Euler.z);
            Forward = matV * forward;
            Right = matV * right;
            Up = Forward.Cross(Right);
            // proj
            if (isOrtho)
            {
                // orthogonal
                Proj = Matrix4x4.GenOrthoFrustum(aspect, size, near, far);
            }
            else
            {
                // perspective
                Proj = Matrix4x4.GenFrustum(fov, aspect, near, far);
            }
        }
        public void Dispose()
        {
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Description("游戏对象")]
    public class GameObject : IDisposable
    {
        private static readonly int MVP_Hash = "MVP".GetHashCode();
        private static readonly int M_Hash = "M".GetHashCode();
        private static readonly int P_Hash = "P".GetHashCode();
        private static readonly int M_IT_Hash = "M_IT".GetHashCode();
        private static readonly int MV_IT_Hash = "MV_IT".GetHashCode();

        public VertexBuffer VertexBuffer { get; private set; }
        public IndexBuffer IndexBuffer { get; private set; }

        public Matrix4x4 ModelMat { get; private set; }
        public Matrix4x4 ModelViewMat { get; private set; }
        public Matrix4x4 ModelViewProjMat { get; private set; }
        public Matrix4x4 ProjMat { get; private set; }

        public Matrix4x4 ModelITMat { get; private set; }
        public Matrix4x4 ModelViewITMat { get; private set; }

        [Description("名字")]
        public string Name { get; set; }

        [Description("父对象")]
        public GameObject Parent { get; private set; }
        [Description("本地坐标")]
        public Vector3 LocalPosition { get; set; } = Vector3.zero;
        [Description("本地旋转")]
        public Vector3 LocalRotation { get; set; } = Vector3.zero;
        [Description("本地缩放")]
        public Vector3 LocalScale { get; set; } = Vector3.one;
        [Description("网格对象，后面重构成Component")]
        public Mesh Mesh { get; set; }
        [Description("网格渲染器，后面重构成Component")]
        public MeshRenderer MR { get; set; }
        [Description("材质对象，后面重构成Component")]
        public Material Material { get; set; }
        [Description("世界坐标")]
        public Vector3 WorldPosition { get; private set; }

        public GameObject(string name = null)
        {
            ModelMat = Matrix4x4.Get();
            ModelITMat = Matrix4x4.Get();
            ModelViewITMat = Matrix4x4.Get();

            Mesh = new Mesh();
            Name = name;

            MR = new MeshRenderer();
            MR.Mesh = Mesh;
        }

        public void UpdateTransform(Camera camera)
        {
            ModelMat.Identity();
            ModelMat = ModelMat.TRS(LocalPosition, LocalRotation, LocalScale);
            ModelITMat.CopyFrom(ModelMat.m);
            ModelITMat.Invert();
            ModelITMat.Transpose();

            if (Parent != null)
            {
                ModelMat = Parent.ModelMat * ModelMat;
            }

            ModelViewMat = camera.View * ModelMat;
            ModelViewProjMat = camera.Proj * ModelViewMat;
            ProjMat = camera.Proj;

            ModelViewITMat.CopyFrom(ModelViewMat);
            ModelViewITMat.Invert();
            ModelViewITMat.Transpose();

            WorldPosition = ModelMat * Vector4.zeroPos;
        }

        public void Draw()
        {
            if (VertexBuffer == null)
            {
                var perVertexCount = 0;
                if (Mesh.vertices != null && Mesh.vertices.Length > 0)
                    perVertexCount += 3;
                if (Mesh.uv != null && Mesh.uv.Length > 0)
                    perVertexCount += 2;
                if (Mesh.colors != null && Mesh.colors.Length > 0)
                    perVertexCount += 4;
                if (Mesh.normals != null && Mesh.normals.Length > 0)
                    perVertexCount += 3;
                if (Mesh.tangents != null && Mesh.tangents.Length > 0)
                    perVertexCount += 3;

                var count = perVertexCount * Mesh.vertices.Length;

                // 定义顶点格式
                VertexBuffer = new VertexBuffer(count, perVertexCount);

                var offset = 0;

                List<VertexDataFormat> dataformList = new List<VertexDataFormat>();
                var lastCount = 0;

                if (Mesh.vertices != null && Mesh.vertices.Length > 0)
                    dataformList.Add(new VertexDataFormat { type = VertexDataType.Position, location = 0, offset = offset += lastCount, count = lastCount = 3 });
                if (Mesh.uv != null && Mesh.uv.Length > 0)
                    dataformList.Add(new VertexDataFormat { type = VertexDataType.UV, location = 0, offset = offset += lastCount, count = lastCount = 2 });
                if (Mesh.colors != null && Mesh.colors.Length > 0)
                    dataformList.Add(new VertexDataFormat { type = VertexDataType.Color, location = 0, offset = offset += lastCount, count = lastCount = 4 });
                if (Mesh.normals != null && Mesh.normals.Length > 0)
                    dataformList.Add(new VertexDataFormat { type = VertexDataType.Normal, location = 0, offset = offset += lastCount, count = lastCount = 3 });
                if (Mesh.tangents != null && Mesh.tangents.Length > 0)
                    dataformList.Add(new VertexDataFormat { type = VertexDataType.Tangent, location = 0, offset = offset += lastCount, count = lastCount = 3 });

                VertexBuffer.SetFormat(dataformList.ToArray());

                // 顶点装配索引
                IndexBuffer = new IndexBuffer(Mesh.triangles.Length);
                IndexBuffer.Set(Mesh.triangles);

                // VertexBuffer按需是否需要实时更新到Shader，如果没有变换就不需要，一般不会有变化
                VertexBuffer.writePos = 0;
                var len = Mesh.vertices.Length;
                for (int i = 0; i < len; i++)
                {
                    if (Mesh.vertices != null && Mesh.vertices.Length > 0)
                        VertexBuffer.Write(Mesh.vertices[i]);
                    if (Mesh.uv != null && Mesh.uv.Length > 0)
                        VertexBuffer.Write(Mesh.uv[i]);
                    if (Mesh.colors != null && Mesh.colors.Length > 0)
                        VertexBuffer.Write(Mesh.colors[i]);
                    if (Mesh.normals != null && Mesh.normals.Length > 0)
                        VertexBuffer.Write(Mesh.normals[i]);
                    if (Mesh.tangents != null && Mesh.tangents.Length > 0)
                        VertexBuffer.Write(Mesh.tangents[i]);
                }
            }

            MR.Renderer.BindVertexBuff(VertexBuffer);
            MR.Renderer.BindIndexBuff(IndexBuffer);
            
            MR.Renderer.Shader = Material.Shader;

            Material.Shader.ShaderProperties.SetUniform(MVP_Hash, ModelViewProjMat);
            Material.Shader.ShaderProperties.SetUniform(M_Hash, ModelMat);
            Material.Shader.ShaderProperties.SetUniform(P_Hash, ProjMat);
            Material.Shader.ShaderProperties.SetUniform(M_IT_Hash, ModelITMat);
            Material.Shader.ShaderProperties.SetUniform(MV_IT_Hash, ModelViewITMat);
            
            MR.Renderer.Present();
        }

        public void Update(float deltaMs)
        {
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            if (VertexBuffer != null)
            {
                VertexBuffer.Dispose();
                VertexBuffer = null;
            }
            if (IndexBuffer != null)
            {
                IndexBuffer.Dispose();
                IndexBuffer = null;
            }
        }
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    [Description("材质")]
    public class Material : IDisposable
    {
        public bool DisposedShdaer = false;
        public ShaderBase Shader;

        public Material(ShaderBase shader)
        {
            Shader = shader;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            if (DisposedShdaer)
            {
                if (Shader != null)
                {
                    Shader.Dispose();
                    Shader = null;
                }
            }
        }
    }
}
