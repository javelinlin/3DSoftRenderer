// jave.lin 2019.07.18
using SoftRenderer.Common.Mathes;
using System;
using System.ComponentModel;

namespace SoftRenderer.SoftRenderer
{
    [Description("逐片段")]
    public class Per_Frag : IDisposable
    {
        public DepthBuff DepthBuff { get; private set; }
        public Renderer Renderer { get; private set; }
        // Scissor
        // Alpha
        // Stencil
        // Depth
        // Blend OK
        // Logic
        public Per_Frag(Renderer renderer)
        {
            this.Renderer = renderer;
            this.DepthBuff = new DepthBuff(renderer.BackBufferWidth, renderer.BackBufferHeight);
        }

        // Alpha测试
        public bool AlphaTest(ComparisonFunc comp, float refV, float srcAlpha)
        {
            if (comp == ComparisonFunc.Always) return true;
            else if (comp == ComparisonFunc.Never) return false;

            switch (comp)
            {
                case ComparisonFunc.LEqual: return srcAlpha <= refV;
                case ComparisonFunc.GEqual: return srcAlpha >= refV;
                case ComparisonFunc.Equal: return srcAlpha == refV;
                case ComparisonFunc.NotEqual: return srcAlpha != refV;
                case ComparisonFunc.Less: return srcAlpha < refV;
                case ComparisonFunc.Greater: return srcAlpha > refV;
                default: throw new Exception("Not implements");
            }
        }

        // 混合
        public ColorNormalized Blend(
            ColorNormalized src, ColorNormalized dst,
            BlendFactor srcColorFactor, BlendFactor dstColorFactor,
            BlendFactor srcAlphaFactor, BlendFactor dstAlphaFactor,
            BlendOp blendColorOp, BlendOp blendAlphaOp
            )
        {
            var sr = src.r;
            var sg = src.g;
            var sb = src.b;
            var sa = src.a;

            var dr = dst.r;
            var dg = dst.g;
            var db = dst.b;
            var da = dst.a;

            var oneMSA = 1 - sa;
            var oneMDA = 1 - da;

            switch (srcColorFactor)
            {
                case BlendFactor.One: /* noops */ break;
                case BlendFactor.Zero: sr = 0; sg = 0; sb = 0; break;
                case BlendFactor.SrcAlpha: sr *= sa; sg *= sa; sb *= sa; break;
                case BlendFactor.OneMinusSrcAlpha: sr *= oneMSA; sg *= oneMSA; sb *= oneMSA; break;
                case BlendFactor.DstAlpha: sr *= da; sg *= da; sb *= da; break;
                case BlendFactor.OneMinusDstAlpha: sr *= oneMDA; sg *= oneMDA; sb *= oneMDA; break;
                default: throw new NotImplementedException($"Not implements");
            }
            switch (dstColorFactor)
            {
                case BlendFactor.One: /* noops */ break;
                case BlendFactor.Zero: dr = 0; dg = 0; db = 0; break;
                case BlendFactor.SrcAlpha: dr *= sa; dg *= sa; db *= sa; break;
                case BlendFactor.OneMinusSrcAlpha: dr *= oneMSA; dg *= oneMSA; db *= oneMSA; break;
                case BlendFactor.DstAlpha: dr *= da; dg *= da; db *= da; break;
                case BlendFactor.OneMinusDstAlpha: dr *= oneMDA; dg *= oneMDA; db *= oneMDA; break;
                default: throw new NotImplementedException($"Not implements");
            }
            switch (srcAlphaFactor)
            {
                case BlendFactor.One: /* noops */ break;
                case BlendFactor.Zero: sa = 0; break;
                case BlendFactor.SrcAlpha: sa *= sa; break;
                case BlendFactor.OneMinusSrcAlpha: sa *= oneMSA; break;
                case BlendFactor.DstAlpha: sa *= da; break;
                case BlendFactor.OneMinusDstAlpha: sa *= oneMDA; break;
                default: throw new NotImplementedException($"Not implements");
            }
            switch (dstAlphaFactor)
            {
                case BlendFactor.One: /* noops */ break;
                case BlendFactor.Zero: da = 0; break;
                case BlendFactor.SrcAlpha: da *= sa; break;
                case BlendFactor.OneMinusSrcAlpha: da *= oneMSA; break;
                case BlendFactor.DstAlpha: da *= da; break;
                case BlendFactor.OneMinusDstAlpha: da *= oneMDA; break;
                default: throw new NotImplementedException($"Not implements");
            }
            switch (blendColorOp)
            {
                case BlendOp.Add: sr += dr; sg += dg; sb += db; break;
                case BlendOp.Sub: sr -= dr; sg -= dg; sb -= db; break;
                case BlendOp.Multiply: sr *= dr; sg *= dg; sb *= db; break;
                case BlendOp.Divide: sr /= dr; sg /= dg; sb /= db; break; // 除法的性能好渣，尽量不用
                default: throw new NotImplementedException($"Not implements");
            }
            switch (blendAlphaOp)
            {
                case BlendOp.Add: sa += da; break;
                case BlendOp.Sub: sa -= da; break;
                case BlendOp.Multiply: sa *= da; break;
                case BlendOp.Divide: sa /= da; break; // 除法的性能好渣，尽量不用
                default: throw new NotImplementedException($"Not implements");
            }
            return new ColorNormalized(sr, sg, sb, sa);
        }

        public void Dispose()
        {
            Renderer = null;
            if (DepthBuff != null)
            {
                DepthBuff.Dispose();
                DepthBuff = null;
            }
        }
    }
}
