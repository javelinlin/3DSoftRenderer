// jave.lin 2019.07.18
using RendererCommon.SoftRenderer.Common.Attributes;
using RendererCommon.SoftRenderer.Common.Shader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;

namespace SoftRenderer.SoftRenderer
{
    [Description("Shader程序")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ShaderProgram : IDisposable
    {
        private Dictionary<ShaderType, ShaderBase> usingShader = new Dictionary<ShaderType, ShaderBase>();

        public Renderer Renderer { get; private set; }

        public ShaderProgram(Renderer renderer)
        {
            Renderer = renderer;
        }

        public void SetShader(ShaderType type, ShaderBase shader)
        {
            usingShader[type] = shader;
        }

        public ShaderBase GetShader(ShaderType type)
        {
            usingShader.TryGetValue(type, out ShaderBase shader);
            return shader;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (usingShader != null)
            {
                foreach (var kv in usingShader)
                {
                    kv.Value.Dispose();
                }
                usingShader.Clear();
                usingShader = null;
            }
        }
    }
    [Description("Shader加载管理")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ShaderLoadMgr : IDisposable
    {
        private Type baseType = typeof(ShaderBase);

        private Type vsAtType = typeof(VSAttribute);
        private Type fsAtType = typeof(FSAttribute);
        private Type inAtType = typeof(InAttribute);
        private Type outAtType = typeof(OutAttribute);
        private Type positionAtType = typeof(PositionAttribute);
        private Type colorAtType = typeof(ColorAttribute);
        private Type uvAtType = typeof(TexcoordAttribute);
        private Type normalAtType = typeof(NormalAttribute);
        private Type mainAtType = typeof(MainAttribute);
        private Type svPositionAtType = typeof(SV_PositionAttribute);
        private Type svTargetAtType = typeof(SV_TargetAttribute);

        private Dictionary<string, Type> shaderTypeDictName = new Dictionary<string, Type>();
        private Dictionary<int, Type> shaderTypeDictHash = new Dictionary<int, Type>();

        public Renderer Renderer { get; private set; }
        public ShaderLoadMgr(Renderer renderer)
        {
            Renderer = renderer;
        }

        public void Load(string path)
        {
            Console.WriteLine($"Loading Shader file start..., path:{path}");
            Load(File.ReadAllBytes(path));
            Console.WriteLine($"Loading Shader file end, path:{path}");
        }

        public void Load(byte[] bytes)
        {
            Console.WriteLine("Loading Shader bytes start...");
            var assembly = Assembly.Load(bytes);

            var module = assembly.ManifestModule;
            var types = module.GetTypes();

            string name = "Name";
            string hash = "NameHash";

            //ShaderBase vs = null;
            //ShaderBase fs = null;

            foreach (var type in types)
            {
                var methods = type.GetMethods();
                var fields = type.GetFields();
                foreach (var at in type.CustomAttributes)
                {
                    var check = type.IsSubclassOf(baseType);
                    if (!check)
                    {
                        throw new Exception("shader type invalidated");
                    }
                    var foundMainMethod = false;
                    foreach (var method in methods)
                    {
                        foreach (var mAt in method.CustomAttributes)
                        {
                            if (mAt.AttributeType.IsEquivalentTo(mainAtType))
                            {
                                foundMainMethod = true;
                                break;
                            }
                        }
                        if (foundMainMethod)
                        {
                            break;
                        }
                    }
                    if (!foundMainMethod)
                    {
                        throw new Exception($"shader:{type.Name} not found main attribute method");
                    }
                    if (at.AttributeType.IsEquivalentTo(vsAtType))
                    {
                        var foundOutSVPos = false;
                        foreach (var field in fields)
                        {
                            var foundOut = false;
                            var foundSV_Pos = false;
                            foreach (var fAt in field.CustomAttributes)
                            {
                                if (!foundOut && fAt.AttributeType.IsEquivalentTo(outAtType))
                                {
                                    foundOut = true;
                                }
                                if (!foundSV_Pos && fAt.AttributeType.IsEquivalentTo(svPositionAtType))
                                {
                                    foundSV_Pos = true;
                                }
                                if (foundOut && foundSV_Pos)
                                {
                                    foundOutSVPos = true;
                                    break;
                                }
                            }
                            if (foundOutSVPos) break;
                        }
                        if (!foundOutSVPos)
                        {
                            throw new Exception($"shader:{type.Name} not found Out & SV_Position attribute");
                        }
                        //if (vs == null)
                        //{
                        //    vs = Activator.CreateInstance(type, new[] { shaderData }) as ShaderBase;
                        //}
                    }
                    else if (at.AttributeType.IsEquivalentTo(fsAtType))
                    {
                        var foundOutSVTraget = false;
                        foreach (var field in fields)
                        {
                            var foundOut = false;
                            var foundSV_Target = false;
                            foreach (var fAt in field.CustomAttributes)
                            {
                                if (!foundOut && fAt.AttributeType.IsEquivalentTo(outAtType))
                                {
                                    foundOut = true;
                                }
                                if (!foundSV_Target && fAt.AttributeType.IsEquivalentTo(svTargetAtType))
                                {
                                    foundSV_Target = true;
                                }
                                if (foundOut && foundSV_Target)
                                {
                                    foundOutSVTraget = true;
                                    break;
                                }
                            }
                            if (foundOutSVTraget) break;
                        }
                        if (!foundOutSVTraget)
                        {
                            throw new Exception($"shader:{type.Name} not found Out & SV_Position attribute");
                        }
                        //if (fs == null)
                        //{
                        //    fs = Activator.CreateInstance(type, new[] { shaderData }) as ShaderBase;
                        //}
                    }
                    else
                    {
                        // other attributes
                        // noops
                        continue;
                    }

                    var f = type.GetField(name, BindingFlags.GetField | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static);
                    var shaderName = (string)f.GetValue(null);
                    f = type.GetField(hash, BindingFlags.GetField | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static);
                    var shaderHash = (int)f.GetValue(null);

                    if (shaderTypeDictName.ContainsKey(shaderName))
                    {
                        throw new Exception($"shaderName existing, shader:{shaderName}, type:{type.Name}, in buff shader.type:{shaderTypeDictName[shaderName].Name}");
                    }
                    if (shaderTypeDictHash.ContainsKey(shaderHash))
                    {
                        throw new Exception($"shaderName existing, shaderHash:{shaderHash}, type:{type.Name}, in buff shader.type:{shaderTypeDictName[shaderName].Name}");
                    }
                    shaderTypeDictName[shaderName] = type;
                    shaderTypeDictHash[shaderHash] = type;

                    Console.WriteLine($"Loading shader type:{type.Name} complete!");
                }
            }

            Console.WriteLine("Loading Shader bytes end.");
        }

        public Type GetShaderType(string name)
        {
            return shaderTypeDictName[name];
        }

        public Type GetShaderType(int hash)
        {
            return shaderTypeDictHash[hash];
        }

        public ShaderBase CreateShader(string name, BasicShaderData data = null)
        {
            if (data == null) data = Renderer.ShaderData;
            return Activator.CreateInstance(GetShaderType(name), data) as ShaderBase;
        }

        public ShaderBase CreateShader(int hash, BasicShaderData data = null)
        {
            if (data == null) data = Renderer.ShaderData;
            return Activator.CreateInstance(GetShaderType(hash), data) as ShaderBase;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (shaderTypeDictName!=null)
            {
                shaderTypeDictName.Clear();
                shaderTypeDictName = null;
            }
            if (shaderTypeDictHash != null)
            {
                shaderTypeDictHash.Clear();
                shaderTypeDictHash = null;
            }
        }
    }
}
