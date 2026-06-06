using Data;
using Data.Models;
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
        public BotServices(ApplicationDbContext db)
        {
            _db = db;
        }

        public Result Create(BotModel model)
        {
            if (model == null)
                return Result.Failure(new Error(ErrorType.ModelIsNull, "Некорректная модель 'bot'"));

            var newBot = new Bot(model);
            _db.Bots.Add(newBot);
            _db.SaveChanges();
            return Result.Success();
        }
        public BotModel Create(string email, string login, string password)
        {
            if (email != null && login != null && password != null)
            {
                var newBot = new Bot(email, login, password);
                _db.Bots.Add(newBot);
                _db.SaveChanges();
                return newBot.ToDto();
            }
            return null;
        }
        public BotModel Read(int id)
        {
            var bot = _db.Bots.FirstOrDefault(b => b.Id == id);
            if (bot != null)
                return bot.ToDto();
            return null;
        }
        public BotModel Read(string login)
        {
            var bot = _db.Bots.FirstOrDefault(b => b.Login == login);
            if (bot != null)
                return bot.ToDto();
            return null;
        }
        public Result Update(BotModel model, int id)
        {
            var botForUpdate = _db.Bots.FirstOrDefault(b => b.Id == id);
            if (botForUpdate == null)
                return Result.Failure(new Error(ErrorType.IncorrectId, "Некорректный 'botId'"));

            botForUpdate.Email = model.Email;
            botForUpdate.Login = model.Login;
            botForUpdate.Password = model.Password;
            botForUpdate.MaFile = model.MaFile;
            botForUpdate.TradeLink = model.TradeLink;
            botForUpdate.IsFarmed = model.IsFarmed;
            botForUpdate.IsActiveTm = model.IsActiveTm;
            botForUpdate.IsPrime = model.IsPrime;
            botForUpdate.RestoreCode = model.RestoreCode;
            botForUpdate.Rank = model.Rank;

            _db.Bots.Update(botForUpdate);
            _db.SaveChanges();
            return Result.Success();
        }
        public Result Delete(int id)
        {
            var bot = _db.Bots.FirstOrDefault(b => b.Id == id);
            if (bot == null)
                return Result.Failure(new Error(ErrorType.IncorrectId, "Некорректный 'botId'"));

            _db.Bots.Remove(bot);
            _db.SaveChanges();
            return Result.Success();
        }
    }
}
