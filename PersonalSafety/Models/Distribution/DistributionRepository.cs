using Microsoft.CodeAnalysis;
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
