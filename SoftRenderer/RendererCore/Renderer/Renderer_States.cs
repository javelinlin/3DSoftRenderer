// jave.lin 2019.07.18
using RendererCoreCommon.Renderer.Common.Mathes;
using RendererCoreCommon.Renderer.Common.Shader;
using System.ComponentModel;
using System.Drawing;

namespace RendererCore.Renderer
{
    [Description("渲染状态，包含了一些测试用的数据")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class RendererState
    {
        public Renderer Renderer { get; private set; }

        // default state
        public DrawState DrawState { get; set; } = new DrawState();

        [Category("ClearInfo")]
        [Description("Clear Color buff的颜色值")]
        public Color ClearColor {
            get => Renderer.FrameBuffer.ClearedColor;
            set => Renderer.FrameBuffer.ClearedColor = value; }

        [Category("ClearInfo")]
        [Description("Clear Depth buff的值")]
        public float ClearDepthBuffValue {
            get => Renderer.FrameBuffer.ClearedDepth;
            set => Renderer.FrameBuffer.ClearedDepth = value;
        }

        [Category("Camera")]
        public float CameraNear { get; set; }
        [Category("Camera")]
        public float CameraFar { get; set; }
        [Category("Camera")]
        public Rectangle CameraViewport { get; set; }
        [Category("Camera")]
        public bool IsOrtho { get; set; }

        [Category("ShadingMode")]
        public ShadingMode ShadingMode { get; set; } = ShadingMode.Shaded;
        [Category("ShadingMode")]
        public Vector4 WireframeColor { get; set; } = Vector4.white;

        [Category("Debug")]
        [Description("调试用：显示TBN切线、副切线、法线")]
        public bool DebugShowTBN { get; set; } = false;
        [Category("Debug")]
        [Description("调试用：显示TBN切线、副切线、法线的长度")]
        public float DebugTBNlLen { get; set; } = 30;
        [Category("Debug")]
        [Description("调试用：方向灯光")]
        public Vector3 DebugDirectionalLight = new Vector3(0, 0.5f, 1);
        [Category("Debug")]
        [Description("调试用：方向灯光强度")]
        public float DebugLItensity = 1;
        [Category("Debug")]
        [Description("调试用：半兰伯特光照")]
        public bool DebugHalfLambertLighting = true;
        [Category("Debug")]
        [Description("调试用：是否启用高光")]
        public bool DebugSpecular = true;
        [Category("Debug")]
        [Description("调试用：高光颜色")]
        public Vector4 DebugSpecularColor = Vector4.Get(1, 1, 1, 1);
        [Category("Debug")]
        [Description("调试用：高光强度[0~100]的值，默认80")]
        public float DebugSpecularItensity = 80f;

        public float CamX;
        public float CamY;
        public float CamZ;

        public RendererState(Renderer renderer)
        {
            Renderer = renderer;
        }
    }

    [Description("全局的渲染状态")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class GlobalRenderSstate
    {
        [Description("抗锯齿开光")]
        public AA AA { get; set; }
        [Description("抗锯齿类型")]
        public AAType AAType { get; set; }

        public int aa_resample_count { get; set; } = 1;
        public float edge_thresold { get; set; } = 3e-5f;
        public bool show_edge { get; set; } = false;

        public bool fullscreen_blur { get; set; } = false;
        public int fullscreen_blur_resample_count { get; set; } = 3;
    }
}
