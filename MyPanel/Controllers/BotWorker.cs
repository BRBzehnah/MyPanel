using MyPanel.Services;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using MyPanel.Models;
using System.Windows;

namespace MyPanel.Controllers
{
    public class BotWorker
    {
        private readonly BotModel _bot;
        private readonly IntPtr _window;
        public BotWorker() { }
        public BotWorker(BotModel bot, IntPtr window) 
        {
            _bot = bot;
            _window = window;
        }

        public void Authorize()
        {

        }


    }
}
