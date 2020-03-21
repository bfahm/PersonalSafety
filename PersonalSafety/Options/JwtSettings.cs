using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Options
{
    public class JwtSettings
    {
        public string Secret { get; set; }
        public int JwtExpireInHours { get; set; }
    }
}
