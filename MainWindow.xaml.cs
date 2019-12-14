using Microsoft.Win32;
using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media.Animation;
using Models;
using Notifications.Wpf;

namespace LockScreen
{
    // logic for MainWindow.xaml
    public partial class MainWindow : Window
    {
        DataBase<tbl_Setting> liteDb;

        public MainWindow()
        {
            try
            {
                InitializeComponent();

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

                liteDb = new Models.DataBase<tbl_Setting>();

                // fill Settings Values from DataBase
                fillSettings();

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
            catch (Exception ex) { Debug.Text(ex, "MainWindow()"); }
        }

        // fill Settings Values from DataBase
        private void fillSettings()
        {
            try
            {
                tbl_Setting setting = liteDb.Select(1);
                titleTextBox.Text = txtTitle.Text = setting.title;
                startUpSwitch.IsChecked = setting.isStartUp;
            }
            catch (Exception ex) { Debug.Text(ex, "fillSettings()"); }
        }

        private void BtnLogin_Click(object sender, RoutedEventArgs e)
        {
            if (FloatingPasswordBox.Password == liteDb.Select(1).passWord)
            {
                Message("موفقیت", "در حال ورود به سیستم", NotificationType.Success);
                System.Windows.Application.Current.Shutdown();
            }
            else
            {
                Message("خطا", "رمز عبور نامعتبر است", NotificationType.Error);
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            try
            {
                UnhookWindowsHookEx(hHook); // release keyboard hook
                EnableCTRLALTDEL();
            }
            catch (Exception ex) { Debug.Text(ex, "Window_Closed()"); }
        }

        // hide/show setting box 
        void show_hideSettingBox()
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

        private void Chip_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Animation0_Completed(object sender, EventArgs e)
        {
            settingBox.Visibility = Visibility.Collapsed;
        }

        private void btnSetting_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrEmpty(myPasswordBox.Password) && myPasswordBox.Password == myPasswordBoxRepeat.Password)
                {
                    tbl_Setting setting = liteDb.Select(1);
                    setting.isStartUp = startUpSwitch.IsChecked == true ? true : false;
                    setting.passWord = myPasswordBox.Password;
                    setting.title = titleTextBox.Text;

                    if (liteDb.Update(setting))
                    {
                        InstallMeOnStartUp(setting.isStartUp);
                        Message("موفقیت", "اطلاعات ذخیره شد", NotificationType.Information);
                        show_hideSettingBox();
                    }
                    else
                    {
                        Message("خطای برنامه", "مشکلی پیش آمده است لطفا دوباره تلاش کنید", NotificationType.Information);
                    }
                }
                else
                {
                    Message("هشدار", "رمز عبورها باید یکسان باشند و نمیتواند خالی باشد", NotificationType.Warning);
                }
            }
            catch (Exception ex) { Debug.Text(ex, "btnSetting_Click()"); }
        }

        private void ButtonSetting_Click(object sender, RoutedEventArgs e)
        {
            if (FloatingPasswordBox.Password == liteDb.Select(1).passWord)
            {
                Message("موفقیت", "به تنظیمات خوش آمدید", NotificationType.Success);
                show_hideSettingBox();
            }
            else
            {
                Message("خطا", "رمز عبور نامعتبر است", NotificationType.Error);
            }
        }

        // show notification message 
        // <param name="type">blue-green-red-yellow</param>
        public void Message(string title, string message, NotificationType type)
        {
            try
            {
                WindowArea.Show(new NotificationContent
                {
                    Type = type,
                    Message = message,
                    Title = title
                }
                , TimeSpan.FromSeconds(3), null, null);
                //, TimeSpan.FromSeconds(3), onClick: () => Console.WriteLine("Click"), onClose: () => Console.WriteLine("Closed!"));
            }
            catch (Exception ex) { Debug.Text(ex, "Message()"); }
        }

        void InstallMeOnStartUp(bool setInStartUp = true)
        {
            try
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                if (setInStartUp)
                {
                    if (key.GetValue(curAssembly.GetName().Name) == null)
                        key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
                }
                else
                {
                    if (key.GetValue(curAssembly.GetName().Name) != null)
                        key.SetValue(curAssembly.GetName().Name, null);
                }
            }
            catch (Exception ex) { Debug.Text(ex, "InstallMeOnStartUp()"); }
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
            catch (Exception ex) { Debug.Text(ex, "DisableTaskManager()"); }
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
            catch (Exception ex) { Debug.Text(ex, "EnableCTRLALTDEL()"); }
        }

        // Disable Real All Key out of Program (Except ctrl+alt+delete)
        #region disable keys

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

        #endregion

    }
}
