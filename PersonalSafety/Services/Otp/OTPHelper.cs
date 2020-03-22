using OtpNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PersonalSafety.Services.Otp
{
    public class OTPHelper
    {
        public static int otpSize = 4;
        public static int validFor = 120;
        public static Totp GenerateOTP(string secretString)
        {
            byte[] secretBytes = Encoding.ASCII.GetBytes(secretString);
            var totp = new Totp(secretBytes, step: validFor, totpSize: otpSize);
            return totp;
        }
    }
}
