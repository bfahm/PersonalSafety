using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using PersonalSafety.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public class DistributionRepository : BaseRepository<Distribution>, IDistributionRepository
    {
        private readonly AppDbContext context;

        public DistributionRepository(AppDbContext context) : base(context)
        {
            this.context = context;
        }

        public void AddWithIdentityInsert(List<Distribution> distributions)
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                foreach(var distribution in distributions)
                {
                    context.Distributions.Add(distribution);
                }
                
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Distributions ON;");
                context.SaveChanges();
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Distributions OFF");
                transaction.Commit();
            }
        }

        public IEnumerable<Distribution> GetCities()
        {
            return context.Distributions.Where(d => d.Type == (int)DistributionTypesEnum.City);
        }

        public Distribution GetCityByName(string name)
        {
            return context.Distributions.SingleOrDefault(d => d.Value == name);
        }

        public List<Distribution> GetGrantedDistribution(int distributionId, int distributionType)
        {
            var result = new List<Distribution>();

            var parentDistribution = context.Distributions.Find(distributionId);

            if(parentDistribution != null)
            {
                var childrenQueue = new Queue<Distribution>();
                childrenQueue.Enqueue(parentDistribution);

                while (childrenQueue.Count > 0)
                {
                    var next = childrenQueue.Dequeue();
                    if (!IsDistributionLeaf(next.Id))
                    {
                        var children = GetChildDistributions(next.Id);
                        foreach (var child in children)
                        {
                            childrenQueue.Enqueue(child);
                        }
                    }
                    else
                    {
                        result.Add(next);
                    }
                }
            }

            return result.Where(d=>d.Type == distributionType).ToList();
        }

        public bool IsCity(int distributionId)
        {
            return context.Distributions.Find(distributionId).Type == (int)DistributionTypesEnum.City;
        }

        private List<Distribution> GetChildDistributions(int distributionId)
        {
            return context.Distributions.Where(d => d.ParentId == distributionId).ToList();
        }

        private bool IsDistributionLeaf(int distributionId)
        {
            return !context.Distributions.Any(d => d.ParentId == distributionId);
        }
    }
}
