using Microsoft.Win32;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace LockScreen
{
    /// <summary>
    /// logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InstallMeOnStartUp();

            // set width & height for App
            int width = 0, height = 0;
            foreach (var screen in Screen.AllScreens)
            {
                width += screen.WorkingArea.Width;
                height += screen.WorkingArea.Height;
            }
            this.Width = width;
            this.Height = height;

            // for responsive in multiple Monitors
            if (Screen.AllScreens.Length > 1)
            {
                Grid.SetColumn(mainBox, 2);
                Screen screen = Screen.AllScreens[0];
                int primaryWidth = screen.WorkingArea.Width;
                int primaryHeight = screen.WorkingArea.Height;
                mainBox.Margin = new Thickness(0, 0, ((primaryWidth / 2) - (mainBox.Width / 2)) - 120, 0);
            }

            // fill Settings Values from DataBase
            fillFromSettings();

            // hook keyboard
            IntPtr hModule = GetModuleHandle(IntPtr.Zero);
            hookProc = new LowLevelKeyboardProcDelegate(LowLevelKeyboardProc);
            hHook = SetWindowsHookEx(WH_KEYBOARD_LL, hookProc, hModule, 0);
            DisableTaskManager();
            //if (hHook == IntPtr.Zero)
            //{
            //    Console.WriteLine("Failed to set hook, error = " + Marshal.GetLastWin32Error());
            //}
        }

        /// <summary>
        /// fill Settings Values from DataBase
        /// </summary>
        private void fillFromSettings()
        {
            try
            {

            }
            catch { }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (FloatingPasswordBox.Password != "@dmin")
            {
                ErrorGrid.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(244, 67, 54));
                ErrorMessage.Text = "رمز عبور نامعتبر است";
            }
            else
            {
                ErrorGrid.Background = new SolidColorBrush(System.Windows.Media.Color.FromRgb(76, 175, 80));
                ErrorMessage.Text = "با موفقیت وارد می شوید";
                System.Windows.Application.Current.Shutdown();
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            UnhookWindowsHookEx(hHook); // release keyboard hook
            EnableCTRLALTDEL();
        }

        //private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        //{
        //    try
        //    {
        //        if (Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.F4 ||
        //          Keyboard.Modifiers == ModifierKeys.Control && e.SystemKey == Key.Escape ||
        //                Keyboard.Modifiers == ModifierKeys.Alt && e.SystemKey == Key.Tab)
        //        {
        //            e.Handled = true;
        //        }

        //        else if (Keyboard.Modifiers == ModifierKeys.Windows)
        //        {
        //            e.Handled = true;
        //        }
        //        else
        //        {
        //            base.OnPreviewKeyDown(e);
        //        }
        //    }
        //    catch { }
        //}

        void InstallMeOnStartUp()
        {
            try
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                if (key.GetValue(curAssembly.GetName().Name) == null)
                    key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
            }
            catch { }
        }

        private void DisableTaskManager()
        {
            RegistryKey regkey = default(RegistryKey);
            string keyValueInt = "1";
            string subKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";
            try
            {
                regkey = Registry.CurrentUser.CreateSubKey(subKey);
                regkey.SetValue("DisableTaskMgr", keyValueInt);
                regkey.Close();
            }
            catch (Exception ex)
            {
                //System.Windows.MessageBox.Show("Error " + ex.Message);
            }
        }

        public static void EnableCTRLALTDEL()
        {
            try
            {
                string subKey = "Software\\Microsoft\\Windows\\CurrentVersion\\Policies\\System";
                RegistryKey rk = Registry.CurrentUser;
                RegistryKey sk1 = rk.OpenSubKey(subKey);
                if (sk1 != null)
                    rk.DeleteSubKeyTree(subKey);
            }
            catch { }
        }

        // Disable Real All Key out of Program (Except ctrl+alt+delete)
        private struct KBDLLHOOKSTRUCT
        {
            public int vkCode;
            int scanCode;
            public int flags;
            int time;
            int dwExtraInfo;
        }

        private delegate int LowLevelKeyboardProcDelegate(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProcDelegate lpfn, IntPtr hMod, int dwThreadId);

        [DllImport("user32.dll")]
        private static extern bool UnhookWindowsHookEx(IntPtr hHook);

        [DllImport("user32.dll")]
        private static extern int CallNextHookEx(int hHook, int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetModuleHandle(IntPtr path);

        private IntPtr hHook;
        LowLevelKeyboardProcDelegate hookProc; // prevent gc
        const int WH_KEYBOARD_LL = 13;

        private static int LowLevelKeyboardProc(int nCode, int wParam, ref KBDLLHOOKSTRUCT lParam)
        {
            if (nCode >= 0)
                switch (wParam)
                {
                    case 256: // WM_KEYDOWN
                    case 257: // WM_KEYUP
                    case 260: // WM_SYSKEYDOWN
                    case 261: // M_SYSKEYUP
                        if (
                            (lParam.vkCode == 0x09 && lParam.flags == 32) || // Alt+Tab
                            (lParam.vkCode == 0x1b && lParam.flags == 32) || // Alt+Esc
                            (lParam.vkCode == 0x73 && lParam.flags == 32) || // Alt+F4
                            (lParam.vkCode == 0x1b && lParam.flags == 0) || // Ctrl+Esc
                            (lParam.vkCode == 0x5b && lParam.flags == 1) || // Left Windows Key 
                            (lParam.vkCode == 0x5c && lParam.flags == 1))    // Right Windows Key 
                        {
                            return 1; //Do not handle key events
                        }
                        break;
                }
            return CallNextHookEx(0, nCode, wParam, ref lParam);
        }

        private void Chip_Click(object sender, RoutedEventArgs e)
        {
            if (settingBox.Visibility == Visibility.Visible)
            {
                DoubleAnimation animation0 = new DoubleAnimation();
                animation0.From = settingBox.ActualHeight;
                animation0.To = 0;
                animation0.Duration = new Duration(TimeSpan.FromMilliseconds(150));
                animation0.Completed += Animation0_Completed;
                settingBox.BeginAnimation(HeightProperty, animation0);
            }
            else
            {
                settingBox.Visibility = Visibility.Visible;

                DoubleAnimation animation1 = new DoubleAnimation();
                animation1.From = 0;
                animation1.To = 350;
                animation1.Duration = new Duration(TimeSpan.FromMilliseconds(150));
                settingBox.BeginAnimation(HeightProperty, animation1);
            }
        }

        private void Animation0_Completed(object sender, EventArgs e)
        {
            settingBox.Visibility = Visibility.Collapsed;
        }


    }
}
