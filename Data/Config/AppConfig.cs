using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Data.Config
{
    public class AppConfig
    {
        public Paths Paths { get; set; } = new Paths();
        public SizeOf SizeOf { get; set; } = new SizeOf();
        public List<SanboxConfig> SanboxConfig { get; set; } = new List<SanboxConfig>();
    }
    public class Paths
    {
        public string AppPath { get; set; } = "";
        public string SandboxiePath { get; set; } = "";
        public string IniPath { get; set; } = "";
        public string AgentExePath { get; set; } = "";
    }
    public class SizeOf
    {
        [DllImport("user32.dll")]       //Импорт для получения информации о разрешении
        static extern int GetSystemMetrics(int nIndex);

        public int WindowWidth { get; set; }
        public int WindowHeight { get; set; }
        public int MonitorWidth { get; set; } = GetSystemMetrics(0);
        public int MonitorHeight { get; set; } = GetSystemMetrics(1);
    }
    public class SanboxConfig
    {
        public string Key { get; set; } = "";
        public string Value { get; set; } = "";
    }
}
