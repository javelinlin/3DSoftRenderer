// jave.lin 2019.07.22
using RendererCore.Games;
using RendererCoreCommon.Renderer.Common.Mathes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace RendererCore.Renderer.Utils
{
    [Description("对象池类")]
    public class Pool<T> : IDisposable where T : IDisposable, new()
    {
        private int max;
        private int maxMinusOne;
        private List<T> pool;

        public int Count { get; private set; }

        public Pool(int max)
        {
            this.max = max;
            this.maxMinusOne = max - 1;
            pool = new List<T>(max);
            for (int i = 0; i < max; i++)
            {
                pool.Add(default(T));
            }
        }

        public Object Get()
        {
            if (Count > 0)
            {
                var result = pool[Count - 1];
                --Count;
                return result;
            }
            else
            {
                return Activator.CreateInstance(typeof(T));
                //return new T();
            }
        }

        public void To(T instance)
        {
            if (Count > maxMinusOne)
            {
                instance.Dispose();
                return;
            }
            pool[Count++] = instance;
        }

        public void Dispose()
        {
            if (pool != null)
            {
                foreach (var item in pool)
                {
                    item.Dispose();
                }
                pool.Clear();
                pool = null;
            }
        }
    }

    [Description("全局消息处理类")]
    public class GlobalMessageHandler : IMessageFilter
    {
        public delegate void OnWheel(bool fromDownToUp);
        public delegate void OnMsRDown();
        public delegate void OnAltAndLDown();

        /* =======message start======= */
        public const int WM_KEYDOWN = 0x100;
        public const int WM_KEYUP = 0x101;
        public const int WM_SYSKEYDOWN = 0x104;
        public const int WM_SYSKEYUP = 0x105;

        //https://docs.microsoft.com/zh-cn/dotnet/api/system.windows.forms.message.msg?view=netframework-4.8
        public const int WM_ACTIVATEAPP = 0x001C;

        // https://docs.microsoft.com/zh-cn/previous-versions/windows/desktop/inputmsg/wm-parentnotify
        // CREATE WIN, DESTROY WIN, MS_LBTN, MS_RBTN, MS_MBTN, ETC.
        public const int WM_LBUTTONDOWN = 0x0201;
        public const int WM_LBUTTONUP = 0x0202;
        public const int WM_MBUTTONDOWN = 0x0207;
        public const int WM_MBUTTONUP = 0x0208;
        public const int WM_RBUTTONDOWN = 0x0204;
        public const int WM_RBUTTONUP = 0x0205;
        public const int WM_MOUSEWHEEL = 0x020A;
        // https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-ncmousemove
        //https://docs.microsoft.com/en-us/windows/win32/inputdev/wm-mousemove
        // MS_MOVE
        //private const int WM_NCMOUSEMOVE = 0x00A0;
        public const int WM_MOUSEMOVE = 0x0200;

        public bool appActive { get; set; }         // 程序是否有激活
        private Point _last_alt_and_ms_ldown_pos;
        public Point last_alt_and_ms_ldown_pos { get => _last_alt_and_ms_ldown_pos;
            private set {
                _last_alt_and_ms_ldown_pos = value;
                OnAltAndLDownEvent.Invoke();
            } } // 上次alt+鼠标左键点下的鼠标坐标
        public Point last_ms_rdown_pos { get; private set; } // 上次鼠标右键点下的鼠标坐标
        public Point last_ms_pos { get; private set; } = new Point(-1, -1); // 上次鼠标的位置
        public Point delta_ms_pos { get; set; }     // 与上次鼠标的偏移差
        public bool MS_LBTN { get; private set; }   // 鼠标左键
        public bool MS_RBTN { get; private set; }   // 鼠标右键
        public bool MS_MBTN { get; private set; }   // 鼠标中键
        public Keys ControlKeys { get; set; }       // 设置或获取命令键组合值
        public Keys MenuAltKeys { get; set; }       // 设置或获取菜单键组合值
        public bool IsCtrlDown { get => (ControlKeys & Keys.ControlKey) != 0; }     // CTRL 键是否按下
        public bool IsAltDown { get => ((MenuAltKeys & Keys.Menu) != 0 && (MenuAltKeys & Keys.Alt) != 0); }            // ALT 键是否按下

        private Dictionary<Keys, bool> keysDownStatus = new Dictionary<Keys, bool>();

        public event OnWheel OnWheelEvent;
        public event OnMsRDown OnMsRDownEvent;
        public event OnAltAndLDown OnAltAndLDownEvent;

        public void SetKeyDown(Keys k, bool isDown) // 设置普通按键状态
        {
            keysDownStatus[k] = isDown;
        }
        public bool GetKeyDown(Keys k)              // 获取普通按键状态
        {
            keysDownStatus.TryGetValue(k, out bool down);
            return down;
        }
        /* =======message end======= */

        #region IMessageFilter Members

        public bool PreFilterMessage(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_LBUTTONDOWN:
                    var srcLbtn = MS_LBTN;
                    MS_LBTN = true;
                    if (!srcLbtn && IsAltDown && this.MS_LBTN)
                        last_alt_and_ms_ldown_pos = Cursor.Position;
                    break;
                case WM_LBUTTONUP:
                    MS_LBTN = false;
                    break;
                case WM_MBUTTONDOWN:
                    MS_MBTN = true;
                    break;
                case WM_MBUTTONUP:
                    MS_MBTN = false;
                    break;
                case WM_RBUTTONDOWN:
                    if (!MS_RBTN)
                    {
                        MS_RBTN = true;
                        last_ms_rdown_pos = Cursor.Position;
                        OnMsRDownEvent.Invoke();
                    }
                    break;
                case WM_RBUTTONUP:
                    MS_RBTN = false;
                    break;
                case WM_MOUSEWHEEL:
                    OnWheelEvent.Invoke((int)m.WParam > 0);
                    break;
                case WM_MOUSEMOVE:
                    var nowPos = Cursor.Position;
                    if (last_ms_pos.X == -1 || last_ms_pos.Y == -1)
                        last_ms_pos = nowPos;
                    delta_ms_pos = new Point(nowPos.X - last_ms_pos.X, nowPos.Y - last_ms_pos.Y);
                    last_ms_pos = nowPos;
                    //Console.WriteLine($"mx,y:{nowPos}, deltax,y:{delta_ms_pos}");
                    break;
                case WM_ACTIVATEAPP:
                    appActive = m.WParam != IntPtr.Zero;
                    //Console.WriteLine($"win actived:{appActive}");
                    break;
                case WM_KEYUP:
                    SetKeyDown((Keys)m.WParam, false);
                    //Console.WriteLine($"Key{(Keys)m.WParam} up:true");
                    break;
                case WM_SYSKEYUP:
                    var srcAltDown = IsAltDown;
                    MenuAltKeys = (Keys)m.WParam;
                    if (!srcAltDown && IsAltDown && this.MS_LBTN)
                        last_alt_and_ms_ldown_pos = Cursor.Position;
                    break;
            }
            
            // Always allow message to continue to the next filter control
            return false;
        }

        #endregion
    }

    [Description("模型读取器")]
    public static class ModelReader
    {
        protected enum DataType
        {
            none,
            vertices,
            indices,
            colors,
            uv,
            normals,
            tangent,



            exit,
        }

        public static void ReadOut(string file, out Mesh mesh)
        {
            mesh = new Mesh();
            const string label = "#";
            const string vertices = "vertices";
            const string indices = "indices";
            const string colors = "colors";
            const string uv = "uv"; // uv_n, 这里我们只支持一个uv
            const string normals = "normals";
            const string tangents = "tangents";

            var label_spliter = new char[] { ':' };

            using (var txtReader = new StreamReader(file, Encoding.UTF8))
            {
                var type = DataType.none;
                var count = 0;

                while (!txtReader.EndOfStream)
                {
                    if (type == DataType.none)
                    {
                        var line = txtReader.ReadLine();
                        if (line.StartsWith(label))
                        {
                            var str = line.Substring(1);
                            var arr = str.Split(label_spliter);
                            var labelName = arr[0];
                            count = System.Convert.ToInt32(arr[1]);
                            if (labelName == vertices) type = DataType.vertices;
                            else if (labelName == indices) type = DataType.indices;
                            else if (labelName == colors) type = DataType.colors;
                            else if (labelName == uv) type = DataType.uv;
                            else if (labelName == normals) type = DataType.normals;
                            else if (labelName == tangents) type = DataType.tangent;
                            continue;
                        }
                    }
                    else if (type == DataType.vertices)
                    {
                        mesh.vertices = ReadDatType<Vector3>(count, txtReader);
                        type = DataType.none;
                    }
                    else if (type == DataType.indices)
                    {
                        // indices沒有基础IConvert<T>所以手写在这
                        // 也可以写另一个类将它int[]包起来，再实现IConvert<T>即可
                        var result = new int[count];
                        var spliter = new char[] { ',' };
                        for (int i = 0; i < count; i += 3)
                        {
                            var line = txtReader.ReadLine();
                            var arr = line.Split(spliter);
                            for (int j = 0; j < 3; j++)
                            {
                                result[i + j] = System.Convert.ToInt32(arr[j]);
                            }
                        }
                        mesh.triangles = result;
                        type = DataType.none;
                    }
                    else if (type == DataType.colors)
                    {
                        mesh.colors = ReadDatType<Vector4>(count, txtReader);
                        type = DataType.none;
                    }
                    else if (type == DataType.uv)
                    {
                        mesh.uv = ReadDatType<Vector2>(count, txtReader);
                        type = DataType.none;
                    }
                    else if (type == DataType.normals)
                    {
                        mesh.normals = ReadDatType<Vector3>(count, txtReader);
                        type = DataType.none;
                    }
                    else if (type == DataType.tangent)
                    {
                        mesh.tangents = ReadDatType<Vector3>(count, txtReader);
                        type = DataType.none;
                    }
                    else
                    {
                        throw new Exception($"not implements:{type}");
                    }

                    if (type == DataType.exit) break;
                }

                txtReader.Close();
            }

            var len = mesh.normals.Length;
            for (int i = 0; i < len; i++)
            {
                mesh.normals[i].Normalize();
            }
            len = mesh.tangents.Length;
            for (int i = 0; i < len; i++)
            {
                mesh.tangents[i].Normalize();
            }
        }

        public static T[] ReadDatType<T>(int count, StreamReader txtReader) where T : IConvert<T>
        {
            var result = new T[count];
            for (int i = 0; i < count; i++)
            {
                result[i] = Convert<T>(txtReader.ReadLine());
            }
            return result;
        }

        public static T Convert<T>(string str) where T : IConvert<T>
        {
            T result = default(T);
            result.ConvertFrom(str);
            return result;
        }
    }
}
