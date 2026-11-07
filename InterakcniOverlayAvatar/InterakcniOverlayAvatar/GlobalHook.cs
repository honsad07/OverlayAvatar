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

        const int WM_KEYDOWN = 0x0100;
        const int WM_SYSKEYDOWN = 0x0104;

        // --- P/Invoke ---
        // Funkce k zapnutí odposlechu
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(int idHook, LowLevelHookProc lpfn, IntPtr hMod, uint dwThreadId);

        // Funkce k vypnutí odposlechu
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool UnhookWindowsHookEx(IntPtr hhk);

        // Zjistí jméno programu
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        // Funkce pro předání zprávy dál, aby se nezasekl systém
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        
        // Delegát pro parametr v zapínání odposlechu, potřebuje více parametrů pro odpověď
        delegate IntPtr LowLevelHookProc(int nCode, IntPtr wParam, IntPtr lParam);


        IntPtr keyboardHookId = IntPtr.Zero; // Potvrzuje zapnutí odposlechu
        // Proměnná aby garbage collector nesmazal funkc
        LowLevelHookProc keyboardProc;

        // Odkaz na okno programu
        MainWindow okno;

        public GlobalHook(MainWindow hlavniOkno)
        {
            okno = hlavniOkno;
            keyboardProc = HookCallbackKeyboard;
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
            }
        }

        public void Stop()
        {
            // Vypnutí odposlechu
            UnhookWindowsHookEx(keyboardHookId);
        }

        private IntPtr HookCallbackKeyboard(int nCode, IntPtr wParam, IntPtr lParam)
        {
            // Zda je platná zpráva systému
            if (nCode >= 0)
            {
                if (wParam == (IntPtr)(WM_KEYDOWN) || wParam == (IntPtr)(WM_SYSKEYDOWN))
                {
                    okno.ZaznamenejAktivitu();
                }
            }

            // Vrací stisknutou klávesu
            return CallNextHookEx(keyboardHookId, nCode, wParam, lParam);
        }
    }
}
