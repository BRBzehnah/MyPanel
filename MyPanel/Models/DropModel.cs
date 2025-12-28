using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPanel.Models
{
    public class DropModel
    {
        public int Id { get; set; }

        public int BotId { get; set; } 

        public DateOnly DropDate { get; set; }

        public bool IsSended { get; set; }


        public DropModel() { }
        public DropModel(int botId, bool isSended, DateOnly dropDate) 
        {
            BotId = botId;
            IsSended = isSended;
            DropDate = dropDate;
        }

    }
}
