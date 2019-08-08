// jave.lin 2019.07.18
using RendererCoreCommon.Renderer.Common.Attributes;
using RendererCoreCommon.Renderer.Common.Shader;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reflection;

namespace RendererCore.Renderer
{
    [Description("Shader加载管理")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class ShaderLoadMgr : IDisposable
    {
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
            var baseType = typeof(ShaderBase);

            string name = "Name";
            string hash = "NameHash";

            foreach (var type in types)
            {
                var shader_at = type.GetCustomAttribute<ShaderAttribute>();
                if (shader_at == null) continue; // 不是shader的都过滤掉

                if (!type.IsSubclassOf(baseType))
                    throw new Exception($"shader:{type.Name} invalidated");

                var f = type.GetField(name, BindingFlags.GetField | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static);
                if (f == null || !f.IsInitOnly)
                    throw new Exception($"Shader Class Name:{type.Name}, not defines the 'public static readonly string {name};' field");
                var shaderName = (string)f.GetValue(null);
                f = type.GetField(hash, BindingFlags.GetField | BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Static);
                if (f == null || !f.IsInitOnly)
                    throw new Exception($"Shader Class Name:{type.Name}, not defines the 'public static readonly string {hash};' field");
                var shaderHash = (int)f.GetValue(null);

                if (shaderTypeDictName.ContainsKey(shaderName))
                    throw new Exception($"shaderName existing, shader:{shaderName}, type:{type.Name}, in buff shader.type:{shaderTypeDictName[shaderName].Name}");
                if (shaderTypeDictHash.ContainsKey(shaderHash))
                    throw new Exception($"shaderName existing, shaderHash:{shaderHash}, type:{type.Name}, in buff shader.type:{shaderTypeDictHash[shaderHash].Name}");
                shaderTypeDictName[shaderName] = type;
                shaderTypeDictHash[shaderHash] = type;

                Console.WriteLine($"Loading shader type:{type.Name} complete!");
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
