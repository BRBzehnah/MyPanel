using MyPanel.Data.Models;
using MyPanel.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPanel.Models
{
    public class BotModel
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public string MaFile { get; set; }

        public string RestoreCode { get; set; }  

        public string? TradeLink { get; set; } 

        public bool IsActiveTm { get; set; }    

        public bool IsPrime { get; set; }     

        public bool IsFarmed { get; set; } 

        public Role Role { get; set; }

        public Rank Rank { get; set; }


        public BotModel() { }

        public BotModel(string email, string login, string password, string restoreCode = null, string maFile = null, bool isActiveTm = false, bool isPrime = false, bool isFarmed = false, Role role = Role.Worker, Rank rank = Rank.None, string tradeLink = null)
        {
            Email = email;
            Login = login;
            Password = password;
            MaFile = maFile;
            RestoreCode = restoreCode;
            TradeLink = tradeLink;
            IsActiveTm = isActiveTm;
            IsPrime = isPrime;
            IsFarmed = isFarmed;
            Role = role;
            Rank = rank;
        }
    }
}
