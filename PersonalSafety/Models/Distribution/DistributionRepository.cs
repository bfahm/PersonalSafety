using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using PersonalSafety.Contracts.Enums;
using PersonalSafety.Models.ViewModels.AdminVM;
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

        #region Public Get Functions

        /// <summary>
        /// Used to return the single root parent in th database
        /// </summary>
        
        public int GetNumberOfParents(int startingNode)
        {
            var result = 1;
            var node = context.Distributions.Include(d => d.Parent).Single(d => d.Id == startingNode);
            var parent = node.Parent;
            if (parent != null)
            {
                result += GetNumberOfParents(parent.Id);
            }
            return result;
        }

        public Distribution GetRootDistribution()
        {
            return context.Distributions.SingleOrDefault(d => d.ParentId == null);
        }

        public Distribution GetCityByName(string name)
        {
            return context.Distributions.SingleOrDefault(d => d.Value == name);
        }

        public IEnumerable<Distribution> GetCities()
        {
            return context.Distributions.Where(d => d.Type == (int)DistributionTypesEnum.City);
        }

        public List<Distribution> GetGrantedDistributions(int distributionId)
        {
            var result = new List<Distribution>();

            var parentDistribution = context.Distributions.Find(distributionId);

            if (parentDistribution != null)
            {
                var childrenQueue = new Queue<Distribution>();
                childrenQueue.Enqueue(parentDistribution);

                while (childrenQueue.Count > 0)
                {
                    var next = childrenQueue.Dequeue();
                    if (!IsDistributionLeaf(next.Id))
                    {
                        var children = GetDirectChildren(next.Id);
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

            return result;
        }

        public DistributionTreeViewModel GetDistributionTree(int startingNode, bool recursive)
        {
            var root = context.Distributions.Single(d => d.Id == startingNode);
            DistributionTreeViewModel tree = new DistributionTreeViewModel
            {
                Id = root.Id,
                TypeId = root.Type,
                TypeName = ((DistributionTypesEnum)root.Type).ToString(),
                Value = root.Value
            };

            var children = GetDirectChildren(startingNode);
            foreach (var child in children)
            {
                if (recursive)
                {
                    tree.Children.Add(GetDistributionTree(child.Id, true));
                }
                else
                {
                    tree.Children.Add(new DistributionTreeViewModel
                    {
                        Id = child.Id,
                        TypeId = child.Type,
                        TypeName = ((DistributionTypesEnum)child.Type).ToString(),
                        Value = child.Value,
                        Children = null
                    });
                }
            }

            return tree;
        }

        #endregion

        #region Other Public Functions

        public void AddWithIdentityInsert(List<Distribution> distributions)
        {
            using (var transaction = context.Database.BeginTransaction())
            {
                foreach (var distribution in distributions)
                {
                    context.Distributions.Add(distribution);
                }

                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Distributions ON;");
                context.SaveChanges();
                context.Database.ExecuteSqlRaw("SET IDENTITY_INSERT Distributions OFF");
                transaction.Commit();
            }
        }

        public bool IsCity(int distributionId)
        {
            return context.Distributions.Find(distributionId).Type == (int)DistributionTypesEnum.City;
        }

        public bool DoesNodeExist(int nodeId)
        {
            return context.Distributions.Any(d => d.Id == nodeId);
        }

        #endregion

        #region Private Helpers

        private List<Distribution> GetDirectChildren(int parentId)
        {
            return context.Distributions.Where(d => d.ParentId == parentId).ToList();
        }

        private bool IsDistributionLeaf(int distributionId)
        {
            return !context.Distributions.Any(d => d.ParentId == distributionId);
        }

        #endregion
    }
}
