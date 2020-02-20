using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Helpers
{
    public class APIResult<T>
    {
        public int Status { get; set; }
        public T Result { get; set; }
        public string Message { get; set; }
    }
}
