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
    [Description("混合开关")]
    public enum Blend
    {
        Off,
        On,
    }
    [Description("RGB/A混合因子枚举")]
    public enum BlendFactor
    {
        One,
        Zero,
        SrcAlpha,
        OneMinusSrcAlpha,
        DstAlpha,
        OneMinusDstAlpha,
    }
    [Description("混合Op枚举")]
    public enum BlendOp
    {
        Add,
        Sub,
        Multiply,
        Divide,
    }
    [Description("裁剪")]
    public enum Scissor
    {
        Off,
        On,
    }
    //[Description("Alpha测试开关枚举")]
    //public enum AlphaTest
    //{
    //    Off,
    //    On
    //}
    [Description("决定正/反面的枚举")]
    public enum FrontFace
    {
        Clock,          // default: 顺时针，这儿我与Unity类似。
        CounterClock,   // 逆时针，这个是OpenGL默认的
    }
    [Description("面向剔除")]
    public enum FaceCull
    {
        Back,           // default: 剔除背面
        Front,          // 剔除背面
        Off,            // 关闭正/背面剔除
    }
    [Flags]
    [Description("着色模式")]   // 类似Unity的ShadingMode
    public enum ShadingMode
    {
        Shaded = 1,             // 着色填充
        Wireframe = 2,          // 线框绘制
        ShadedAndWireframe = Shaded | Wireframe, // 线框与着色
    }
    [Description("深度写入开关枚举")]
    public enum DepthWrite
    {
        On,
        Off
    }
    [Description("比较方式")]
    public enum ComparisonFunc
    {
        Less,           // 默认：小于,
        LEqual,         // 原来默认是这个：小于等于，因为用Less性能更高
        GEqual,         // 大于等于
        Equal,          // 等于
        NotEqual,       // 不等于
        Greater,        // 大于
        Always,         // 不比较，直接通过
        Never,          // 不比较，直接失败
    }
    [Description("深度偏移")]
    public enum DepthOffset
    {
        Off,
        On
    }
    [Description("着色器类型")]
    public enum ShaderType
    {
        VertexShader,           // 必要shader
        FragmentShader,         // 必要
        GeometryShader,         // 可选，未实现管线
        TessellationShader,     // 可选，为实现管线
        ComputeShader,          // 可选，未实现管线
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
    [Description("多边形模式")]
    public enum PolygonMode
    {
        //这里就不高line_strip, triangle_strip了
        Triangle,               // 三角形 - 目前实现的方式
        Line,                   // 线 - 未实现
        Point,                  // 点 - 未实现
    }
    [Description("AA：Anti-Aliasing，抗锯齿")]
    public enum AA
    {
        Off,        // 默认是关闭的（本想默认开的，但这个功能太卡，就改为默认Off吧）
        On
    }
    [Description("AA的抗锯齿类型")]
    public enum AAType
    {
        MSAA,       // 目前实现的
        //ETC,  以后再实现其他的
    }
}
