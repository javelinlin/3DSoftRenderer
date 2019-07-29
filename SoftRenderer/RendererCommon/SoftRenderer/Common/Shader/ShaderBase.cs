// jave.lin 2019.07.21
using RendererCommon.SoftRenderer.Common.Attributes;
using SoftRenderer.Common.Mathes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;

namespace RendererCommon.SoftRenderer.Common.Shader
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

    [Description("Shader字段反射")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ShaderFieldReflection : IDisposable
    {
        private Dictionary<string, FieldInfo> uniformFieldDictName = new Dictionary<string, FieldInfo>();
        private Dictionary<int, FieldInfo> uniformFieldDictHash = new Dictionary<int, FieldInfo>();

        private Dictionary<string, FieldInfo> inFieldDictName = new Dictionary<string, FieldInfo>();
        private Dictionary<int, FieldInfo> inFieldDictHash = new Dictionary<int, FieldInfo>();

        private Dictionary<InLayout, Dictionary<int, FieldInfo>> inLayoutFieldDict = new Dictionary<InLayout, Dictionary<int, FieldInfo>>();
        private Dictionary<OutLayout, Dictionary<int, FieldInfo>> outLayoutFieldDict = new Dictionary<OutLayout, Dictionary<int, FieldInfo>>();

        public ShaderBase Shader { get; private set; }

        public ShaderFieldReflection(ShaderBase shader)
        {
            Shader = shader;

            var type = shader.GetType();
            var fs = type.GetFields();
            var uniformAtType = typeof(UniformAttribute);
            var inAtType = typeof(InAttribute);
            var outAtType = typeof(OutAttribute);
            var posAtType = typeof(PositionAttribute);
            var colorAtType = typeof(ColorAttribute);
            var uvAtType = typeof(TexcoordAttribute);
            var normalAtType = typeof(NormalAttribute);
            var tangentAtType = typeof(TangentAttribute);
            var svposAtType = typeof(SV_PositionAttribute);
            var svtargetAtType = typeof(SV_TargetAttribute);
            foreach (var f in fs)
            {
                foreach (var at in f.CustomAttributes)
                {
                    // 收集uniform字段
                    if (at.AttributeType.IsEquivalentTo(uniformAtType))
                    {
                        var conflict = f.GetCustomAttribute(outAtType); // conflict out
                        if (conflict != null)
                        {
                            throw new Exception($"Shader:{type.Name} field:{f.Name}'s {at.AttributeType.Name} conflict, it also has {conflict.GetType().Name}");
                        }
                        conflict = f.GetCustomAttribute(inAtType);  // conflict in
                        if (conflict != null)
                        {
                            throw new Exception($"Shader:{type.Name} field:{f.Name}'s {at.AttributeType.Name} conflict, it also has {conflict.GetType().Name}");
                        }
                        uniformFieldDictName[f.Name] = f;
                        uniformFieldDictHash[f.Name.GetHashCode()] = f;
                    }
                    // 收集in字段
                    else if (at.AttributeType.IsEquivalentTo(inAtType))
                    {
                        // TODO 后面可以让一个字段同时是in也可以同时是out
                        var conflict = f.GetCustomAttribute(outAtType); // conflict out
                        if (conflict != null)
                        {
                            throw new Exception($"Shader:{type.Name} field:{f.Name}'s {at.AttributeType.Name} conflict, it also has {conflict.GetType().Name}");
                        }
                        conflict = f.GetCustomAttribute(uniformAtType); // conflict uniform
                        if (conflict != null)
                        {
                            throw new Exception($"Shader:{type.Name} field:{f.Name}'s {at.AttributeType.Name} conflict, it also has {conflict.GetType().Name}");
                        }
                        inFieldDictName[f.Name] = f;
                        inFieldDictHash[f.Name.GetHashCode()] = f;

                        var svpos = f.GetCustomAttribute(svposAtType);
                        var pos = f.GetCustomAttribute(posAtType);
                        var color = f.GetCustomAttribute(colorAtType);
                        var uv = f.GetCustomAttribute(uvAtType);
                        var normal = f.GetCustomAttribute(normalAtType);
                        var tangent = f.GetCustomAttribute(tangentAtType);
                        Dictionary<int, FieldInfo> dict = null;
                        InLayout layout = InLayout.SV_Position;
                        if (svpos != null) layout = InLayout.SV_Position;
                        if (pos != null) layout = InLayout.Position;
                        if (color != null) layout = InLayout.Color;
                        if (uv != null) layout = InLayout.Texcoord;
                        if (normal != null) layout = InLayout.Normal;
                        if (tangent != null) layout = InLayout.Tangent;
                        inLayoutFieldDict.TryGetValue(layout, out dict);
                        if (dict == null)
                            inLayoutFieldDict[layout] = dict = new Dictionary<int, FieldInfo>();
                        var num = at.ConstructorArguments.Count > 0 ? (int)at.ConstructorArguments[0].Value : 0;
                        dict[num] = f;
                    }
                    else if (at.AttributeType.IsEquivalentTo(outAtType))
                    {
                        var conflict = f.GetCustomAttribute(inAtType); // conflict in
                        if (conflict != null)
                        {
                            throw new Exception($"Shader:{type.Name} field:{f.Name}'s {at.AttributeType.Name} conflict, it also has {conflict.GetType().Name}");
                        }
                        conflict = f.GetCustomAttribute(uniformAtType); // conflict uniform
                        if (conflict != null)
                        {
                            throw new Exception($"Shader:{type.Name} field:{f.Name}'s {at.AttributeType.Name} conflict, it also has {conflict.GetType().Name}");
                        }
                        inFieldDictName[f.Name] = f;
                        inFieldDictHash[f.Name.GetHashCode()] = f;

                        var svpos = f.GetCustomAttribute(svposAtType);
                        var svtarget = f.GetCustomAttribute(svtargetAtType);
                        var pos = f.GetCustomAttribute(posAtType);
                        var color = f.GetCustomAttribute(colorAtType);
                        var uv = f.GetCustomAttribute(uvAtType);
                        var normal = f.GetCustomAttribute(normalAtType);
                        var tangent = f.GetCustomAttribute(tangentAtType);
                        Dictionary<int, FieldInfo> dict = null;
                        OutLayout layout = OutLayout.Tangent;
                        if (svpos != null) layout = OutLayout.SV_Position;
                        if (svtarget != null) layout = OutLayout.SV_Target;
                        if (pos != null) layout = OutLayout.Position;
                        if (color != null) layout = OutLayout.Color;
                        if (uv != null) layout = OutLayout.Texcoord;
                        if (normal != null) layout = OutLayout.Normal;
                        //if (tangent != null) layout = OutLayout.Tangent; // 默认是Tangent
                        outLayoutFieldDict.TryGetValue(layout, out dict);
                        if (dict == null)
                            outLayoutFieldDict[layout] = dict = new Dictionary<int, FieldInfo>();
                        var num = at.ConstructorArguments.Count > 0 ? (int)at.ConstructorArguments[0].Value : 0;
                        dict[num] = f;
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
            uniformFieldDictHash[hash].SetValue(Shader, value);
        }

        // 根据输入限定符来输入数据
        public void SetIn<T>(InLayout layout, T value, int num = 0)
        {
            inLayoutFieldDict[layout][num].SetValue(Shader, value);
        }

        // 根据输出限定符来输入数据
        public void SetInWithOut<T>(OutLayout layout, T value, int num = 0)
        {
            if (layout == OutLayout.SV_Target)
                return;
            InLayout inlayout = InLayout.SV_Position;
            if (layout == OutLayout.SV_Position)/* noops */;
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

        public T GetOut<T>(OutLayout layout, int num = 0) where T: struct
        {
            return (T)outLayoutFieldDict[layout][num].GetValue(Shader);
        }

        public OutInfo[] GetOuts()
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
                    result[idx++] = new OutInfo {
                        layout = kv.Key, location = kkvv.Key, value = kkvv.Value.GetValue(Shader)
                    };
                }
            }
            return result;
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
            if (inFieldDictName != null)
            {
                inFieldDictName.Clear();
                inFieldDictName = null;
            }
            if (inFieldDictHash != null)
            {
                inFieldDictHash.Clear();
                inFieldDictHash = null;
            }
            if (inLayoutFieldDict != null)
            {
                foreach (var kv in inLayoutFieldDict)
                {
                    kv.Value.Clear();
                }
                inLayoutFieldDict.Clear();
                inLayoutFieldDict = null;
            }
            if (outLayoutFieldDict != null)
            {
                foreach (var kv in outLayoutFieldDict)
                {
                    kv.Value.Clear();
                }
                outLayoutFieldDict.Clear();
                outLayoutFieldDict = null;
            }
            Shader = null;
        }
    }

    [Description("着色器的输出数据")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public struct OutInfo
    {
        public OutLayout layout;
        public int location;
        public object value;

        public T Get<T>()
        {
            return (T)value;
        }
    }

    [Description("Shader基类")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ShaderBase : IDisposable
    {
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
        protected static ColorNormalized tex2D(Sampler2D sampler, Texture2D tex, Vector2 uv)=> sampler.Sample(tex, uv);
        protected static float pow(float v, float times) => (float)Math.Pow(v, times);
        protected static float min(float a, float b) => Mathf.Min(a, b);
        protected static float max(float a, float b) => Mathf.Max(a, b);

        public ShaderFieldReflection ShaderProperties;
        [SharedData]
        public BasicShaderData Data { get; private set; }
        public ShaderBase(BasicShaderData data)
        {
            this.Data = data;

            ShaderProperties = new ShaderFieldReflection(this);
        }

        public virtual void Begin()
        {

        }

        [Main]
        public virtual void Main()
        {

        }

        public virtual void End()
        {

        }

        public virtual void Dispose()
        {
            GC.SuppressFinalize(this);

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
        public bool discard;

        public FSBase(BasicShaderData data) : base(data)
        {
        }

        public virtual void Reset()
        {
            discard = false;
        }
    }
}
