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
}
