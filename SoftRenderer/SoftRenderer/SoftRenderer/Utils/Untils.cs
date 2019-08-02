// jave.lin 2019.07.22
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace SoftRenderer.SoftRenderer.Utils
{
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

    public class GlobalMessageHandler : IMessageFilter
    {
        public delegate void OnWheel(bool fromDownToUp);
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

        public bool appActive { get; set; }
        public Point last_ms_pos { get; private set; } = new Point(-1, -1);
        public Point delta_ms_pos { get; set; }
        public bool MS_LBTN { get; private set; }
        public bool MS_RBTN { get; private set; }
        public bool MS_MBTN { get; private set; }

        private Dictionary<Keys, bool> keysDownStatus = new Dictionary<Keys, bool>();

        public event OnWheel OnWheelEvent;

        public void SetKeyDown(Keys k, bool isDown)
        {
            if ((k & Keys.Control) != 0)
                k = k ^ Keys.Control;
            keysDownStatus[k] = isDown;
        }
        public bool GetKeyDown(Keys k)
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
                    MS_LBTN = true;
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
                    MS_RBTN = true;
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
            }
            // Always allow message to continue to the next filter control
            return false;
        }

        #endregion
    }
}
