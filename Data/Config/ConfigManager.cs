using SteamKit2.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Data.Config
{
    public class ConfigManager
    {
        private readonly string _cfgPath = @"C:\Users\obvin\Desktop\MyPanel\Data\Config\config.json";
        private static readonly object _lock = new object();
        private static ConfigManager _instance;

        public AppConfig Config { get; private set; }
        private ConfigManager()
        {
            Load();
        }
        public static ConfigManager Instance
        {
            get 
            { 
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if(_instance == null)
                            _instance = new ConfigManager();
                    }
                }
                return _instance;
            }
        }
        private void Load()
        {
            if (!File.Exists(_cfgPath))
            {
                Config = new AppConfig();
                Save();
                return;
            }
            string json = File.ReadAllText(_cfgPath);
            Config = JsonConvert.DeserializeObject<AppConfig>(json) ?? new AppConfig();
        }
        private void Save()
        {
            lock (_lock)
            {
                string json = JsonConvert.SerializeObject(Config, Formatting.Indented);
                File.WriteAllText(_cfgPath, json);
            }
        }
        
        public void SetWindowSize(int width, int height)
        {
            Config.SizeOf.WindowWidth = width;
            Config.SizeOf.WindowHeight = height;
            Save();
        }
        public void SetAppPath(string path)
        {
            Config.Paths.AppPath = path;
            Save();
        }
        public void SetSandboxiePath(string path)
        {
            Config.Paths.SandboxiePath = path;
            Save();
        }
        public void SetIniPath(string path)
        {
            Config.Paths.IniPath = path;
            Save();
        }
    }
}
