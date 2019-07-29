// jave.lin 2019.07.21
using RendererCommon.SoftRenderer.Common.Attributes;
using RendererCommon.SoftRenderer.Common.Shader;
using SoftRenderer.Common.Mathes;
using System.ComponentModel;

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

        /* ==========In======== */

        [In] [Position] public Vector4 inPos;
        [In] [Texcoord] public Vector2 inUV;
        [In] [Color] public ColorNormalized inColor;
        //[In]
        //[Normal]
        //public Vector3 inNormal;

        /* ==========Out======== */

        [Out] [SV_Position] public Vector4 outPos;
        [Out] [Texcoord] public Vector2 outUV;
        [Out] [Color] public ColorNormalized outColor;
        //[Out]
        //[Normal]
        //public Vector3 outNormal;

        public VertexShader(BasicShaderData data) : base(data)
        {
        }

        [Main]
        public override void Main()
        {
            outPos = MVP * inPos;
            outUV = inUV;
            outColor = inColor;
            //outNormal = inNormal;
        }
    }

    [FS]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class FragmentShader : ShaderBase
    {
        [Name] public static readonly string Name = "MyTestFSShader";
        [NameHash] public static readonly int NameHash = NameUtil.HashID(Name);

        [Uniform] public Texture2D mainTex;
        public Sampler2D sampler;

        //[In]
        //[Position]
        //public Vector4 inPos;
        [In] [Texcoord] public Vector2 inUV;
        [In] [Color] public ColorNormalized inColor;
        //[In]
        //[Normal]
        //public Vector3 inNormal;

        [Out] [SV_Target] public ColorNormalized outColor;

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
            //var shaderData = Data as ShaderData;
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
            // 8
            var c = new ColorNormalized(inUV.x, inUV.y, 1, 1);
            outColor = tex2D(sampler, mainTex, inUV) + c; // * c;
            //outColor = new ColorNormalized(inUV.x, inUV.y, 1, 1);
        }
    }
}
