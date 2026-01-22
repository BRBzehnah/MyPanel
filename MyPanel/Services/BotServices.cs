using MyPanel.Data;
using MyPanel.Data.Models;
using MyPanel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace MyPanel.Services
{
    public class BotServices : ICommonService<BotModel>
    {
        private readonly ApplicationDbContext _db;
        public BotServices (ApplicationDbContext db)
        {
            _db = db;
        }

        public bool Create(BotModel model)
        {
            if (model != null)
            {
                var newBot = new Bot(model.Email, model.Login, model.Password, model.RestoreCode, model.MaFilePath);
                _db.Bots.Add(newBot);
                _db.SaveChanges();
                return true;
            }
            return false;
        }
        public BotModel Read(int id)
        {
            var bot = _db.Bots.FirstOrDefault(b => b.Id == id);
            if (bot != null)
                return bot.ToDto();
            return null;
        }
        public bool Update(BotModel model, int id)
        {
            var botForUpdate = _db.Bots.FirstOrDefault(b =>b.Id == id);
            if (botForUpdate != null)
            {
                botForUpdate.Email = model.Email;
                botForUpdate.Login = model.Login;
                botForUpdate.Password = model.Password;
                botForUpdate.TradeLink = model.TradeLink;
                botForUpdate.IsFarmed = model.IsFarmed;
                botForUpdate.IsActiveTm = model.IsActiveTm;
                botForUpdate.IsPrime = model.IsPrime;
                botForUpdate.RestoreCode = model.RestoreCode;
                botForUpdate.Role = model.Role;
                botForUpdate.Rank = model.Rank;

                _db.Bots.Update(botForUpdate);
                _db.SaveChanges();
                return true;
            }
            return false;
        }
        public bool Delete(int id)
        {
            var bot = _db.Bots.FirstOrDefault(b => b.Id == id);
            if (bot != null)
            {
                _db.Bots.Remove(bot);
                _db.SaveChanges();
                return true;
            }
            return false;
        }
    }
}
