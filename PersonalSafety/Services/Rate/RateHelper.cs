using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Services.Rate
{
    public static class RateHelper
    {
        public static void UpdateUserRate(ref Rate rate, float newRate)
        {
            rate.RateAverage = ((rate.RateAverage * rate.RateCount) + newRate) / (rate.RateCount + 1);
            rate.RateCount += 1;
        }
    }

    public class Rate
    {
        public int RateCount { get; set; }
        public float RateAverage { get; set; }
    }
}
