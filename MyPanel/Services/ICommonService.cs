using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyPanel.Services
{
    public interface ICommonService<T>
    {
        public bool Create(T model);
        public T Read(int id);
        public bool Update(T model, int id);
        public bool Delete(int id);
    }
}
