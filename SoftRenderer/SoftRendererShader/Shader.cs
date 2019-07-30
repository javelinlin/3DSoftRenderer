// jave.lin 2019.07.21
using RendererCommon.SoftRenderer.Common.Attributes;
using RendererCommon.SoftRenderer.Common.Shader;
using SoftRenderer.Common.Mathes;
using System.ComponentModel;

using Color = SoftRenderer.Common.Mathes.ColorNormalized;

namespace SoftRendererShader
{
    [VS]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class VertexShader : ShaderBase
    {
        [Name] public static readonly string Name = "MyTestVSShader";
        [NameHash] public static readonly int NameHash = NameUtil.HashID(Name);

        /* ==========Uniform======== */
        [Uniform] public Matrix4x4 MVP;
        [Uniform] public Matrix4x4 M;
        [Uniform] public Matrix4x4 M_IT;

        /* ==========In======== */

        [In] [Position] public Vector4 inPos;
        [In] [Texcoord] public Vector2 inUV;
        [In] [Color] public Color inColor;
        [In] [Normal] public Vector3 inNormal;
        //[In] [Tangent] public Vector3 inTangent;

        /* ==========Out======== */

        [Out] [SV_Position] public Vector4 outPos;
        [Out] [Position] public Vector4 outWorldPos;
        [Out] [Texcoord] public Vector2 outUV;
        [Out] [Color] public Color outColor;
        [Out] [Normal] public Vector3 outNormal;
        //[Out] [Tangent] public Vector3 outTangent;

        public VertexShader(BasicShaderData data) : base(data)
        {
        }

        [Main]
        public override void Main()
        {
            outPos = MVP * inPos;
            outWorldPos = M * inPos;
            outUV = inUV;
            outColor = inColor;
            outNormal = M_IT * inNormal;
            //outTangent = M_IT * inTangent;
        }
    }

    [FS]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class FragmentShader : FSBase
    {
        [Name] public static readonly string Name = "MyTestFSShader";
        [NameHash] public static readonly int NameHash = NameUtil.HashID(Name);

        [Uniform] public Texture2D mainTex;
        public Sampler2D sampler;

        [In] [SV_Position] public Vector4 inPos;
        [In] [Position] public Vector4 inWorldPos;
        [In] [Texcoord] public Vector2 inUV;
        [In] [Color] public Color inColor;
        [In] [Normal] public Vector3 inNormal;
        //[In] [Tangent] public Vector3 inTangent;

        [Out] [SV_Target] public Color outColor;

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
            //if (inUV.x >= 0 && inUV.x <= 0.25f) outColor = ColorNormalized.red;
            //else if (inUV.x > 0.25f && inUV.x <= 0.5f) outColor = ColorNormalized.green;
            //else if (inUV.x > 0.5f && inUV.x <= 0.75f) outColor = ColorNormalized.blue;
            //else outColor = ColorNormalized.yellow;
            // 4
            //var v = inUV.y * 100;
            //var times = (int)(v / 5);
            //if (times % 2 == 0) outColor = ColorNormalized.red;
            //else outColor = ColorNormalized.green;
            // 5
            //outColor = new ColorNormalized(inUV.x, inUV.y, 0, 1);
            // 6
            //outColor = sampler.Sample(mainTex, inUV);
            // 7
            //outColor = tex2D(sampler, mainTex, inUV);
            // 8 alpha test in here
            //var c = new ColorNormalized(inUV.x, inUV.y, 0, 1);
            //outColor = tex2D(sampler, mainTex, inUV) + c; // * c;
            //var b = outColor.r + outColor.g + outColor.b;
            //b *= 0.3f;
            //if (b < 0.9f) discard = true;

            // diffuse
            var lightDir = shaderData.LightPos[0].xyz;
            var LdotN = dot(lightDir, inNormal);// * 0.5f + 0.5f;
            var diffuse = (1 - tex2D(sampler, mainTex, inUV)) * (LdotN * 0.5f + 0.5f) * inColor;
            diffuse *= 2;
            // specular
            var viewDir = shaderData.CameraPos.xyz - inWorldPos.xyz;
            var specular = Color.black;
            // specular 1
            // 高光也可以使用：光源角与视角的半角来算
            if (LdotN > 0)
            {
                var halfAngleDir = (lightDir + viewDir).normalized;
                var HdotN = max(0, dot(halfAngleDir, inNormal));
                HdotN = pow(HdotN, 80f);
                specular.rgb = (shaderData.LightColor[0] * HdotN).rgb * shaderData.LightColor[0].a;
            }
            // specular 2
            //var reflectDir = reflect(-lightDir.xyz, inNormal);
            //var RnotV = reflectDir.Dot(viewDir);
            //var specular = shaderData.LightColor[0] * RnotV;

            // ambient
            var ambient = shaderData.Ambient;
            ambient.rgb *= ambient.a;

            outColor = diffuse + specular + ambient;

            // test
            //outColor.rgb = inNormal * 0.5f + 0.5f;
        }
    }
}
