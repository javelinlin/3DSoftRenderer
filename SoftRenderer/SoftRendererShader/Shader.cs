// jave.lin 2019.07.21
using RendererCommon.SoftRenderer.Common.Attributes;
using RendererCommon.SoftRenderer.Common.Shader;
using SoftRenderer.Common.Mathes;
using System.ComponentModel;

using color = SoftRenderer.Common.Mathes.ColorNormalized;
using vec2 = SoftRenderer.Common.Mathes.Vector2;
using vec3 = SoftRenderer.Common.Mathes.Vector3;
using vec4 = SoftRenderer.Common.Mathes.Vector4;
using mat4 = SoftRenderer.Common.Mathes.Matrix4x4;
using System;

namespace SoftRendererShader
{
    [VS]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class VertexShader : ShaderBase
    {
        [Name] public static readonly string Name = "MyTestVSShader";
        [NameHash] public static readonly int NameHash = NameUtil.HashID(Name);

        /* ==========Uniform======== */
        [Uniform] public mat4 MVP;
        [Uniform] public mat4 M;
        [Uniform] public mat4 M_IT;
        [Uniform] public float outlineOffset;

        /* ==========In or Out======== */

        [In] [Position] public vec4 inPos;

        [In] [Out] [Texcoord] public vec2 ioUV;
        [In] [Out] [Color] public color ioColor;
        [In] [Out] [Normal] [Nointerpolation] public vec3 ioNormal;
        //[In] [Out] [Tangent] public Vector3 ioTangent;

        [Out] [SV_Position] public vec4 outPos;
        [Out] [Position] public vec4 outWorldPos;

        public VertexShader(BasicShaderData data) : base(data)
        {
        }

        [Main]
        public override void Main()
        {
            outPos = MVP * inPos;
            outPos.xyz += ioNormal * outlineOffset;
            outWorldPos = M * inPos;
            ioNormal = M_IT * ioNormal;
            //outTangent = M_IT * ioTangent;
        }
    }

    [FS]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class FragmentShader : FSBase
    {
        [Name] public static readonly string Name = "MyTestFSShader";
        [NameHash] public static readonly int NameHash = NameUtil.HashID(Name);

        [Uniform] public Texture2D mainTex;
        private Sampler2D sampler = default(Sampler2D);

        [In] [SV_Position] public vec4 inPos;
        [In] [Position] public vec4 inWorldPos;
        [In] [Texcoord] public vec2 inUV;
        [In] [Color] public color inColor;
        [In] [Normal] public vec3 inNormal;
        //[In] [Tangent] public Vector3 inTangent;

        [Out] [SV_Target] public color outColor;

        public FragmentShader(BasicShaderData data) : base(data)
        {
            //sampler.wrapMode = SampleWrapMode.Clamp;
            //sampler.wrapMode = SampleWrapMode.Repeat;
            //sampler.wrapMode = SampleWrapMode.Mirror;
            //sampler.wrapMode = SampleWrapMode.MirrorOnce;
            //sampler.wrapMode = SampleWrapMode.RepeatX | SampleWrapMode.MirrorOnceY;
            //sampler.wrapMode = SampleWrapMode.RepeatY | SampleWrapMode.MirrorOnceX;
            //sampler.wrapMode = SampleWrapMode.ClampX | SampleWrapMode.MirrorY;
            //sampler.wrapMode = SampleWrapMode.ClampY | SampleWrapMode.MirrorX;
            //sampler.wrapMode = SampleWrapMode.ClampX | SampleWrapMode.RepeatY;
            //sampler.wrapMode = SampleWrapMode.ClampY | SampleWrapMode.RepeatX;
            //sampler.wrapMode = SampleWrapMode.RepeatX | SampleWrapMode.MirrorY;
            //sampler.wrapMode = SampleWrapMode.RepeatY | SampleWrapMode.MirrorX;
        }

        [Main]
        public override void Main()
        {
            //outColor.rgb = inNormal;return;
            //1
            var shaderData = Data as ShaderData;
            //
            //Vector3 lightDir = shaderData.LightPos[0];
            //float LdotN = Vector3.Dot(lightDir, inNormal);
            //// tex2D(tex, uv)
            //
            //outColor = inColor * LdotN;

            //2
            //outColor = inColor;
            //3
            //if (inUV.x >= 0 && inUV.x <= 0.25f) outColor = color.red;
            //else if (inUV.x > 0.25f && inUV.x <= 0.5f) outColor = ColorNormalized.green;
            //else if (inUV.x > 0.5f && inUV.x <= 0.75f) outColor = ColorNormalized.blue;
            //else outColor = ColorNormalized.yellow;
            // 4
            //var v = inUV.y * 100;
            //var times = (int)(v / 5);
            //if (times % 2 == 0) outColor = color.red;
            //else outColor = color.green;
            //return;
            // 5
            //outColor = new color(inUV.x, inUV.y, 0, 1);
            // 6
            //outColor = sampler.Sample(mainTex, inUV);
            // 7
            //outColor = tex2D(sampler, mainTex, inUV);
            // 8 alpha test in here
            //var c = new color(inUV.x, inUV.y, 0, 1);
            //outColor = tex2D(sampler, mainTex, inUV) + c; // * c;
            //var b = outColor.r + outColor.g + outColor.b;
            //b *= 0.3f;
            //if (b < 0.9f) discard = true;

            // diffuse
            var lightPos = shaderData.LightPos[0];
            var lightType = lightPos.w;
            vec3 lightDir;
            if (lightType == 0) // 方向光
                lightDir = lightPos.xyz;
            else if (lightType == 1) // 点光源
                lightDir = (lightPos.xyz - inWorldPos.xyz).normalized;
                // intensity = max(0, 1 - distance / range);
            else
                throw new Exception($"not implements lightType:{lightType}");
            var LdotN = dot(lightDir, inNormal);// * 0.5f + 0.5f;
            var diffuse = (1 - tex2D(sampler, mainTex, inUV)) * 2 * (LdotN * 0.5f + 0.5f) * inColor;
            // specular
            var viewDir = (shaderData.CameraPos.xyz - inWorldPos.xyz);
            viewDir.Normalize();
            var specular = color.black;

            if (LdotN > 0)
            {
                // specular 1
                // 高光也可以使用：光源角与视角的半角来算
                //var halfAngleDir = (lightDir + viewDir);
                //halfAngleDir.Normalize();
                //var HdotN = max(0, dot(halfAngleDir, inNormal));
                //HdotN = pow(HdotN, 90f);
                //specular.rgb = (shaderData.LightColor[0] * HdotN).rgb * shaderData.LightColor[0].a;
                // specular 2
                var reflectDir = reflect(-lightDir, inNormal);
                var RnotV = max(0, dot(reflectDir, viewDir));
                specular.rgb = (shaderData.LightColor[0] * RnotV).rgb * shaderData.LightColor[0].a;
            }


            // ambient
            var ambient = shaderData.Ambient;
            ambient.rgb *= ambient.a;
            //ambient.rgb = 0; // test

            outColor = diffuse + specular + ambient;
            //outColor.rgb = LdotN;
            //outColor.rgb = inNormal + specular.rgb;
            //outColor.rgb = inNormal;

            // test
            //outColor.rgb = inNormal * 0.5f + 0.5f;
            //var reflectDir = reflect(-lightDir, inNormal);
            //var RnotV = max(0, dot(reflectDir, viewDir));
            //outColor.rgb = lightDir; return;
            //outColor.rgb = new Vector3(1, -0.5f, 1).normalized; return;

            //outColor.rgb = reflectDir;
        }
    }
}
