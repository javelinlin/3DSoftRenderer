// jave.lin 2019.07.18
using SoftRenderer.Common.Mathes;
using System.ComponentModel;
using System.Drawing;

namespace SoftRenderer.SoftRenderer
{
    [Description("渲染状态")]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class RenderState
    {
        public Renderer Renderer { get; private set; }

        [Category("Blend")] public Blend Blend { get; set; } = Blend.Off;
        [Category("Blend")] public BlendFactor BlendSrcColorFactor { get; set; } = BlendFactor.One;
        [Category("Blend")] public BlendFactor BlendDstColorFactor { get; set; } = BlendFactor.Zero;
        [Category("Blend")] public BlendFactor BlendSrcAlphaFactor { get; set; } = BlendFactor.One;
        [Category("Blend")] public BlendFactor BlendDstAlphaFactor { get; set; } = BlendFactor.Zero;
        [Category("Blend")] public BlendOp BlendColorOp { get; set; } = BlendOp.Add;
        [Category("Blend")] public BlendOp BlendAlphaOp { get; set; } = BlendOp.Add;

        [Category("ClearInfo")]
        [Description("Clear Color buff的颜色值")]
        public Color ClearColor {
            get => Renderer.backBuffer.ClearedColor;
            set => Renderer.backBuffer.ClearedColor = value; }

        [Category("ClearInfo")]
        [Description("Clear Depth buff的值")]
        public float ClearDepthBuffValue {
            get => Renderer.Per_Frag.DepthBuff.ClearedBuffValue;
            set => Renderer.Per_Frag.DepthBuff.ClearedBuffValue = value;
        }

        private Rectangle scissorRect;
        [Category("Scissor")]
        public Rectangle ScissorRect
        {
            get { return scissorRect; }
            set
            {
                var sl = value.Left;
                var sr = value.Right;
                var st = value.Top;
                var sb = value.Bottom;

                if (sl < 0) sl = 0;
                if (sr > Renderer.BackBufferWidth - 1)
                    sr = Renderer.BackBufferWidth - 1;
                if (st < 0) st = 0;
                if (sb > Renderer.BackBufferHeight - 1)
                    sb = Renderer.BackBufferHeight - 1;

                this.scissorRect = Rectangle.FromLTRB(sl, st, sr, sb);
            }
        }
        [Category("Scissor")]
        public Scissor Scissor { get; set; }
        //[Category("AlphaTest")]
        //public AlphaTest AlphaTest { get; set; }
        //[Category("AlphaTest")]
        //[Description("Alpha测试的比较关系")]
        //public ComparisonFunc AlphaTestComp { get; set; } = ComparisonFunc.LEqual;
        //[Category("AlphaTest")]
        //[Description("Alpha测试的参考值")]
        //public float AlphaTestRef { get; set; } = 1f;
        [Category("Facing-Culling")]
        public FrontFace FrontFace { get; set; }
        [Category("Facing-Culling")]
        public FaceCull Cull { get; set; }
        [Category("ShadingMode")]
        public ShadingMode ShadingMode { get; set; }
        [Category("ShadingMode")]
        public ColorNormalized WireframeColor { get; set; } = ColorNormalized.white;
        [Category("Depth")]
        public DepthWrite DepthWrite { get; set; }
        [Category("Depth")]
        public ComparisonFunc DepthTest { get; set; }
        [Category("Depth")]
        public DepthOffset DepthOffset { get; set; }
        [Category("PolygonMode")]
        public PolygonMode PolygonMode { get; set; }
        [Category("Depth")]
        [Description("Depth的掠射角偏移系数")]
        public float DepthOffsetFactor { get; set; } = 0;
        [Category("Depth的最小深度刻度单位偏移系数")]
        public float DepthOffsetUnit { get; set; } = 0;
        [Category("Camera")]
        public float CameraNear { get; set; }
        [Category("Camera")]
        public float CameraFar { get; set; }
        [Category("Camera")]
        public Rectangle CameraViewport { get; set; }
        [Category("Camera")]
        public bool IsOrtho { get; set; }

        [Category("Debug")]
        [Description("调试用：显示TBN切线、副切线、法线")]
        public bool DebugShowTBN { get; set; } = true;
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
        public ColorNormalized DebugSpecularColor = new ColorNormalized(1, 1, 1, 1);
        [Category("Debug")]
        [Description("调试用：高光强度[0~100]的值，默认80")]
        public float DebugSpecularItensity = 80f;

        public float CamX;
        public float CamY;
        public float CamZ;

        public RenderState(Renderer renderer)
        {
            Renderer = renderer;
            this.ScissorRect = new Rectangle(0, 0, renderer.BackBufferWidth, renderer.BackBufferHeight);
        }
    }
}
