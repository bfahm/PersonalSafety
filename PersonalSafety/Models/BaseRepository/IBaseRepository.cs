using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public interface IBaseRepository<T>
    {
        T GetById(int Id);
        IEnumerable<T> GetAll();
        T Add(T item);
        T Update(T item);
        void Remove(int Id);
        void Save();
    }
}
