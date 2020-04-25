using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Models
{
    public interface IDistributionRepository : IBaseRepository<Distribution>
    {
        List<Distribution> GetGrantedDistribution(int distributionId, int distributionType);
    }
}
