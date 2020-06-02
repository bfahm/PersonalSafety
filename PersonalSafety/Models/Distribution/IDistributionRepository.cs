using PersonalSafety.Models.ViewModels.AdminVM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public interface IDistributionRepository : IBaseRepository<Distribution>
    {
        // Get Functions
        int GetNumberOfParents(int startingNode);
        Distribution GetRootDistribution();
        Distribution GetCityByName(string name);
        IEnumerable<Distribution> GetCities();
        List<Distribution> GetGrantedCities(int distributionId);
        DistributionTreeViewModel GetDistributionTree(int startingNode, bool recursive);
        
        // General Functions
        void AddWithIdentityInsert(List<Distribution> distributions);
        bool IsCity(int distributionId);
        bool DoesNodeExist(int nodeId);

    }
}
