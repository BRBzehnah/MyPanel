using MyPanel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPanel.Data.Models
{
    public class Drop
    {
        public int Id { get; set; }

        public int BotId { get; set; }  /*Айди бота*/

        public DateOnly DropDate { get; set; }  /*Дата дропа*/

        public bool IsSended { get; set; }  /*Указатель отправлен ли дроп*/


        public Drop() { }
        public Drop(DropModel model) 
        {
            BotId = model.BotId;
            IsSended = model.IsSended;
            DropDate = model.DropDate;
        }
        public Drop(int botId, bool isSended = false) 
        {
            BotId = botId;
            IsSended = isSended;
            DropDate = DateOnly.FromDateTime(DateTime.Today);
        }

        public DropModel ToDto()
        {
            return new DropModel()
            {
                BotId = this.BotId,
                DropDate = this.DropDate,
                IsSended = this.IsSended,
            };
        }
    }
}
