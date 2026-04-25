using MyPanel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MyPanel.Data.Models
{
    public class Bot
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string EmailPassword { get; set; }

        public string Login { get; set; }

        public string Password { get; set; }

        public string MaFile { get; set; }

        public string RestoreCode { get; set; }   /*Код восстановления гварда*/

        public string? TradeLink { get; set; }  /*Ссылка на обмен*/

        public bool IsActiveTm { get; set; }    /*Указатель доступности ТМ*/

        public bool IsPrime { get; set; }       /*Указатель прайма*/

        public bool IsFarmed { get; set; }      /*Указатель получен ли дроп*/

        public List<Drop> Drop { get; set; } = new List<Drop>();    /*Список дропа*/

        public Rank Rank { get; set; }          /*Указатель ранга*/


        public Bot() { }
        public Bot(BotModel model) 
        { 
            Email = model.Email;
            EmailPassword = model.EmailPassword;
            Login = model.Login;
            Password = model.Password;
            RestoreCode = model.RestoreCode;
            TradeLink = model.TradeLink;
            IsActiveTm = model.IsActiveTm;
            IsPrime = model.IsPrime;
            IsFarmed = model.IsFarmed;
            Rank = model.Rank;
        }

        public Bot(string email, string login, string password, string emailPassword = null, string restoreCode = null, string maFile = null, bool isActiveTm = false, bool isPrime = false, bool isFarmed = false, Rank rank = Rank.None, string tradeLink = null)
        {
            Email = email;
            Login = login;
            Password = password;
            EmailPassword = emailPassword;
            MaFile = maFile;
            RestoreCode = restoreCode;
            TradeLink = tradeLink;
            IsActiveTm = isActiveTm;
            IsPrime = isPrime;
            IsFarmed = isFarmed;
            Rank = rank;
        }

        public BotModel ToDto()
        {
            return new BotModel
            {
                Email = this.Email,
                EmailPassword = this.EmailPassword,
                Login = this.Login,
                Password = this.Password,
                MaFile = this.MaFile,
                RestoreCode = this.RestoreCode,
                IsActiveTm = this.IsActiveTm,
                IsPrime = this.IsPrime,
                IsFarmed = this.IsFarmed,
                Rank = this.Rank
            };
        }
    }
}
