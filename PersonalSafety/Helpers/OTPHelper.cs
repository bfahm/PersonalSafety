using OtpNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalSafety.Helpers
{
    public class OTPHelper
    {
        public static Totp GenerateOTP(string secretString)
        {
            byte[] secretBytes = Encoding.ASCII.GetBytes(secretString);
            var totp = new Totp(secretBytes, 120);
            return totp;
        }
    }
}
