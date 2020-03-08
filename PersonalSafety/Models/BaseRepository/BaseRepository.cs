using Microsoft.EntityFrameworkCore;
using PersonalSafety.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public class BaseRepository<T> : IBaseRepository<T> where T : class
    {
        private readonly AppDbContext context;

        public BaseRepository(AppDbContext context)
        {
            this.context = context;
        }

        public bool Add(T item)
        {
            var result = context.Add(item);
            return result.State == EntityState.Added;
        }

        public IEnumerable<T> GetAll()
        {
            return context.Set<T>().AsNoTracking().AsEnumerable<T>();
        }

        public T GetById(string Id)
        {
            return context.Set<T>().Find(Id);
        }

        // Needs testing, commented to take caution.
        //public void Remove(string id)
        //{
        //    T item = GetById(id);
        //    context.Set<T>().Remove(item);
        //    context.SaveChanges();
        //}

        // Needs testing, commented to take caution.
        //public T Update(T item)
        //{
        //    context.Set<T>().Update(item);
        //    context.SaveChangesAsync();
        //    return item;
        //}

        public int Save()
        {
            return context.SaveChanges();
        }
    }
}
