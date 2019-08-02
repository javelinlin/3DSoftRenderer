// jave.lin 2019.07.15
// 这个做模拟光线可以测试用，但会非常卡，因为可编程管线我们使用的是反射的机制来处理的
#define PROGRAMMABLE_PIPELINE

using RendererCommon.SoftRenderer.Common.Shader;
using SoftRenderer.Common.Mathes;
using SoftRenderer.Games;
using SoftRenderer.SoftRenderer;
using SoftRenderer.SoftRenderer.Primitives;
using SoftRenderer.SoftRenderer.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace SoftRenderer
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public partial class MainForm : Form
    {
        //API声明：获取当前焦点控件句柄      
        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.Winapi)]
        internal static extern IntPtr GetFocus();
        ///获取 当前拥有焦点的控件
        private Control GetFocusedControl()
        {
            Control focusedControl = null;
            // To get hold of the focused control:
            IntPtr focusedHandle = GetFocus();
            if (focusedHandle != IntPtr.Zero)
                //focusedControl = Control.FromHandle(focusedHandle);
                focusedControl = Control.FromChildHandle(focusedHandle);
            return focusedControl;
        }

        private Control lastFocusControl;

        private static readonly int outlineOffsetHash = "outlineOffset".GetHashCode();
        private static readonly int specularPowHash = "specularPow".GetHashCode();
        private const int buff_size = 200;
        private Renderer renderer;

        private long lastTick = -1; // 1 tick = 1/10000 ms
        private float elapsedTime;
        private int fpsCounter;
        private enum FpsShowType
        {
            Immediately,
            OneSecond,
        }
        private FpsShowType fpsShowType = FpsShowType.OneSecond;
        private Rectangle bmdRect = new Rectangle(0, 0, buff_size, buff_size);
        private Random ran = new Random();
        private bool paused = false;
        private List<GameObject> gameObjs = new List<GameObject>();
        public List<GameObject> GameObjs { get => gameObjs; }
        private Camera camera;
        private List<Vector3> wposList = new List<Vector3>();
        private List<Triangle> triangles = new List<Triangle>();
        private ShaderData shaderData;

        private bool autoRotate = true;

        // 法线描边的偏移值，这个需要好的模型才能测试出来
        // TODO 后面添加支持：加载*.obj模型
        public float normalOutlineOffset { get; set; } = 0; // 法线现在有问题，所以取负数，以后有精力再看，因为现在看了不少于50篇相关内容，没一个可以解决问题的
        public float specularPow { get; set; } = 1; // 高光亮度

        private GlobalMessageHandler globalMsg = new GlobalMessageHandler();

        public MainForm()
        {
            InitializeComponent();

            PictureBox.MouseDown += PictureBox_MouseDown;
            globalMsg.OnWheelEvent += GlobalMsg_OnWheelEvent;
            Application.AddMessageFilter(globalMsg);
            this.FormClosed += (s, e) => Application.RemoveMessageFilter(globalMsg);

            renderer = new Renderer(buff_size, buff_size);

            renderer.State.ClearColor = Color.Gray;
            renderer.State.ShadingMode = ShadingMode.Shaded;
            renderer.State.BlendSrcColorFactor = BlendFactor.SrcAlpha;
            renderer.State.BlendDstColorFactor = BlendFactor.OneMinusSrcAlpha;
            renderer.State.BlendSrcAlphaFactor = BlendFactor.One;
            renderer.State.BlendDstAlphaFactor = BlendFactor.One;
            // test
            //renderer.State.Cull = FaceCull.Off;

            this.PictureBox.Width = buff_size;
            this.PictureBox.Height = buff_size;

            PropertyGrid.SelectedObject = renderer;

            gameObjs = new List<GameObject>();

            var go = new GameObject("Cube");
            /**
               
            (-1,1)          (1,1)
             ----------------
             |              |
             |              |
             |----(0,0)-----|
             |              |
             |              |
             ----------------
            (-1,-1)         (1,-1)



            right-handside coordinates
                    y
                    /\  
                    |
                    |
                    |-----> x
                   /
                  /
                z

      [4/15/16]             [11/12/19]
            ------------------
            |\              | \
            | \(-1,1)[0/7/18]  \ (1,1)[3/8/17]
            |  \ ----------------
            |    |          |   |
            |- - |- - - - - -   |
   [6/13/22] \   | [9/14/21] \  |
              \  |            \ |
               \ |             \|
                 ----------------
            (-1,-1)[2/5/20]    (1,-1)[1/10/23]



             * */
            var size = 1f;
            var vertices = new Vector3[]
            {
                #region back
                new Vector3(-size, size,-size),         // 0
                new Vector3(size,-size,-size),          // 1
                new Vector3(-size,-size,-size),         // 2
                new Vector3(size, size,-size),          // 3
                #endregion
                #region left
                new Vector3(-size, size,size),           // 4
                new Vector3(-size,-size,-size),          // 5
                new Vector3(-size,-size,size),           // 6
                new Vector3(-size, size,-size),          // 7
                #endregion
                #region right
                new Vector3(size, size,-size),         // 8
                new Vector3(size,-size,size),          // 9
                new Vector3(size,-size,-size),         // 10
                new Vector3(size, size,size),          // 11
                #endregion
                #region front
                new Vector3(size, size,size),          // 12
                new Vector3(-size,-size,size),           // 13
                new Vector3(size,-size,size),          // 14
                new Vector3(-size, size,size),           // 15
                #endregion
                #region top
                new Vector3(-size, size,size),           // 16
                new Vector3(size, size,-size),         // 17
                new Vector3(-size, size,-size),          // 18
                new Vector3(size, size,size),          // 19
                #endregion
                #region bottom
                new Vector3(-size,-size,-size),          // 20
                new Vector3(size,-size,size),          // 21
                new Vector3(-size,-size,size),           // 22
                new Vector3(size,-size,-size),         // 23
                #endregion
            };
            var indices = new int[vertices.Length / 4 * 6];
            var idx = 0;
            //for (int i = 0; i < 36; i+=6)
            for (int i = 0; i < indices.Length; i+=6)
            {
                indices[i + 0] = 0 + 4 * idx;
                indices[i + 1] = 1 + 4 * idx;
                indices[i + 2] = 2 + 4 * idx;
                indices[i + 3] = 0 + 4 * idx;
                indices[i + 4] = 3 + 4 * idx;
                indices[i + 5] = 1 + 4 * idx;
                idx++;
            }
            var uvs = new Vector2[vertices.Length];
            //var uvScale = 3;
            //var offsetU = -1.5f;
            //var offsetV = -1.5f;
            var uvScale = 1;
            var offsetU = 0;
            var offsetV = 0;
            for (int i = 0; i < vertices.Length; i+=4)
            {
                uvs[i + 0] = new Vector2(0, 0) * uvScale + new Vector2(offsetU, offsetV);    // 0
                uvs[i + 1] = new Vector2(1, 1) * uvScale + new Vector2(offsetU, offsetV);    // 1
                uvs[i + 2] = new Vector2(0, 1) * uvScale + new Vector2(offsetU, offsetV);    // 2
                uvs[i + 3] = new Vector2(1, 0) * uvScale + new Vector2(offsetU, offsetV);    // 3
            }
            var colors = new ColorNormalized[vertices.Length];
            for (int i = 0; i < colors.Length; i+=4)
            {
                //colors[i + 0] = ColorNormalized.red;
                //colors[i + 1] = ColorNormalized.green;
                //colors[i + 2] = ColorNormalized.blue;
                //colors[i + 3] = ColorNormalized.yellow;
                colors[i + 0] = ColorNormalized.yellow;
                colors[i + 1] = ColorNormalized.yellow;
                colors[i + 2] = ColorNormalized.yellow;
                colors[i + 3] = ColorNormalized.yellow;
            }
            //colors[0] = ColorNormalized.red;
            //colors[1] = ColorNormalized.green;
            //colors[2] = ColorNormalized.blue;
            var mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = indices;
            mesh.uv = uvs;
            mesh.colors = colors;
            mesh.CaculateNormalAndTangent();
            go.Mesh = mesh;
            // 第一个游戏对象
            //gameObjs.Add(go);

            colors = new ColorNormalized[vertices.Length];
            for (int i = 0; i < colors.Length; i += 4)
            {
                //colors[i + 0] = ColorNormalized.lightBlue;
                //colors[i + 1] = ColorNormalized.pink;
                //colors[i + 2] = ColorNormalized.purple;
                //colors[i + 3] = ColorNormalized.green;
                colors[i + 0] = ColorNormalized.blue;
                colors[i + 1] = ColorNormalized.blue;
                colors[i + 2] = ColorNormalized.blue;
                colors[i + 3] = ColorNormalized.blue;
            }
            go = new GameObject("Cube1");
            go.LocalPosition = new Vector3(1, 0, -1);
            mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = indices;
            mesh.uv = uvs;
            mesh.colors = colors;
            mesh.CaculateNormalAndTangent();
            go.Mesh = mesh;
            // 第二个游戏对象
            //gameObjs.Add(go);

            go = new GameObject("Sphere");
            ModelReader.ReadOut("Models/Sphere_637003627289014299.m", out Mesh sphere);
            go.Mesh = sphere;
            go.LocalPosition = new Vector3(3, 0, -1);
            go.LocalScale = 3;
            // 第三个是球体
            //gameObjs.Add(go);

            go = new GameObject("Ballooncat");
            ModelReader.ReadOut("Models/BalloonStupidCat_637003750150312129.m", out Mesh ballooncat);
            go.Mesh = ballooncat;
            go.LocalPosition = new Vector3(4, 0, -1);
            go.LocalScale = 3;
            // 第四个是气球猫
            gameObjs.Add(go);

            camera = new Camera();
            camera.aspect = 1;
            camera.TranslateTo(new Vector3(0, 0, 8));
            // 暂时使用正交来测试
            // 透视有校正问题没处理好
            camera.isOrtho = false;
            if (camera.isOrtho)
            {
                camera.size = 5;
            }
            else
            {
                camera.TranslateTo(new Vector3(0, 0, 8));
            }

#if PROGRAMMABLE_PIPELINE
            renderer.ShaderData = shaderData = new ShaderData(1);

            renderer.ShaderMgr.Load("Shaders/SoftRendererShader.dll");

            var vs_shaderName = "MyTestVSShader";
            var fs_shaderName = "MyTestFSShader";

            var sphere_vs_shaderName = "SphereVertexShader";
            var sphere_fs_shaderName = "SphereFragmentShader";

            var ballooncat_vs_shaderName = "BallooncatVertexShader";
            var ballooncat_fs_shaderName = "BallooncatFragmentShader";

            var vsShader = renderer.ShaderMgr.CreateShader(vs_shaderName);
            var fsShader = renderer.ShaderMgr.CreateShader(fs_shaderName);
            var sphere_vsShader = renderer.ShaderMgr.CreateShader(sphere_vs_shaderName);
            var sphere_fsShader = renderer.ShaderMgr.CreateShader(sphere_fs_shaderName);
            var ballooncat_vsShader = renderer.ShaderMgr.CreateShader(ballooncat_vs_shaderName);
            var ballooncat_fsShader = renderer.ShaderMgr.CreateShader(ballooncat_fs_shaderName);
            List<ShaderBase> shaders = new List<ShaderBase>(
                new ShaderBase[] {
                    //vsShader,fsShader,
                    //vsShader,fsShader,
                    //sphere_vsShader,sphere_fsShader,
                    ballooncat_vsShader,ballooncat_fsShader,
                });

            //var tex_bmp = new Bitmap("Images/texture.png");
            var tex_bmp = new Bitmap("Images/GitHubIcon.PNG");
            var sp_tex_bmp = new Bitmap("Images/my_tex.png");
            var ballooncat_tex_bmp = new Bitmap("Images/balloonstupidcat_sg.png");
            //var tex_bmp = new Bitmap("Images/heightMap1.jpg");
            //var tex_bmp = new Bitmap("Images/tex.jpg");
            //var tex_bmp = new Bitmap("Images/icon.PNG");
            fsShader.ShaderProperties.SetUniform("mainTex", new Texture2D(tex_bmp));
            sphere_fsShader.ShaderProperties.SetUniform("mainTex", new Texture2D(sp_tex_bmp));
            ballooncat_fsShader.ShaderProperties.SetUniform("mainTex", new Texture2D(ballooncat_tex_bmp));

            for (int i = 0; i < gameObjs.Count; i++)
            {
                gameObjs[i].Material = new Material(shaders[i * 2 + 0], shaders[i * 2 + 1]);
            }
#endif
        }

        private void GlobalMsg_OnWheelEvent(bool fromDownToUp)
        {
            if (!PictureBox.Focused) return;
            if (fromDownToUp) // 鼠标滚轮控制镜头远近
            {
                camera.Translate += camera.Forward;
            }
            else
            {
                camera.Translate -= camera.Forward;
            }
        }

        private void UpdateCameraTR()
        {
            if (PictureBox.Focused)
            {
                if (this.globalMsg.GetKeyDown(Keys.ControlKey) && this.globalMsg.MS_LBTN)
                {
                    if (this.globalMsg.delta_ms_pos.IsEmpty == false)
                    {
                        this.camera.RotateY(this.globalMsg.delta_ms_pos.X);
                        this.camera.RotateX(-this.globalMsg.delta_ms_pos.Y);
                        this.globalMsg.delta_ms_pos = Point.Empty;
                    }
                }
                if (this.globalMsg.MS_LBTN) // 鼠标左右按下移动，控制相对相机坐标方向来水平、垂直的位移 - 这个功能会比较实用
                {
                    if (this.globalMsg.delta_ms_pos.IsEmpty == false)
                    {
                        const float scale = 0.1f;
                        camera.Translate -= this.globalMsg.delta_ms_pos.X * scale * camera.Right;
                        camera.Translate += this.globalMsg.delta_ms_pos.Y * scale * camera.Up;
                        this.globalMsg.delta_ms_pos = Point.Empty;
                    }
                }
                if (this.globalMsg.MS_MBTN) // 鼠标中键控制镜头的水平、垂直位移，相对世界坐标方向来水平、垂直的位移
                {
                    if (this.globalMsg.delta_ms_pos.IsEmpty == false)
                    {
                        const float scale = 0.1f;
                        camera.Translate -= this.globalMsg.delta_ms_pos.X * scale * Vector3.right;
                        camera.Translate -= this.globalMsg.delta_ms_pos.Y * scale * Vector3.up;
                        this.globalMsg.delta_ms_pos = Point.Empty;
                    }
                }
                if (this.globalMsg.MS_RBTN) // 按住鼠标右键不放的同时再用W,S,A,D控制镜头的移动
                {
                    if (this.globalMsg.delta_ms_pos.IsEmpty == false)
                    {
                        this.camera.RotateY(this.globalMsg.delta_ms_pos.X);
                        this.camera.RotateX(-this.globalMsg.delta_ms_pos.Y);
                        this.globalMsg.delta_ms_pos = Point.Empty;
                    }
                    const float scale = 0.5f;
                    if (this.globalMsg.GetKeyDown(Keys.W)) camera.Translate += scale * camera.Forward;
                    if (this.globalMsg.GetKeyDown(Keys.S)) camera.Translate -= scale * camera.Forward;
                    if (this.globalMsg.GetKeyDown(Keys.A)) camera.Translate -= scale * camera.Right;
                    if (this.globalMsg.GetKeyDown(Keys.D)) camera.Translate += scale * camera.Right;
                }
            }
        }

        private void Update(float deltaMs)
        {
            // camera update
            // 后面封装承Component
            //camera.TranslateTo(new Vector3(Tx, Ty, Tz));
            //camera.RotateTo(new Vector3(Rx, Ry, Rz));
            camera.Update(deltaMs);

            if (camera.viewport == Rectangle.Empty)
            {
                camera.viewport = new Rectangle(0, 0, buff_size, buff_size);
            }
            // renderer state update
            renderer.State.CameraFar = camera.far;
            renderer.State.CameraNear = camera.near;
            renderer.State.CameraViewport = camera.viewport;
            renderer.State.IsOrtho = camera.isOrtho;

#if PROGRAMMABLE_PIPELINE
            // shader uniform block update
            shaderData.Ambient = ambient;// new ColorNormalized(0.3f, 0.2f, 0.1f, 0.6f);
            // directional light
            shaderData.LightPos[0].xyz = new Vector3(-1, 0, 1).normalized;
            shaderData.LightPos[0].w = 0;// directional light
            shaderData.LightColor[0] = lightColor;// new ColorNormalized(1, 0.5f, 0.5f, 1f);
            shaderData.LightItensity[0] = Vector4.one;
            shaderData.LightParams1[0] = Vector4.one;
            shaderData.CameraPos = camera.Translate;
            shaderData.CameraParams = new Vector4(camera.near, camera.far, 0, 0);

            shaderData.NowDataWriteToBuff();

            for (int i = 0; i < gameObjs.Count; i++)
            {
                gameObjs[i].Material.VS.ShaderProperties.SetUniform(outlineOffsetHash, normalOutlineOffset);
                gameObjs[i].Material.FS.ShaderProperties.SetUniform(specularPowHash, specularPow);
            }
#else
            renderer.State.CamX = camera.Translate.x;
            renderer.State.CamY = camera.Translate.y;
            renderer.State.CamZ = camera.Translate.z;
#endif
        }

        public Color ambient { get; set; } = new ColorNormalized(0.3f, 0.2f, 0.1f, 0.6f);
        public Color lightColor { get; set; } = new ColorNormalized(1, 0.5f, 0.5f, 1f);

        private void Draw()
        {
            renderer.Clear();
#if PROGRAMMABLE_PIPELINE
            for (int i = 0; i < gameObjs.Count; i++)
            {
                gameObjs[i].Draw();
            }
#else
            //var srcDepthOffset = renderer.State.DepthOffset;
            //var srcDepthOffsetFactor = renderer.State.DepthOffsetFactor;
            //var srcDepthOffsetUnity = renderer.State.DepthOffsetUnit;

            for (int j = 0; j < triangles.Count; j++)
            {
                //if (j >= 12)
                //{
                //    //renderer.State.DepthOffset = DepthOffset.On;
                //    //renderer.State.DepthOffsetFactor = -1;
                //    //renderer.State.DepthOffsetUnit = -1;
                //}
                renderer.Rasterizer.DrawTriangle(triangles[j], j < 12 ? Color.Red : Color.Green, Color.White);

                //if (j >= 12)
                //{
                //    renderer.State.DepthOffset = srcDepthOffset;
                //    renderer.State.DepthOffsetFactor = srcDepthOffsetFactor;
                //    renderer.State.DepthOffsetUnit = srcDepthOffsetUnity;
                //}
            }
#endif
        }

        private void UpdateGameObjs(float deltaMs)
        {
            GameObject target = null;
            var len = gameObjs.Count;
            for (int i = 0; i < len; i++)
            {
                var go = gameObjs[i];
                go.UpdateTransform(camera);
                go.Update(deltaMs);
                target = go;
            }
            if (target != null)
            {
                //camera.target = target.WorldPosition;
            }
            // dummy : vertex shader
            var cx = camera.viewport.X;
            var cy = camera.viewport.Y;
            var cw = camera.viewport.Width;
            var ch = camera.viewport.Height;
            var f = camera.far;
            var n = camera.near;

            triangles.Clear();
            for (int i = 0; i < len; i++)
            {
                var go = gameObjs[i];
                if (autoRotate)
                {
                    go.LocalRotation += new Vector3(0.1f + i * 0.01f, 0.055f + i * 0.02f, 0.016f + i * 0.03f) * deltaMs;
                }
#if !PROGRAMMABLE_PIPELINE
                if (go.Mesh != null)
                {
                    var mesh = go.Mesh;
                    wposList.Clear();
                    for (int j = 0; j < mesh.vertices.Length; j++)
                    {
                        var clipPos = Matrix4x4.MulPos4(go.ModelViewProjMat, mesh.vertices[j]);
                        // ndc:https://blog.csdn.net/linjf520/article/details/95770635
                        Vector4 ndcPos;
                        if (!camera.isOrtho)
                        {
                            // 透视投影需要处理，透视除法
                            // http://www.songho.ca/opengl/gl_projectionmatrix.html#perspective
                            ndcPos = clipPos * (1 / clipPos.w);
                        }
                        else
                        {
                            ndcPos = clipPos;
                        }
                        // window pos:
                        var wposX = cw * 0.5f * ndcPos.x + (cx + cw * 0.5f);
                        var wposY = ch * 0.5f * ndcPos.y + (cy + ch * 0.5f);
                        var wposZ = (f - n) * 0.5f * ndcPos.z + (f + n) * 0.5f;
                        var winPos = new Vector3(wposX, wposY, wposZ);
                        wposList.Add(winPos);
                    }
                    // polygon mode, now only has the triangle polygon
                    // triangles
                    
                    for (int j = 0; j < mesh.triangles.Length; j += 3)
                    {
                        var idx0 = mesh.triangles[j];
                        var idx1 = mesh.triangles[j + 1];
                        var idx2 = mesh.triangles[j + 2];
                        triangles.Add(new Triangle(wposList[idx0], wposList[idx1], wposList[idx2]));
                    }

                    // draw the triangle3D
                    //renderer.Rasterizer.DrawTriangle()
                    //for (int j = 0; j < triangles.Count; j++)
                    //{
                    //    renderer.Rasterizer.DrawTriangle(triangles[j], Color.Pink, Color.White);
                    //}
                }
#endif
            }
        }

        protected override bool ProcessCmdKey(ref Message m, Keys keyData)
        {
            if ((m.Msg == GlobalMessageHandler.WM_KEYDOWN) || (m.Msg == GlobalMessageHandler.WM_SYSKEYDOWN) || (m.Msg == GlobalMessageHandler.WM_KEYUP))
            {
                var down = (m.Msg == GlobalMessageHandler.WM_KEYDOWN || m.Msg == GlobalMessageHandler.WM_SYSKEYDOWN);
                if (m.Msg == GlobalMessageHandler.WM_KEYUP) down = false;

                this.globalMsg.SetKeyDown(keyData, down);
                //Console.WriteLine($"SetKey:{keyData}, down:{down}");

                //switch (keyData)
                //{
                //    case Keys.Down:
                //        Debug.WriteLine("Down Arrow Captured");
                //        break;

                //    case Keys.Up:
                //        Debug.WriteLine("Up Arrow Captured");
                //        break;

                //    case Keys.Tab:
                //        Debug.WriteLine("Tab Key Captured");
                //        break;

                //    case Keys.Control | Keys.M:
                //        Debug.WriteLine("<CTRL> + M Captured");
                //        break;

                //    case Keys.Alt | Keys.Z:
                //        Debug.WriteLine("<ALT> + Z Captured");
                //        break;
                //    default:
                //        Debug.WriteLine($"Unhandle {keyData} Captured");
                //        break;
                //}
            }

            return base.ProcessCmdKey(ref m, keyData);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == GlobalMessageHandler.WM_ACTIVATEAPP)
            {
                this.globalMsg.appActive = (((int)m.WParam != 0));
                //Console.WriteLine($"Win actived:{this.globalMsg.appActive}");
                return;
            }
            base.WndProc(ref m);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            long nowTick = DateTime.Now.Ticks;

            float dt = nowTick - lastTick;
            dt /= 10000f; // to ms
            if (lastTick == -1) dt = timer1.Interval;

            if (!paused)
            {
                var usingDt = dt * TimeScaleSlider.Value / 500f;
                Update(usingDt);
                UpdateGameObjs(usingDt);
                Draw();
                PictureBox.Image = renderer.SwapBuffer();

                if (fpsShowType == FpsShowType.Immediately)
                {
                    var fps = timer1.Interval / (float)dt * 60;
                    FpsLabel1.Text = $"FPS:{fps.ToString("0.00")}";
                }
                else
                {
                    elapsedTime += dt;
                    fpsCounter++;
                    if (elapsedTime >= 1000)
                    {
                        FpsLabel1.Text = $"FPS:{fpsCounter}";
                        elapsedTime %= 1000;
                        fpsCounter = 0;
                    }
                }
            }
            lastTick = nowTick;

            var curCtrl = GetFocusedControl();
            focusCtrlLabel.Text = curCtrl != null ? $"Focus:{curCtrl.Name}" : "";
            PictureBox.BorderStyle = PictureBox.Focused ? BorderStyle.Fixed3D : BorderStyle.None;
            if (lastFocusControl != curCtrl)
            {
                lastFocusControl = curCtrl;
                if (curCtrl == PictureBox)
                    this.Cursor = Cursors.SizeAll;
                else
                    this.Cursor = Cursors.Default;
            }

            UpdateCameraTR();
        }

        private void PictureBox_MouseDown(object sender, EventArgs e)
        {
            PictureBox.Focus();
        }

        private void RenderBtn_Click(object sender, EventArgs e)
        {
            Draw();
        }

        private void ClearBtn_Click(object sender, EventArgs e)
        {
            renderer.Clear();
        }

        private void PauseBtn_Click(object sender, EventArgs e)
        {
            paused = !paused;
            PauseBtn.Text = paused ? "Continue" : "Pause";
        }

        private void SelectCameraBtn_Click(object sender, EventArgs e)
        {
            PropertyGrid.SelectedObject = camera;
        }

        private void SelectRendererBtn_Click(object sender, EventArgs e)
        {
            PropertyGrid.SelectedObject = renderer;
        }

        private void SelectFormBtn_Click(object sender, EventArgs e)
        {
            PropertyGrid.SelectedObject = this;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var srcPaused = paused;
            paused = false;
            statusLabel.Text = $"Snapshotting...";
            if (!Directory.Exists("Snapshots")) Directory.CreateDirectory("Snapshots");
            var path = $"Snapshots/{DateTime.Now.Ticks}.png";
            renderer.BackbuffSaveAs(path);
            statusLabel.Text = $"Snapshot : {path} Complete.";
            paused = srcPaused;
        }

        private void resetTRS_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < gameObjs.Count; i++)
            {
                gameObjs[i].LocalPosition = Vector3.zero;
                gameObjs[i].LocalRotation = Vector3.zero;
                gameObjs[i].LocalScale = Vector3.one;
            }
        }

        private void TimeScaleSlider_ValueChanged(object sender, EventArgs e)
        {
            TimeScaleValueLabel.Text = TimeScaleSlider.Value.ToString();
        }

        private void ResetCamera_Click(object sender, EventArgs e)
        {
            Vector3 pos = Vector3.zero;
            if (gameObjs.Count > 0)
            {
                pos = gameObjs[0].WorldPosition;
                pos.z += 8;
            }
            else
            {
                pos.x = 0;
                pos.y = 0;
                pos.z = 8;
            }

            camera.RotateTo(Vector3.zero);
            camera.TranslateTo(pos);
        }
    }
}