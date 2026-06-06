using Data;
using Data.Models;
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

        public Result Create(DropModel model)
        {
            if(model == null)
                return Result.Failure(new Error(ErrorType.ModelIsNull, "Некорректная модель 'drop'"));

            var drop = new Drop(model.BotId);
            _db.Drops.Add(drop);
            _db.SaveChanges();
            return Result.Success();

        }
        public DropModel Read(int id)
        {
            var drop = _db.Drops.FirstOrDefault(d => d.Id == id);
            if (drop != null)
                return drop.ToDto();
            return null;
        }
        public Result Update(DropModel model, int id)
        {
            var dropForUdate = _db.Drops.FirstOrDefault(d => d.Id == id);
            if(dropForUdate == null)
                return Result.Failure(new Error(ErrorType.IncorrectId, "Некорректный 'dropId'"));

            dropForUdate.IsSended = model.IsSended;
            _db.Drops.Update(dropForUdate);
            _db.SaveChanges();
            return Result.Success();

        }
        public Result Delete(int id)
        {
            var drop = _db.Drops.FirstOrDefault(d => d.Id == id);
            if(drop == null)
                return Result.Failure(new Error(ErrorType.IncorrectId, "Некорректный 'dropId'"));

            _db.Drops.Remove(drop);
            _db.SaveChanges();
            return Result.Success();

        }
    }
}
