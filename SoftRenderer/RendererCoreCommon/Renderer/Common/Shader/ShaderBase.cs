// jave.lin 2019.07.21
#define SHOW_WARNING_NOT_FOUND_SHADER_FIELD // 没有找到Shader字段时，提示警告

using RendererCoreCommon.Renderer.Common.Attributes;
using RendererCoreCommon.Renderer.Common.Mathes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Reflection;

namespace RendererCoreCommon.Renderer.Common.Shader
{
    [Description("输入的布局限定符")]
    public enum InLayout
    {
        SV_Position,
        Position,
        Color,
        Texcoord,
        Normal,
        Tangent,
    }

    [Description("输出的布局限定符")]
    public enum OutLayout
    {
        SV_Position,
        SV_Target,
        Position,
        Color,
        Texcoord,
        Normal,
        Tangent,        // 暂时没用到
    }

    [Description("布局符使用的浮点个数")]
    public enum LayoutFloatNum
    {
        Undefine,
        F2,
        F1,
        F3,
        F4,
    }

    [Description("FuncData字段类")]
    public class FuncField : IDisposable
    {
        public FuncFieldReflection<FuncField> Properties { get; private set; }
        public Pass Pass { get; private set; }

        public FuncField(Pass pass)
        {
            Pass = pass;
            Properties = new FuncFieldReflection<FuncField>(this);
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);

