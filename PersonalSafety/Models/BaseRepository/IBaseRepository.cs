using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public interface IBaseRepository<T>
    {
        T GetById(string Id);
        IEnumerable<T> GetAll();
        bool Add(T item);
        T Update(T item);
        //void Remove(string Id);
        int Save();
    }
}
