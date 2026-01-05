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
        [DllImport("kernel32", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern bool WritePrivateProfileString(string Section, string Key, string Value, string FilePath);

        private readonly string _iniPath;
        private readonly string _sbiePath;
        private readonly string _appPath;
        private readonly string[] _boxesNames;

        public SandboxController(int countOfBoxes)
        {
            _sbiePath = @"C:\Program Files\Sandboxie-Plus\Start.exe";
            _iniPath = @"C:\Windows\SbieCtrl.ini";
            _appPath = @"C:\Users\obvin\Desktop\Counter-Strike 2.url";

            if (!File.Exists(_sbiePath))
                throw new FileNotFoundException("Sandboxie Start.exe не найден!");
            if (!IsUserAdministrator())
                MessageBox.Show("ВНИМАНИЕ: Запустите программу от имени администратора");

            _boxesNames = SetBoxes(countOfBoxes);
            ConfigureSandbox();
        }
        private bool IsUserAdministrator()
        {
            using (var identity = System.Security.Principal.WindowsIdentity.GetCurrent())
            {
                var principal = new System.Security.Principal.WindowsPrincipal(identity);
                return principal.IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
        }
        private string[] SetBoxes(int countOfBoxes)
        {
            string[] boxes = new string[countOfBoxes];

            for (int i = 0; i < countOfBoxes; i++)
            {
                boxes[i] = $"Sandbox{i}";
            }

            return boxes;
        }
        private void ConfigureSandbox()
        {
            foreach (var box in _boxesNames)
            {
                WritePrivateProfileString(box, "Enabled", "y", _iniPath);
                WritePrivateProfileString(box, "AutoDelete", "y", _iniPath);
                WritePrivateProfileString(box, "ConfigLevel", "9", _iniPath);
            }
            
            Process.Start(_sbiePath, "/reload").WaitForExit();
        }

        public async Task RunIsolatedTasks()
        {
            List<Task> tasks = new List<Task>();

            foreach (var box in _boxesNames)
            { 
                var currentBox = box;
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

                    using (Process p = Process.Start(psi))
                    {
                        p?.WaitForExit();
                    }
                })
                );
            }

            await Task.WhenAll(tasks);
        }
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
