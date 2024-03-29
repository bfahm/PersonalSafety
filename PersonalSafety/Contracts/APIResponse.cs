﻿using Newtonsoft.Json;
using PersonalSafety.Contracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Contracts
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

        public void WrapResponseData(APIResponseData data)
        {
            Status = data.Status;
            HasErrors = data.HasErrors;
            Messages.AddRange(data.Messages);
        }
    }

    public class APIResponseData
    {
        public int Status { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
        public bool HasErrors { get; set; }

        public APIResponseData(int status, List<string> messages)
        {
            Status = status;
            Messages = messages;
            HasErrors = status != (int)APIResponseCodesEnum.Ok;
        }
    }
}