            if (Properties != null)
            {
                Properties.Dispose();
                Properties = null;
            }
            Pass = null;
        }
    }

    [Description("FuncData字段反射")]
    public class FuncFieldReflection<T> : IDisposable
    {
        private Dictionary<string, FieldInfo> inFieldDictName = new Dictionary<string, FieldInfo>();
        private Dictionary<int, FieldInfo> inFieldDictHash = new Dictionary<int, FieldInfo>();

        private Dictionary<InLayout, Dictionary<int, FieldInfo>> inLayoutFieldDict = new Dictionary<InLayout, Dictionary<int, FieldInfo>>();
        private Dictionary<OutLayout, Dictionary<int, FieldInfo>> outLayoutFieldDict = new Dictionary<OutLayout, Dictionary<int, FieldInfo>>();

        private Dictionary<FieldInfo, bool> nointerpolationFlagDict = new Dictionary<FieldInfo, bool>();
        private Dictionary<FieldInfo, LayoutFloatNum> layoutFloatNumDict = new Dictionary<FieldInfo, LayoutFloatNum>();

        public T Owner { get; private set; }
        public FuncFieldReflection(T owner)
        {
            Owner = owner;
            var type = owner.GetType();
            var fs = type.GetFields();
            var uniformAtType = typeof(UniformAttribute);
            var inAtType = typeof(InAttribute);
            var outAtType = typeof(OutAttribute);
            var nointerpolation = typeof(NointerpolationAttribute);

            var fType = typeof(float);
            var vec2Type = typeof(Vector2);
            var vec3Type = typeof(Vector3);
            var vec4Type = typeof(Vector4);

            foreach (var f in fs)
            {
                foreach (var at in f.CustomAttributes)
                {
                    // 收集in字段
                    if (at.AttributeType.IsEquivalentTo(inAtType))
                    {
                        var conflict = f.GetCustomAttribute<UniformAttribute>(); // conflict uniform
                        if (conflict != null)
                        {
                            throw new Exception($"Shader:{type.Name} field:{f.Name}'s {at.AttributeType.Name} conflict, it also has {conflict.GetType().Name}");
                        }
                        inFieldDictName[f.Name] = f;
                        inFieldDictHash[f.Name.GetHashCode()] = f;

                        var svpos = f.GetCustomAttribute<SV_PositionAttribute>();
                        var svtarget = f.GetCustomAttribute<SV_TargetAttribute>();
                        var pos = f.GetCustomAttribute<PositionAttribute>();
                        var color = f.GetCustomAttribute<ColorAttribute>();
                        var uv = f.GetCustomAttribute<TexcoordAttribute>();
                        var normal = f.GetCustomAttribute<NormalAttribute>();
                        var tangent = f.GetCustomAttribute<TangentAttribute>();
                        Dictionary<int, FieldInfo> dict = null;
                        InLayout layout = InLayout.SV_Position;
                        var location = 0;
                        if (svpos != null) { layout = InLayout.SV_Position; location = 0; } // only one:0
                        if (pos != null) { layout = InLayout.Position; location = 0; } // only one:0
                        if (color != null) { layout = InLayout.Color; location = color.Location; }
                        if (uv != null) { layout = InLayout.Texcoord; location = uv.Location; }
                        if (normal != null) { layout = InLayout.Normal; location = normal.Location; }
                        if (tangent != null) { layout = InLayout.Tangent; location = tangent.Location; }
                        inLayoutFieldDict.TryGetValue(layout, out dict);
                        if (dict == null)
                            inLayoutFieldDict[layout] = dict = new Dictionary<int, FieldInfo>();
                        dict[location] = f;
                    }
                    else if (at.AttributeType.IsEquivalentTo(outAtType))
                    {
                        var uniformConflict = f.GetCustomAttribute<UniformAttribute>(); // conflict uniform
                        if (uniformConflict != null)
                        {
                            throw new Exception($"Shader:{type.Name} field:{f.Name}'s {at.AttributeType.Name} conflict, it also has {uniformConflict.GetType().Name}");
                        }
                        inFieldDictName[f.Name] = f;
                        inFieldDictHash[f.Name.GetHashCode()] = f;

                        var svpos = f.GetCustomAttribute<SV_PositionAttribute>();
                        var svtarget = f.GetCustomAttribute<SV_TargetAttribute>();
                        var pos = f.GetCustomAttribute<PositionAttribute>();
                        var color = f.GetCustomAttribute<ColorAttribute>();
                        var uv = f.GetCustomAttribute<TexcoordAttribute>();
                        var normal = f.GetCustomAttribute<NormalAttribute>();
                        var tangent = f.GetCustomAttribute<TangentAttribute>();
                        Dictionary<int, FieldInfo> dict = null;
                        OutLayout layout = OutLayout.Tangent;
                        var location = 0;
                        if (svpos != null) { layout = OutLayout.SV_Position; location = 0; } // only one:0
                        if (svtarget != null) { layout = OutLayout.SV_Target; location = svtarget.Location; }
                        if (pos != null) { layout = OutLayout.Position; location = 0; } // only one:0
                        if (color != null) { layout = OutLayout.Color; location = color.Location; }
                        if (uv != null) { layout = OutLayout.Texcoord; location = uv.Location; }
                        if (normal != null) { layout = OutLayout.Normal; location = normal.Location; }
                        if (tangent != null) { layout = OutLayout.Tangent; location = tangent.Location; }
                        outLayoutFieldDict.TryGetValue(layout, out dict);
                        if (dict == null)
                            outLayoutFieldDict[layout] = dict = new Dictionary<int, FieldInfo>();
                        dict[location] = f;
                    }
                    else if (at.AttributeType.IsEquivalentTo(nointerpolation))
                    {
                        nointerpolationFlagDict[f] = true;
                    }
                }

                var floatNum = LayoutFloatNum.Undefine;
                if (f.FieldType == fType) floatNum = LayoutFloatNum.F1;
                else if (f.FieldType == vec2Type) floatNum = LayoutFloatNum.F2;
                else if (f.FieldType == vec3Type) floatNum = LayoutFloatNum.F3;
                else if (f.FieldType == vec4Type) floatNum = LayoutFloatNum.F4;

                layoutFloatNumDict[f] = floatNum;
            }
        }

        // 根据输入限定符来输入数据
        public void SetIn<T>(InLayout layout, T value, int location = 0)
        {
#if SHOW_WARNING_NOT_FOUND_SHADER_FIELD
            if (inLayoutFieldDict.TryGetValue(layout, out Dictionary<int, FieldInfo> dict))
            {
                if (dict.TryGetValue(location, out FieldInfo f))
                {
                    f.SetValue(Owner, value);
                }
                else
                    ;// Console.WriteLine($"Not found the location:{location}");
            }
            else
                ;// Console.WriteLine($"Not found the layout:{layout}");
#else
            inLayoutFieldDict[layout][location].SetValue(Owner, value);
#endif
        }

        // 根据输出限定符来输入数据
        public void SetInWithOut<T>(OutLayout layout, T value, int num = 0)
        {
            if (layout == OutLayout.SV_Target)
                return;
            InLayout inlayout = InLayout.SV_Position;
            if (layout == OutLayout.SV_Position) /* noops */;
            else if (layout == OutLayout.Position)
                inlayout = InLayout.Position;
            else if (layout == OutLayout.Color)
                inlayout = InLayout.Color;
            else if (layout == OutLayout.Normal)
                inlayout = InLayout.Normal;
            else if (layout == OutLayout.Texcoord)
                inlayout = InLayout.Texcoord;
            else if (layout == OutLayout.Tangent)
                inlayout = InLayout.Tangent;
            else
                throw new Exception("not implements");
            SetIn<T>(inlayout, value, num);
        }

        public T GetOut<T>(OutLayout layout, int num = 0) where T : struct
        {
            return (T)outLayoutFieldDict[layout][num].GetValue(Owner);
        }

        public OutTargetInfo[] GetTargetOut()
        {
            outLayoutFieldDict.TryGetValue(OutLayout.SV_Target, out Dictionary<int, FieldInfo> dict);
            var result = new OutTargetInfo[dict.Count];
            if (dict.Count > 0)
            {
                var idx = 0;
                foreach (var kv in dict)
                {
                    result[idx++] = new OutTargetInfo
                    {
                        localtion = kv.Key,
                        data = (Vector4)kv.Value.GetValue(Owner),
                    };
                }
            }
            return result;
        }

        public OutInfo[] GetVertexOuts()
        {
            var count = 0;
            foreach (var kv in outLayoutFieldDict)
            {
                count += kv.Value.Count;
            }
            var result = new OutInfo[count];
            var idx = 0;
            foreach (var kv in outLayoutFieldDict)
            {
                foreach (var kkvv in kv.Value)
                {
                    nointerpolationFlagDict.TryGetValue(kkvv.Value, out bool nointerpolation);
                    layoutFloatNumDict.TryGetValue(kkvv.Value, out LayoutFloatNum floatNum);
                    result[idx++] = new OutInfo
                    {
                        layout = kv.Key,
                        location = kkvv.Key,
                        value = kkvv.Value.GetValue(Owner),
                        nointerpolation = nointerpolation,
                        floatNum = floatNum,
                    };
                }
            }
            return result;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            inFieldDictName = null;
            inFieldDictHash = null;
            inLayoutFieldDict = null;
            outLayoutFieldDict = null;
            nointerpolationFlagDict = null;
            layoutFloatNumDict = null;

            Owner = default(T);
        }
    }

    [Description("Shader字段反射")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ShaderFieldReflection : IDisposable
    {
        private Dictionary<string, FieldInfo> uniformFieldDictName = new Dictionary<string, FieldInfo>();
        private Dictionary<int, FieldInfo> uniformFieldDictHash = new Dictionary<int, FieldInfo>();

        public ShaderBase Shader { get; private set; }

        public ShaderFieldReflection(ShaderBase shader)
        {
            Shader = shader;

            var type = shader.GetType();
            var fs = type.GetFields();
            var uniformAtType = typeof(UniformAttribute);

            foreach (var f in fs)
            {
                foreach (var at in f.CustomAttributes)
                {
                    // 收集uniform字段
                    if (at.AttributeType.IsEquivalentTo(uniformAtType))
                    {
                        var outConflict = f.GetCustomAttribute<OutAttribute>(); // conflict out
                        if (outConflict != null)
                        {
                            throw new Exception($"Shader:{type.Name} field:{f.Name}'s {at.AttributeType.Name} conflict, it also has {outConflict.GetType().Name}");
                        }
                        var inConflict = f.GetCustomAttribute<InAttribute>();  // conflict in
                        if (inConflict != null)
                        {
                            throw new Exception($"Shader:{type.Name} field:{f.Name}'s {at.AttributeType.Name} conflict, it also has {inConflict.GetType().Name}");
                        }
                        uniformFieldDictName[f.Name] = f;
                        uniformFieldDictHash[f.Name.GetHashCode()] = f;
                    }
                }
            }
        }

        // 以name指定变量，设置uniform变量的数据，
        public void SetUniform<T>(string name, T value)
        {
            uniformFieldDictName[name].SetValue(Shader, value);
        }

        // 以hash指定变量，设置uniform变量的数据，
        public void SetUniform<T>(int hash, T value)
        {
#if SHOW_WARNING_NOT_FOUND_SHADER_FIELD
            if (uniformFieldDictHash.TryGetValue(hash, out FieldInfo f))
            {
                if (f != null)
                {
                    f.SetValue(Shader, value);
                }
                else
                    ; // Console.WriteLine($"not found uniform field:{hash}");
            }
            else
                ; // Console.WriteLine($"not found uniform field:{hash}");
#else
            uniformFieldDictHash[hash].SetValue(Shader, value);
#endif
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            if (uniformFieldDictName != null)
            {
                uniformFieldDictName.Clear();
                uniformFieldDictName = null;
            }
            if (uniformFieldDictHash != null)
            {
                uniformFieldDictHash.Clear();
                uniformFieldDictHash = null;
            }
            Shader = null;
        }
    }

    [Description("着色器的输出数据")]
    public struct OutInfo
    {
        public OutLayout layout;
        public LayoutFloatNum floatNum;
        public int location;
        public object value;
        public bool nointerpolation;

        public T Get<T>()
        {
            return (T)value;
        }
    }

    [Description("像素着色器的输出数据")]
    public struct OutTargetInfo
    {
        public int localtion;
        public Vector4 data;
    }

    [Description("着色器的输出数据容器")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ShaderOut
    {
        public bool clip;
        public OutInfo[] upperStageOutInfos;
    }

    [Description("新版-片段数据")]
    public class FragInfo : IDisposable
    {
        public static FragInfo GetFragInfo(ShaderOut shaderOut = null)
        {
            return new FragInfo(shaderOut);
        }

        public float depth;
        public bool discard;

        public Vector4 normalLineColor; // 调试用

        public Vector4 p;

        public float ClipZ { get => p.w; }

        public int PosIdx { get; private set; }
        public ShaderOut ShaderOut { get; private set; }

        public FragInfo(ShaderOut shaderOut = null)
        {
            if (shaderOut != null)
            {
                Set(shaderOut);
            }
        }

        public void Set(ShaderOut shaderOut)
        {
            var upperStageOutInfos = shaderOut.upperStageOutInfos;
            var len = upperStageOutInfos.Length;
            for (int i = 0; i < len; i++)
            {
                if (upperStageOutInfos[i].layout == OutLayout.SV_Position)
                {
                    PosIdx = i;
                    p = upperStageOutInfos[i].Get<Vector4>();
                    break;
                }
            }
            if (PosIdx == -1)
                throw new Exception("error");
            ShaderOut = shaderOut;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

        }
    }

    [Description("Shader.SubShader.Pass.DrawState")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class DrawState
    {
        [Category("Blend")] public Blend Blend { get; set; } = Blend.Off;
        [Category("Blend")] public BlendFactor BlendSrcColorFactor { get; set; } = BlendFactor.SrcAlpha;
        [Category("Blend")] public BlendFactor BlendDstColorFactor { get; set; } = BlendFactor.OneMinusSrcAlpha;
        [Category("Blend")] public BlendFactor BlendSrcAlphaFactor { get; set; } = BlendFactor.One;
        [Category("Blend")] public BlendFactor BlendDstAlphaFactor { get; set; } = BlendFactor.Zero;
        [Category("Blend")] public BlendOp BlendColorOp { get; set; } = BlendOp.Add;
        [Category("Blend")] public BlendOp BlendAlphaOp { get; set; } = BlendOp.Add;

        [Category("Scissor")] public Rectangle ScissorRect { get; set; }
        [Category("Scissor")] public Scissor Scissor { get; set; } = Scissor.Off;

        [Category("Facing & Culling")] public FrontFace FrontFace { get; set; } = FrontFace.Clock;
        [Category("Facing & Culling")] public FaceCull Cull { get; set; } = FaceCull.Back;

        [Category("Depth")] public DepthWrite DepthWrite { get; set; } = DepthWrite.On;
        [Category("Depth")] public ComparisonFunc DepthTest { get; set; } = ComparisonFunc.Less;
        [Category("Depth")] public DepthOffset DepthOffset { get; set; } = DepthOffset.Off;
        [Description("Depth的掠射角偏移系数")]
        [Category("Depth")] public float DepthOffsetFactor { get; set; } = 0;
        [Description("Depth的最小深度刻度单位偏移系数")]
        [Category("Depth")] public float DepthOffsetUnit { get; set; } = 0;

        [Category("PolygonMode")] public PolygonMode PolygonMode { get; set; } = PolygonMode.Triangle;

        [Category("Stencil")] public Stencil Stencil { get; set; } = Stencil.Off;
        [Category("Stencil")] public byte StencilRef { get; set; }                                          // required
        [Category("Stencil")] public ComparisonFunc StencilComp { get; set; } = ComparisonFunc.Always;      // required
        [Category("Stencil")] public StencilOp StencilPass { get; set; }                                    // required
        [Category("Stencil")] public StencilOp StencilFail { get; set; }                                    // required
        [Category("Stencil")] public StencilOp StencilZFail { get; set; }                                   // required
        [Category("Stencil")] public byte StencilReadMask { get; set; } = 0xff;                             // optional
        [Category("Stencil")] public byte StencilWriteMask { get; set; } = 0xff;                            // optional
    }

    [Description("Shader.SubShader.Pass")]
    public class Pass : IDisposable
    {
        public DrawState State { get; protected set; }

        public SubShader SubShader { get; private set; }

        public bool Actived { get; set; } = true;

        public virtual FuncField VertField { get; protected set; }
        //public virtual FuncField TessCtrlField { get; protected set; }
        //public virtual FuncField TessEvaField { get; protected set; }
        //public virtual FuncField GeometryField { get; protected set; }
        public virtual FuncField FragField { get; protected set; }

        public FragInfo f;
        public bool discard;

        protected static float dot(Vector2 v1, Vector2 v2) => v1.Dot(v2);
        protected static float dot(Vector3 v1, Vector3 v2) => v1.Dot(v2);
        protected static float dot(Vector4 v1, Vector4 v2) => v1.Dot(v2);
        protected static Vector3 reflect(Vector3 i, Vector3 n) => Vector3.Reflect(i, n);
        protected static float cross(Vector2 v1, Vector2 v2) => v1.Cross(v2);
        protected static Vector3 cross(Vector3 v1, Vector3 v2) => v1.Cross(v2);
        protected static float lerp(float v1, float v2, float t) => Mathf.Lerp(v1, v2, t);
        protected static Vector2 lerp(Vector2 v1, Vector2 v2, float t) => Mathf.Lerp(v1, v2, t);
        protected static Vector3 lerp(Vector3 v1, Vector3 v2, float t) => Mathf.Lerp(v1, v2, t);
        protected static Vector4 lerp(Vector4 v1, Vector4 v2, float t) => Mathf.Lerp(v1, v2, t);
        protected static float clamp(float v, float min, float max) => Mathf.Clamp(v, min, max);
        protected static Vector2 clamp(Vector2 v, float min, float max) => Mathf.Clamp(v, min, max);
        protected static Vector3 clamp(Vector3 v, float min, float max) => Mathf.Clamp(v, min, max);
        protected static Vector4 clamp(Vector4 v, float min, float max) => Mathf.Clamp(v, min, max);
        protected static Vector4 tex2D(Sampler2D sampler, Texture2D tex, Vector2 uv) => sampler.Sample(tex, uv);
        protected static float pow(float v, float times) => (float)Math.Pow(v, times);
        protected static float min(float a, float b) => Mathf.Min(a, b);
        protected static float max(float a, float b) => Mathf.Max(a, b);

        public Pass(SubShader subshader)
        {
            State = new DrawState();
            SubShader = subshader;
        }

        public virtual void Reset()
        {
            discard = false;
        }

        public virtual void Vert() { }
        //public virtual void TessCtrl() { }
        //public virtual void TessEva() { }
        //public virtual void Geometry() { }
        public virtual void Frag() { }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);

            if (VertField != null)
            {
                VertField.Dispose();
                VertField = null;
            }
            if (FragField != null)
            {
                FragField.Dispose();
                FragField = null;
            }

            f = null;
            State = null;
            SubShader = null;
        }
    }

    [Description("Shader.SubShader")]
    public class SubShader : IDisposable
    {
        public bool IsSupported { get; private set; } = true;

        public List<Pass> passList { get; private set; }
        public ShaderBase Shader { get; private set; }

        public SubShader(ShaderBase shader)
        {
            passList = new List<Pass>();
            Shader = shader;
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);

            if (passList != null)
            {
                foreach (var p in passList)
                {
                    p.Dispose();
                }
                passList.Clear();
                passList = null;
            }
            Shader = null;
        }
    }

    public delegate void Vert();
    public delegate void TessCtrl();
    public delegate void TessEva();
    public delegate void Geometry();
    public delegate void Frag();

    public class SubShaderExt<T> : SubShader where T : ShaderBase
    {
        public T Shader_T { get; private set; }

        public SubShaderExt(T shader) : base(shader)
        {
            Shader_T = shader;
        }

        public override void Dispose()
        {
            Shader_T = default(T);

            base.Dispose();
        }
    }

    public class PassExt<T> : Pass where T : SubShader
    {
        public T SubShader_T { get; private set; }

        public PassExt(T subshader) : base(subshader)
        {
            SubShader_T = subshader;
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);

            SubShader_T = null;

            base.Dispose();
        }
    }

    [Description("Shader基类")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ShaderBase : IDisposable
    {
        public ShaderFieldReflection ShaderProperties;
        public BasicShaderData Data { get; private set; }

        public List<SubShader> SubShaderList { get; private set; }

        public ShaderBase(BasicShaderData data)
        {
            this.Data = data;

            try
            {
                ShaderProperties = new ShaderFieldReflection(this);
            }
            catch (Exception er)
            {
                Console.WriteLine($"error:{er.StackTrace}");
            }

            SubShaderList = new List<SubShader>();
        }

        public void Init()
        {
            // analyze pass
        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);

            if (SubShaderList != null)
            {
                var subshaderCount = SubShaderList.Count;
                for (int i = 0; i < subshaderCount; i++)
                {
                    SubShaderList[i].Dispose();
                }
                SubShaderList.Clear();
                SubShaderList = null;
            }

            if (ShaderProperties != null)
            {
                ShaderProperties.Dispose();
                ShaderProperties = null;
            }

            Data = null;
        }
    }

    [Description("FragmentShader基类")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class FSBase : ShaderBase
    {
        public FragInfo f;
        public bool discard;    // 是否丢弃片段, default: false
        public bool front;      // 是否正面, default: true

        public FSBase(BasicShaderData data) : base(data)
        {
        }

        public virtual void Reset()
        {
            discard = false;
            front = true;
        }
    }
}
