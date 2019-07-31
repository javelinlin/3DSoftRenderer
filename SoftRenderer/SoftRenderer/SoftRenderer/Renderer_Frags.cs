// jave.lin 2019.07.18
using RendererCommon.SoftRenderer.Common.Shader;
using SoftRenderer.Common.Mathes;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace SoftRenderer.SoftRenderer
{
    [Description("旧版-片段数据")]
    public class FragData : IDisposable
    {
        public Vector3 p;
        public bool discard;
        public float depth;
        public List<Vector4> datas = new List<Vector4>(); // 需要插值的数据

        public FragData()
        {

        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            if (datas != null)
            {
                datas.Clear();
                datas = null;
            }
        }

        public override string ToString()
        {
            return p.ToString();
        }
    }

    [Description("新版-片段数据")]
    public class FragInfo : IDisposable
    {
        public static FragInfo GetFragInfo(OutInfo[] upperStageOutInfos = null)
        {
            return new FragInfo(upperStageOutInfos);
        }

        public float depth;
        public bool discard;
        //	float reciprocalW = 1.0f / vertex.pos.w;
        public float reciprocalW;

        public ColorNormalized normalLineColor; // 调试用

        public Vector4 p;

        public int PosIdx { get; private set; }
        public OutInfo[] UpperStageOutInfos { get; private set; }

        public FragInfo(OutInfo[] upperStageOutInfos = null)
        {
            if (upperStageOutInfos != null)
            {
                Set(upperStageOutInfos);

                reciprocalW = 1 / p.w;
            }
        }

        public void Set(OutInfo[] upperStageOutInfos)
        {
            UpperStageOutInfos = upperStageOutInfos;
            var len = upperStageOutInfos.Length;
            for (int i = 0; i < len; i++)
            {
                if (upperStageOutInfos[i].layout == OutLayout.SV_Position)
                {
                    PosIdx = i;
                    p = upperStageOutInfos[i].Get<Vector4>();
                    break;
                }
            }
            //for (int i = 0; i < len; i++)
            //{
            //    if (upperStageOutInfos[i].layout == OutLayout.Texcoord)
            //    {
            //        var uv = (Vector2)upperStageOutInfos[i].value;
            //        uv /= -p.w;
            //        upperStageOutInfos[i].value = uv;
            //        break;
            //    }
            //}
            if (PosIdx == -1)
                throw new Exception("error");
        }

        public void WriteBackInfos()
        {
            UpperStageOutInfos[PosIdx].value = p;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            UpperStageOutInfos = null;
        }
    }
}
