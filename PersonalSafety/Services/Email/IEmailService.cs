using PersonalSafety.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PersonalSafety.Services
{
    public interface IEmailService
    {
        Task<List<string>> SendConfirmMailAsync(string email);
    }
}
