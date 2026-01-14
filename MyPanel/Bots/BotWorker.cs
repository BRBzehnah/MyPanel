using MyPanel.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPanel.Bots
{
    public class BotWorker
    {
        private readonly BotServices _botServices;
        private readonly int _botId;
        public BotWorker() { }
        public BotWorker(BotServices botServices, int botId) 
        {
            _botServices = botServices;
            _botId = botId;
        }

        public Tuple GetAuthorizationData()
        {
            string login = _botServices.Read(_botId).Login;
            string password = _botServices.Read(_botId).Password;

            return new Tuple<string>.(login, password);
        }
    }
}
