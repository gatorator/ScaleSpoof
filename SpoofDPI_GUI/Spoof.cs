using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SpoofDPI_GUI
{
    internal class Spoof
    {
        #region Win32 #define
        private const int USER_DEFAULT_SCREEN_DPI = 96;

        private const uint RDW_UPDATENOW = 0x0100;
        private const uint RDW_INVALIDATE = 0x0001;
        private const uint RDW_ERASE = 0x0004;
        private const uint RDW_ALLCHILDREN = 0x0080;

        private const uint WM_DPICHANGED = 0x02E0;

        private const uint SWP_NOMOVE = 0x0002;
        private const uint SWP_NOSIZE = 0x0001;
        private const uint SWP_NOZORDER = 0x0004;
        private const uint SWP_FRAMECHANGED = 0x0020;

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public Point ptMinPosition;
            public Point ptMaxPosition;
            public Rect rcNormalPosition;
        }
        #endregion


        private const string DLL_PATH = ".\\InjectDPI.dll";

        private static string dllName = Path.GetFileName(DLL_PATH);

        public static readonly int[] scaleFactorArray = { 100, 120, 125, 140, 150, 160, 175, 180, 200, 225, 250, 300, 350, 400, 450, 500 };

        public static Dictionary<uint, ProcInfo> injectedProcs = new();

        private static ProcInfo _currProcInfo;
        public static ProcInfo CurrProcInfo 
        { 
            get => _currProcInfo;
        }
        public static int CurrScaleFactor
        {
            get => _currProcInfo.ScaleFactor;
            set => _currProcInfo.ScaleFactor = value;
        }

        public struct ProcInfo
        {
            public uint Id;
            public string Name;
            public Image Icon;
            public Point WindowPos;
            public Size WindowSize;

            public IntPtr TopHwnd;

            public bool Injected;
            public bool HookEnabled;
            public int ScaleFactor;

            private Rect _windowRect;
            public Rect WindowRect
            {
                get => _windowRect;
                set
                {
                    _windowRect = value;
                    WindowPos = new Point(value.Left, value.Top);
                    WindowSize = new Size(value.Right - value.Left, value.Bottom - value.Top);
                }
            }
        }

        public static bool SetCurrProcFromCursorPoint()
        {
            Point cursorPoint = new Point();
            GetCursorPos(ref cursorPoint);

            IntPtr newHwnd = GetAncestor(WindowFromPoint(cursorPoint), 3);
            if (_currProcInfo.TopHwnd != newHwnd)
            {
                uint newProcId;
                GetWindowThreadProcessId(newHwnd, out _currProcInfo.Id);
                return SetCurrProcFromProcId(_currProcInfo.Id, newHwnd);
            }
            return (_currProcInfo.Id != 0);
        }

        public static bool SetCurrProcFromProcId(uint procId, IntPtr hWnd = -1)
        {   
            if (!injectedProcs.ContainsKey(procId))
            {
                _currProcInfo.Name = "[No process selected]";
                _currProcInfo.Id = 0;
                _currProcInfo.Icon = null;

                _currProcInfo.ScaleFactor = -1;
                _currProcInfo.Injected = false;
                _currProcInfo.HookEnabled = false;

                if (procId == 0)
                {
                    return false;
                }

                SetHWndForCurrProc(hWnd);

                if (procId == Process.GetCurrentProcess().Id)
                {
                    _currProcInfo.Name = "[Invalid process]";
                    return false;
                }

                Process currProc = Process.GetProcessById(checked((int)procId));

                try
                {
                    _currProcInfo.Icon = Icon.ExtractAssociatedIcon(currProc.MainModule.FileName).ToBitmap();
                    _currProcInfo.Name = currProc.ProcessName + ".exe";
                    
                    foreach (ProcessModule module in currProc.Modules)
                    {
                        if (module.ModuleName == dllName)
                        {
                            _currProcInfo.Injected = true;
                            _currProcInfo.HookEnabled = true;
                        }
                    }
                }
                catch (Exception)
                {
                    _currProcInfo.Name = "[Insufficient permissions]";
                    return false;
                }

                try
                {
                    currProc.EnableRaisingEvents = true;
                    currProc.Exited += (s, e) =>
                    {
                        Process process = s as Process;
                        injectedProcs.Remove(checked((uint)process.Id));
                        SetCurrProcFromProcId(0);
                    };
                }
                catch (Exception)
                {
                    return false;
                }

                _currProcInfo.Id = procId;
            }
            else
            {
                _currProcInfo = injectedProcs[procId];
                SetHWndForCurrProc(hWnd);
            }

            return true;
        }

        private static void SetHWndForCurrProc(IntPtr hWnd = -1)
        {
            if (hWnd != -1)
            {
                _currProcInfo.TopHwnd = hWnd;
                Rect windowRect = new Rect();
                GetWindowRect(hWnd, ref windowRect);
                _currProcInfo.WindowRect = windowRect;
            }
        }

        public static void UpdateInjectedProcsWithCurrProc()
        {
            if (injectedProcs.ContainsKey(_currProcInfo.Id))
            {
                injectedProcs.Remove(_currProcInfo.Id);
            }
            injectedProcs.Add(_currProcInfo.Id, _currProcInfo);
        }

        public static void RefreshCurrProcWindow(Form sender)
        {
            double factor = (CurrScaleFactor / 100);
            uint dpi = checked((uint)(USER_DEFAULT_SCREEN_DPI * factor));
            IntPtr wParam = (IntPtr)((dpi << 16) | dpi);

            Rect windowRect = _currProcInfo.WindowRect;
            windowRect.Bottom = windowRect.Top + (int)((windowRect.Bottom - windowRect.Top) / factor);
            windowRect.Right = windowRect.Left + (int)((windowRect.Right - windowRect.Left) / factor);

            IntPtr lParam = Marshal.AllocHGlobal(Marshal.SizeOf(windowRect));
            try
            {
                Marshal.StructureToPtr(windowRect, lParam, false);

                SendMessage(_currProcInfo.TopHwnd, WM_DPICHANGED, wParam, lParam);
            }
            finally
            {
                Marshal.FreeHGlobal(lParam);
            }

            RedrawWindow(_currProcInfo.TopHwnd, IntPtr.Zero, IntPtr.Zero, RDW_INVALIDATE | RDW_UPDATENOW | RDW_ERASE | RDW_ALLCHILDREN);

            SetWindowPos(_currProcInfo.TopHwnd, IntPtr.Zero, 0, 0, 0, 0, SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);

            WINDOWPLACEMENT currWinPlacement = new();
            GetWindowPlacement(_currProcInfo.TopHwnd, ref currWinPlacement);

            ShowWindow(_currProcInfo.TopHwnd, 0);
            ShowWindow(_currProcInfo.TopHwnd, 3);
            ShowWindow(_currProcInfo.TopHwnd, 1);
            ShowWindow(_currProcInfo.TopHwnd, 9);
            ShowWindow(_currProcInfo.TopHwnd, currWinPlacement.showCmd);

            sender.Activate();
        }

        public static bool Install()
        {
            if (!_currProcInfo.Injected)
            {
                _currProcInfo.Injected = Install(_currProcInfo.Id, _currProcInfo.ScaleFactor);
                if (_currProcInfo.Injected)
                {
                    _currProcInfo.HookEnabled = true;
                    UpdateInjectedProcsWithCurrProc();
                }
            }
            else
            {
                UpdateInjectedProcsWithCurrProc();
                SetScaleFactor();
                Toggle();
                Toggle();
            }
            
            return _currProcInfo.Injected;
        }

        public static bool Toggle()
        {
            if (injectedProcs.ContainsKey(_currProcInfo.Id))
            {
                bool success;
                if (_currProcInfo.HookEnabled)
                {
                    success = Disable(_currProcInfo.Id);
                }
                else
                {
                    success = Enable(_currProcInfo.Id);
                }

                if (success)
                {
                    _currProcInfo.HookEnabled = !_currProcInfo.HookEnabled;
                    UpdateInjectedProcsWithCurrProc();
                }
                return success;
            }
            return false;
        }

        public static bool SetScaleFactor()
        {
            if (injectedProcs.ContainsKey(_currProcInfo.Id))
            {
                bool success = SetScaleFactor(_currProcInfo.Id, CurrScaleFactor);
                if (success)
                {
                    UpdateInjectedProcsWithCurrProc();
                }
                return success;
            }
            return false;
        }

        #region Win32

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(Point loc);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern bool GetCursorPos(ref Point lpPoint);

        [DllImport("user32.dll")]
        private static extern bool GetWindowRect(IntPtr hWnd, ref Rect lpRect);

        [DllImport("user32.dll")]
        private static extern IntPtr GetAncestor(IntPtr hWnd, uint gaFlags);

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern bool RedrawWindow(IntPtr hWnd, IntPtr lprcUpdate, IntPtr hrgnUpdate, uint flags);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, ref WINDOWPLACEMENT lpwndpl);

        #endregion

        [DllImport(DLL_PATH)]
        private static extern bool Install(UInt32 processId, int scaleFactor);

        [DllImport(DLL_PATH)]
        private static extern bool Enable(UInt32 processId);

        [DllImport(DLL_PATH)]
        private static extern bool Disable(UInt32 processId);

        [DllImport(DLL_PATH)]
        private static extern bool SetScaleFactor(UInt32 processId, int scaleFactor);
    }
}
