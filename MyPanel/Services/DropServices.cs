using Microsoft.EntityFrameworkCore;
using MyPanel.Data;
using MyPanel.Data.Models;
using MyPanel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPanel.Services
{
    public class DropServices : ICommonService<DropModel>
    {
        private readonly ApplicationDbContext _db;
        public DropServices(ApplicationDbContext db)
        {
            _db = db;
        }

        public bool Create(DropModel model)
        {
            if (model != null)
            {
                var drop = new Drop(model.BotId);
                _db.Drops.Add(drop);
                _db.SaveChanges();
                return true;
            }
            return false;
        }
        public DropModel Read(int id)
        {
            var drop = _db.Drops.FirstOrDefault(d => d.Id == id);
            if (drop != null)
                return drop.ToDto();
            return null;
        }
        public bool Update(DropModel model, int id)
        {
            var dropForUdate = _db.Drops.FirstOrDefault(d => d.Id == id);
            if (dropForUdate != null)
            {
                dropForUdate.IsSended = model.IsSended;
                _db.Drops.Update(dropForUdate);
                _db.SaveChanges();
            }
            return false;
        }
        public bool Delete(int id)
        {
            var drop = _db.Drops.FirstOrDefault(d => d.Id == id);
            if (drop != null)
            {
                _db.Drops.Remove(drop);
                _db.SaveChanges();
                return true;
            }
            return false;
        }
    }
}
