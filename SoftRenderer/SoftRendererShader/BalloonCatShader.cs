// jave.lin 2019.07.21
using RendererCommon.SoftRenderer.Common.Attributes;
using RendererCommon.SoftRenderer.Common.Shader;
using System;
using System.ComponentModel;
using color = SoftRenderer.Common.Mathes.ColorNormalized;
using mat4 = SoftRenderer.Common.Mathes.Matrix4x4;
using vec2 = SoftRenderer.Common.Mathes.Vector2;
using vec3 = SoftRenderer.Common.Mathes.Vector3;
using vec4 = SoftRenderer.Common.Mathes.Vector4;

namespace SoftRendererShader
{
    [VS]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class BallooncatVertexShader : ShaderBase
    {
        [Name] public static readonly string Name = "BallooncatVertexShader";
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
        [In] [Out] [Normal] public vec3 ioNormal;
        [In] [Out] [Tangent] public vec3 ioTangent;

        [Out] [Tangent(1)] public vec3 outBitangent;

        [Out] [SV_Position] public vec4 outPos;
        [Out] [Position] public vec4 outWorldPos;

        public BallooncatVertexShader(BasicShaderData data) : base(data)
        {
        }

        [Main]
        public override void Main()
        {
            ioColor = color.yellow;
            inPos.xyz += ioNormal * outlineOffset;
            outPos = MVP * inPos;
            outWorldPos = M * inPos;
            ioNormal = M_IT * ioNormal;
            ioTangent = M_IT * ioTangent;
            outBitangent = ioNormal.Cross(ioTangent);
        }
    }

    [FS]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class BallooncatFragmentShader : FSBase
    {
        [Name] public static readonly string Name = "BallooncatFragmentShader";
        [NameHash] public static readonly int NameHash = NameUtil.HashID(Name);

        [Uniform] public Texture2D mainTex;
        [Uniform] public float specularPow = 1;

        private Sampler2D sampler = default(Sampler2D);

        [In] [SV_Position] public vec4 inPos;
        [In] [Position] public vec4 inWorldPos;
        [In] [Texcoord] public vec2 inUV;
        [In] [Color] public color inColor;
        [In] [Normal] public vec3 inNormal;
        [In] [Tangent] public vec3 inTangent;
        [In] [Tangent(1)] public vec3 inBitangent;

        [Out] [SV_Target] public color outColor;

        public BallooncatFragmentShader(BasicShaderData data) : base(data)
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
            outColor = tex2D(sampler, mainTex, inUV); return;
            //outColor.rgb = inNormal;return;
            //var v = inUV.y * 100;
            //var times = (int)(v / 10);
            //if (times % 2 == 0) outColor = color.red;
            //else outColor = color.green;
            //return;
            //outColor = f.depth;return;
            //outColor = inColor; return;
            var shaderData = Data as ShaderData;

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
            var diffuse = (tex2D(sampler, mainTex, inUV)) * 2 * (LdotN * 0.5f + 0.5f) * inColor;
            diffuse *= inNormal * 2;
            // specular
            var viewDir = (shaderData.CameraPos.xyz - inWorldPos.xyz);
            viewDir.Normalize();
            var specular = color.black;

            //if (LdotN > 0)
            {
                // specular 1 - blinn-phong
                // 高光也可以使用：光源角与视角的半角来算
                //var halfAngleDir = (lightDir + viewDir);
                //halfAngleDir.Normalize();
                //var HdotN = max(0, dot(halfAngleDir, inNormal));
                //HdotN = pow(HdotN, specularPow);
                //specular.rgb = (shaderData.LightColor[0] * HdotN).rgb * shaderData.LightColor[0].a;
                // specular 2 - phong
                var reflectDir = reflect(-lightDir, inNormal);
                var RnotV = max(0, dot(reflectDir, viewDir));
                RnotV = pow(RnotV, specularPow) * (LdotN * 0.5f + 0.5f);
                specular.rgb = (shaderData.LightColor[0] * RnotV).rgb * shaderData.LightColor[0].a;
            }

            // ambient
            var ambient = shaderData.Ambient;
            ambient.rgb *= ambient.a;

            outColor = diffuse + specular + ambient;
        }
    }
}
