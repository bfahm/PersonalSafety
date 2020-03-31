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
            try
            {
                return context.Set<T>().Find(Id);
            }catch
            {
                return context.Set<T>().Find(int.Parse(Id));
            }
            
        }

        public void RemoveById(string id)
        {
            T item = GetById(id);
            context.Set<T>().Remove(item);
        }

        public T Update(T item)
        {
            context.Set<T>().Update(item);
            return item;
        }

        public int Save()
        {
            return context.SaveChanges();
        }
    }
}
