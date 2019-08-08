// jave.lin 2019.07.21
using RendererCoreCommon.Renderer.Common.Attributes;
using RendererCoreCommon.Renderer.Common.Shader;
using System;
using System.ComponentModel;
using color = RendererCoreCommon.Renderer.Common.Mathes.Vector4;
using mat4 = RendererCoreCommon.Renderer.Common.Mathes.Matrix4x4;
using vec2 = RendererCoreCommon.Renderer.Common.Mathes.Vector2;
using vec3 = RendererCoreCommon.Renderer.Common.Mathes.Vector3;
using vec4 = RendererCoreCommon.Renderer.Common.Mathes.Vector4;

namespace RendererShader
{
    [Shader]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class MaskStencilShader : ShaderBase
    {
        public static readonly string Name = "MaskStencilShader";
        public static readonly int NameHash = NameUtil.HashID(Name);

        /* ==========Uniform======== */
        // vert
        [Uniform] public mat4 MVP;
        [Uniform] public mat4 M;
        [Uniform] public mat4 M_IT;
        [Uniform] public float outlineOffset;

        // frag
        [Uniform] public Texture2D mainTex;
        [Uniform] public float specularPow = 1;
        private Sampler2D sampler = default(Sampler2D);

        /* ==========In or Out======== */

        private class _SubShader : SubShaderExt<MaskStencilShader>
        {
            public _SubShader(MaskStencilShader shader) : base(shader)
            {
                passList.Add(new _PassExt(this));
            }
        }

        private class _PassExt : PassExt<_SubShader>
        {
            /* ==========In or Out======== */
            public class _VertField : FuncField
            {
                [In] [Position] public vec4 inPos;
                [In] [Out] [Texcoord] public vec2 ioUV;
                [Out] [SV_Position] public vec4 outPos;

                public _VertField(Pass pass) : base(pass)
                {
                }
            }

            public class _FragField : FuncField
            {
                [In] [SV_Position] public vec4 inPos;
                [In] [Texcoord] public vec2 inUV;
                [Out] [SV_Target] public color outColor;
                [Out] [SV_Target(1)] public color outNormal;

                public _FragField(Pass pass) : base(pass)
                {
                }
            }

            private _VertField vertexField;
            private _FragField fragField;
            private MaskStencilShader shader;

            public override FuncField VertField
            {
                get => vertexField;
                protected set=> vertexField = value as _VertField;
            }

            public override FuncField FragField
            {
                get => fragField;
                protected set => fragField = value as _FragField;
            }

            public _PassExt(_SubShader subshader) : base(subshader)
            {
                shader = subshader.Shader_T;

                VertField = vertexField = new _VertField(this);
                FragField = new _FragField(this);

                State = new DrawState
                {
                    Stencil = Stencil.On,
                    StencilComp = ComparisonFunc.Always,
                    StencilRef = 1,
                    StencilPass = StencilOp.Replace,
                    Cull = FaceCull.Off,
                    ColorMask = 0,
                    DepthTest = ComparisonFunc.Always,
                    DepthWrite = DepthWrite.Off,
                };
            }

            public override void Vert()
            {
                vertexField.outPos = shader.MVP * vertexField.inPos;
            }

            public override void Frag()
            {
                var texC = tex2D(shader.sampler, shader.mainTex, fragField.inUV).rgb;
                var bright = dot(texC, texC);
                if (bright < 0.5f)
                {
                    discard = true;
                    return;
                }

                fragField.outColor = texC;
                fragField.outNormal = color.green;
            }

            public override void Dispose()
            {
                if (vertexField != null)
                {
                    vertexField.Dispose();
                    vertexField = null;
                }
                if (fragField != null)
                {
                    fragField.Dispose();
                    fragField = null;
                }
                base.Dispose();
            }
        }

        public MaskStencilShader(BasicShaderData data) : base(data)
        {
            SubShaderList.Add(new _SubShader(this));
        }
    }
}
