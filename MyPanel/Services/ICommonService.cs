using Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPanel.Services
{
    public interface ICommonService<T>
    {
        public Result Create(T model);
        public T Read(int id);
        public Result Update(T model, int id);
        public Result Delete(int id);
    }
}
