using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Helpers
{
    public class APIResponse<T>
    {
        public int Status { get; set; }
        public bool HasErrors { get; set; }
        public T Result { get; set; }
        public List<string> Messages { get; set; } = new List<string>();

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
