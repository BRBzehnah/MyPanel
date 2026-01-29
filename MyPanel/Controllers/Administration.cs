using Microsoft.EntityFrameworkCore;
using MyPanel.APIs.SandboxieAPI;
using MyPanel.Data;
using MyPanel.Models;
using MyPanel.Services;
using SteamKit2.WebUI.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPanel.Controllers
{
    public class Administration
    {
        private readonly ApplicationDbContext _db;
        private readonly BotServices _bs;
        private readonly SandboxController _sbc;
        private readonly SteamServices _ss;
        public Administration()
        {
            _db = new ApplicationDbContext();
            _bs = new BotServices(_db);
            _ss = new SteamServices();
        }

        public async Task<bool> Registration(string email, string login, string password, string phoneNumber)
        {

            var bot = new BotModel(email, login, password);

            try
            {
                bot.MaFile = await _ss.CreateMaFile(bot.Login, bot.Password, phoneNumber);
            }
            catch 
            {
                return false;
            }

            if (bot.MaFile is not null)
            {
                var rCode = _ss.GetRestoreCode(bot.MaFile);
                if (rCode is not null)
                {
                    bot.RestoreCode = rCode;
                    _bs.Create(bot);
                    return true;
                }
            }
            return false;
        }
    }
}
