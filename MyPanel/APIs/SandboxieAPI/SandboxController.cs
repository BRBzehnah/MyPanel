using MyPanel.Controllers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MyPanel.APIs.SandboxieAPI
{
    public class SandboxController
    {
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]     //Импорт для взаимодействия с конфиг файлом песочниц
        private static extern bool WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        [DllImport("user32.dll", SetLastError = true)]      //Импорт для взаимодейтсвия с окнами
        internal static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll")]       //Импорт для получения информации о разрешении
        static extern int GetSystemMetrics(int nIndex);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]       //Импорт для поиска окна
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]       //Импорт для определения заголовка окна
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]       //Импорт для получения всех окон
        static extern bool EnumWindows(EnumWindowsProc lpEnumFunc, IntPtr lParam);

        public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);


        private readonly string _iniPath = ConfigManager.Instance.Config.Path.IniPath;
        private readonly string _sbiePath = ConfigManager.Instance.Config.Path.SandboxiePath;
        private readonly string _appPath = ConfigManager.Instance.Config.Path.AppPath;
        private readonly string[] _boxesNames;
        private readonly int _wndWidth = ConfigManager.Instance.Config.SizeOf.WindowWidth;
        private readonly int _wndHeight = ConfigManager.Instance.Config.SizeOf.WindowHeight;
        private readonly int _screenWidth = ConfigManager.Instance.Config.SizeOf.MonitorWidth;
        private readonly int _screenHeight = ConfigManager.Instance.Config.SizeOf.MonitorHeight;

        public SandboxController(int countOfBoxes)
        {
            _sbiePath = @"C:\Program Files\Sandboxie-Plus\Start.exe";
            _iniPath = @"C:\Windows\SbieCtrl.ini";
            _appPath = @"C:\Users\obvin\Desktop\Counter-Strike 2.url";
            _wndWidth = 808;
            _wndHeight = 600;

            if (!File.Exists(_sbiePath))
                throw new FileNotFoundException("Sandboxie Start.exe не найден!");
            if (!IsUserAdministrator())
                MessageBox.Show("ВНИМАНИЕ: Запустите программу от имени администратора");

            _boxesNames = CreateBoxes(countOfBoxes);
            GetConfig();
        }
        private bool IsUserAdministrator()
        {
            using (var identity = System.Security.Principal.WindowsIdentity.GetCurrent())
            {
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
        }
        private string[] CreateBoxes(int countOfBoxes)
        {
            string[] boxes = new string[countOfBoxes];

            for (int i = 0; i < countOfBoxes; i++)
            {
                boxes[i] = $"Sandbox{i}";
            }

            return boxes;
        }
        private void GetConfig()
        {
            foreach (var box in _boxesNames)
            {
                WritePrivateProfileString(box, "Enabled", "y", _iniPath);
                WritePrivateProfileString(box, "AutoDelete", "y", _iniPath);
                WritePrivateProfileString(box, "ConfigLevel", "9", _iniPath);
            }
            
            Process.Start(_sbiePath, "/reload").WaitForExit();
        }


        public void GetPlaceForBox()
        {
            int x = 0;
            int y = 0;
            for (int i = 0; i < _boxesNames.Length; i++)
            {
                IntPtr currentWindow = FindWindow(_boxesNames[i]);
                if (currentWindow != IntPtr.Zero)
                {
                    x = i * _wndWidth;
                    if (x > _screenWidth)
                    {
                        y +=_wndHeight;
                        x = 0;
                    }

                    MoveWindow(currentWindow, x, y, _wndWidth, _wndHeight, true);
                }
            } 
        }
        public IntPtr FindWindow(string boxName)
        {
            IntPtr foundHandle = IntPtr.Zero;

            EnumWindows((hWnd, lParam) =>
            {
                StringBuilder sb = new StringBuilder(256);
                GetWindowText(hWnd, sb, sb.Capacity);
                string title = sb.ToString();

                if (title.Contains(boxName))
                {
                    foundHandle = hWnd;
                    return false;
                }
                return true;
            }, IntPtr.Zero);

            return foundHandle;
        }

        public async Task RunBoxes()
        {
            List<Task> tasks = new List<Task>();

            for (int i = 0; i < _boxesNames.Length; i++)
            {
                var currentBox = _boxesNames[i];

                tasks.Add(Task.Run(() =>
                {
                    string args = $"/box:{currentBox} /wait {_appPath}";

                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = _sbiePath,
                        Arguments = args,
                        CreateNoWindow = true,
                        UseShellExecute = false
                    };

                    Process.Start(psi);
                    Task.Delay(5000);
                })
                );
            }

            await Task.WhenAll(tasks);
        }

        //public async Task Run()


        public void Cleanup()
        {
            foreach (var box in _boxesNames)
            {
                // Принудительно завершаем всё и удаляем файлы
                Process.Start(_sbiePath, $"/box:{box} /terminate").WaitForExit();
                Thread.Sleep(1000); // Даем системе секунду освободить файлы
                Process.Start(_sbiePath, $"/box:{box} /destroy").WaitForExit();
            }
        }
    }
}
