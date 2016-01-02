using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows;

namespace MusicAnalyzer.Tools
{
    class SystemTools
    {
        private const int APPCOMMAND_VOLUME_UP = 0xA0000;
        private const int APPCOMMAND_VOLUME_DOWN = 0x90000;
        private const int WM_APPCOMMAND = 0x319;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessageW(IntPtr hWnd, int Msg,
            IntPtr wParam, IntPtr lParam);

        public static void VolDown(Window window)
        {
            var wih = new System.Windows.Interop.WindowInteropHelper(window);
            IntPtr hWnd = wih.Handle;
            SendMessageW(hWnd, WM_APPCOMMAND, hWnd, (IntPtr)APPCOMMAND_VOLUME_DOWN);
        }

        public static void VolUp(Window window)
        {
            var wih = new System.Windows.Interop.WindowInteropHelper(window);
            IntPtr hWnd = wih.Handle;
            SendMessageW(hWnd, WM_APPCOMMAND, hWnd, (IntPtr)APPCOMMAND_VOLUME_UP);
        }
    }
}
