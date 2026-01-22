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

namespace MyPanel.Bots
{
    public class BotWorker
    {
        private readonly DbContext _db;
        private readonly BotServices _botServices;
        private readonly int _botId;
        public BotWorker() { }
        public BotWorker(DbContext db, BotServices botServices, int botId) 
        {
            _botServices = botServices;
            _botId = botId;
            _db = db;
        }

        public void Authorize()
        {
            string login = _botServices.Read(_botId).Login;
            string password = _botServices.Read(_botId).Password;

        }
    }
}
