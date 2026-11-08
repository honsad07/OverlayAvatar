using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace InterakcniOverlayAvatar
{
    internal class GlobalHook
    {
        // Konstanta pro záchyt klávesnice
        const int WH_KEYBOARD_LL = 13;
        const int WH_MOUSE_LL = 14;

        // Stisk klávesnice
        const int WM_KEYDOWN = 0x0100;
        const int WM_SYSKEYDOWN = 0x0104;

        // Opuštění klávesnice (držení jinak rapidně inkrementuje)
        const int WM_KEYUP = 0x0101;
        const int WM_SYSKEYUP = 0x0105;

        // Stisk myši
        const int WM_LBUTTONDOWN = 0x0201;
        const int WM_MBUTTONDOWN = 0x0207;
        const int WM_RBUTTONDOWN = 0x0204;


        // --- P/Invoke ---
        // Funkce k zapnutí odposlechu
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(int idHook, LowLevelHookProc lpfn, IntPtr hMod, uint dwThreadId);

        // Funkce k vypnutí odposlechu
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        // Zjistí jméno programu, nepatří do user32.dll!
        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        // Funkce pro předání zprávy dál, aby se nezasekl systém
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        
        // Delegát pro parametr v zapínání odposlechu, potřebuje více parametrů pro odpověď
        delegate IntPtr LowLevelHookProc(int nCode, IntPtr wParam, IntPtr lParam);


        // Potvrzuje zapnutí odposlechu
        IntPtr keyboardHookId = IntPtr.Zero;
        IntPtr mouseHookId = IntPtr.Zero;

        // Proměnná aby garbage collector nesmazal funkci
        LowLevelHookProc keyboardProc;
        LowLevelHookProc mouseProc;

        // Odkaz na okno programu
        MainWindow okno;
        List<int> drzeneKlavesy = new List<int>();

        public GlobalHook(MainWindow hlavniOkno)
        {
            okno = hlavniOkno;
            keyboardProc = HookCallbackKeyboard;
            mouseProc = HookCallbackMouse;
        }

        public void Start()
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                // Zjištuje jméno programu
                IntPtr handleModulu = GetModuleHandle(curModule.ModuleName);

                // Zapnutí odposlechu
                keyboardHookId = SetWindowsHookEx(WH_KEYBOARD_LL, keyboardProc, handleModulu, 0);
                mouseHookId = SetWindowsHookEx(WH_MOUSE_LL, mouseProc, handleModulu, 0);
            }
        }

        public void Stop()
        {
            // Vypnutí odposlechu
            UnhookWindowsHookEx(keyboardHookId);
            UnhookWindowsHookEx(mouseHookId);
        }

        private IntPtr HookCallbackKeyboard(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // Zda je platná zpráva systému
            if (nCode >= 0)
            {
                int idKlavesy = Marshal.ReadInt32(lParam);

                if (wParam == (IntPtr)(WM_KEYDOWN) || wParam == (IntPtr)(WM_SYSKEYDOWN))
                {
                    if (!drzeneKlavesy.Contains(idKlavesy))
                    {
                        drzeneKlavesy.Add(idKlavesy);
                        okno.ZaznamenejAktivitu();
                    }
                }
                else if (wParam == (IntPtr)WM_KEYUP || wParam == (IntPtr)WM_SYSKEYUP)
                {
                    // Opuštění klávesy
                    drzeneKlavesy.Remove(idKlavesy);
                }
            }

            // Vrací stisknutou klávesu
            return CallNextHookEx(keyboardHookId, nCode, wParam, lParam);
        }

        private IntPtr HookCallbackMouse(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // Zda je platná zpráva systému
            if (nCode >= 0)
            {
                if (wParam == (IntPtr)(WM_LBUTTONDOWN) || wParam == (IntPtr)(WM_RBUTTONDOWN) || wParam == (IntPtr)(WM_MBUTTONDOWN))
                {
                    okno.ZaznamenejAktivitu();
                }
            }

            // Předává zprávu dalším programům v systému
            return CallNextHookEx(mouseHookId, nCode, wParam, lParam);
        }
    }
}
