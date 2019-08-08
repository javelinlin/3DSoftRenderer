// jave.lin 2019.08.06
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
    public class SphereShader : ShaderBase
    {
        public static readonly string Name = "SphereShader";
        public static readonly int NameHash = NameUtil.HashID(Name);

        /* ==========Uniform======== */
        // vert
        [Uniform] public mat4 MVP;
        [Uniform] public mat4 M;
        [Uniform] public mat4 P;
        [Uniform] public mat4 M_IT;
        [Uniform] public mat4 MV_IT;
        [Uniform] public float outlineOffset;

        // frag
        [Uniform] public Texture2D mainTex;
        [Uniform] public float specularPow = 1;
        public Sampler2D sampler = default(Sampler2D);

        // 查看该shader时，先将下面的subshader, pass[n]先的IDE里代码折叠起来，会清晰很多
        private class _SubShader : SubShaderExt<SphereShader>
        {
            public _SubShader(SphereShader shader) : base(shader)
            {
                passList.Add(new _PassExt(this));
                passList.Add(new _PassExt1(this));
            }
        }

        // 正常绘制模式的pass
        private class _PassExt : PassExt<_SubShader>
        {
            /* ==========In or Out======== */
            public class _VertField : FuncField
            {
                [In] [Position] public vec4 inPos;

                [In] [Out] [Texcoord] public vec2 ioUV;
                [In] [Out] [Color] public color ioColor;
                [In] [Out] [Normal] public vec3 ioNormal;
                [In] [Out] [Tangent] public vec3 ioTangent;

                [Out] [Tangent(1)] public vec3 outBitangent;

                [Out] [SV_Position] public vec4 outPos;
                [Out] [Position] public vec4 outWorldPos;

                public _VertField(Pass pass) : base(pass)
                {
                }
            }

            public class _FragField : FuncField
            {
                [In] [SV_Position] public vec4 inPos;
                [In] [Position] public vec4 inWorldPos;
                [In] [Texcoord] public vec2 inUV;
                [In] [Color] public color inColor;
                [In] [Normal] public vec3 inNormal;
                [In] [Tangent] public vec3 inTangent;
                [In] [Tangent(1)] public vec3 inBitangent;

                [Out] [SV_Target] public color outColor;
                [Out] [SV_Target(1)] public color outNormal;

                public _FragField(Pass pass) : base(pass)
                {
                }
            }

            private _VertField vertexField;
            private _FragField fragField;
            private SphereShader shader;

            public override FuncField VertField
            {
                get => vertexField;
                protected set => vertexField = value as _VertField;
            }

            public override FuncField FragField
            {
                get => fragField;
                protected set => fragField = value as _FragField;
            }

            public _PassExt(_SubShader subshader) : base(subshader)
            {
                shader = subshader.Shader_T;

                VertField = new _VertField(this);
                FragField = new _FragField(this);

                State = new DrawState
                {
                    Stencil = Stencil.On,
                    StencilComp = ComparisonFunc.NotEqual,
                    StencilRef = 1,
                };
            }

            public override void Vert()
            {
                vertexField.ioColor = color.yellow;
                vertexField.inPos.xyz += vertexField.ioNormal * shader.outlineOffset;
                vertexField.outPos = shader.MVP * vertexField.inPos;
                vertexField.outWorldPos = shader.M * vertexField.inPos;
                vertexField.ioNormal = shader.M_IT * vertexField.ioNormal;
                vertexField.ioTangent = shader.M_IT * vertexField.ioTangent;
                vertexField.outBitangent = vertexField.ioNormal.Cross(vertexField.ioTangent);
            }

            public override void Frag()
            {
                var shaderData = shader.Data as ShaderData;

                // diffuse
                var lightPos = shaderData.LightPos[0];
                var lightType = lightPos.w;
                vec3 lightDir;
                if (lightType == 0) // 方向光
                    lightDir = lightPos.xyz;
                else if (lightType == 1) // 点光源
                    lightDir = (lightPos.xyz - fragField.inWorldPos.xyz).normalized;
                // intensity = max(0, 1 - distance / range);
                else
                    throw new Exception($"not implements lightType:{lightType}");
                var LdotN = dot(lightDir, fragField.inNormal);// * 0.5f + 0.5f;
                var diffuse = (tex2D(shader.sampler, shader.mainTex, fragField.inUV)) * 2 * (LdotN * 0.5f + 0.5f) * fragField.inColor;
                diffuse *= fragField.inNormal * 2;
                // specular
                var viewDir = (shaderData.CameraPos.xyz - fragField.inWorldPos.xyz);
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
                    var reflectDir = reflect(-lightDir, fragField.inNormal);
                    var RnotV = max(0, dot(reflectDir, viewDir));
                    RnotV = pow(RnotV, shader.specularPow) * (LdotN * 0.5f + 0.5f);
                    specular.rgb = (shaderData.LightColor[0] * RnotV).rgb * shaderData.LightColor[0].a;
                }

                // ambient
                var ambient = shaderData.Ambient;
                ambient.rgb *= ambient.a;

                fragField.outColor = diffuse + specular + ambient;
                fragField.outNormal = fragField.inNormal;
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

        // 描边的pass
        private class _PassExt1 : PassExt<_SubShader>
        {
            /* ==========In or Out======== */
            public class _VertField : FuncField
            {
                [In] [Position] public vec4 inPos;
                [In] [Out] [Normal] public vec3 ioNormal;
                [Out] [SV_Position] public vec4 outPos;

                public _VertField(Pass pass) : base(pass)
                {
                }
            }

            public class _FragField : FuncField
            {
                [Out] [SV_Target] public color outColor;
                [Out] [SV_Target(1)] public color outNormal;

                public _FragField(Pass pass) : base(pass)
                {
                }
            }

            private _VertField vertexField;
            private _FragField fragField;
            private SphereShader shader;

            public override FuncField VertField
            {
                get => vertexField;
                protected set => vertexField = value as _VertField;
            }

            public override FuncField FragField
            {
                get => fragField;
                protected set => fragField = value as _FragField;
            }

            public _PassExt1(_SubShader subshader) : base(subshader)
            {
                shader = subshader.Shader_T;

                VertField = new _VertField(this);
                FragField = new _FragField(this);

                State = new DrawState
                {
                    Cull = FaceCull.Front,
                    DepthWrite = DepthWrite.Off,
                    Stencil = Stencil.On,
                    StencilComp = ComparisonFunc.NotEqual,
                    StencilRef = 1,
                };
            }

            public override void Vert()
            {
                //https://blog.csdn.net/linjf520/article/details/95064552#t10
                vertexField.outPos = shader.MVP * vertexField.inPos;
                var n = shader.MV_IT * vertexField.ioNormal;
                n = shader.P * n;
                vertexField.ioNormal = shader.M_IT * vertexField.ioNormal;
                vertexField.outPos.xy += n.xy * 0.1f * vertexField.outPos.w;
            }

            public override void Frag()
            {
                fragField.outColor = color.yellow;
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

        public SphereShader(BasicShaderData data) : base(data)
        {
            SubShaderList.Add(new _SubShader(this));
        }
    }
}
