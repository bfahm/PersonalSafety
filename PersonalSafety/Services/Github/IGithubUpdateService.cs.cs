using PersonalSafety.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Services
{
    public interface IGithubUpdateService
    {
        Task<bool> IsApplicationUpToDate();
    }
}
