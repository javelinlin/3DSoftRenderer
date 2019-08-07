// jave.lin 2019.07.18
using System;
using System.ComponentModel;

namespace RendererCore.Renderer
{
    [Flags]
    [Description("Renderer.Clear时的标记")]
    public enum ClearFlag
    {
        None = 0,
        ColorBuffer = 1,        // 颜色缓存
        DepthBuffer = 2,        // 深度缓存
        StencilBuffer=3,        // 模板缓存
    }
    [Description("顶点数据类型")]
    public enum VertexDataType
    {
        Position,               // 坐标
        Color,                  // 颜色
        UV,                     // 纹理坐标
        Normal,                 // 法线
        Tangent,                // 切线
    }
}
