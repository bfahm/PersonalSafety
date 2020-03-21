using Newtonsoft.Json;
using PersonalSafety.Contracts;
using PersonalSafety.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PersonalSafety.Services
{
    public class FacebookAuthService : IFacebookAuthService
    {
        private const string TokenValidationUrl = "https://graph.facebook.com/debug_token?input_token={0}&access_token={1}|{2}";
        private const string UserInfoUrl = "https://graph.facebook.com/me?fields=first_name,last_name,picture,email&access_token={0}";
        private readonly FacebookAuthSettings _facebookAuthSettings;
        private readonly IHttpClientFactory _httpClientFactory;

        public FacebookAuthService(FacebookAuthSettings facebookAuthSettings, IHttpClientFactory httpClientFactory)
        {
            _facebookAuthSettings = facebookAuthSettings;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<FacebookTokenValidationResult> ValidateAccessTokenAsync(string accessToken)
        {
            string formattedUrl = string.Format(TokenValidationUrl, accessToken, _facebookAuthSettings.AppId, _facebookAuthSettings.AppSecret);

            HttpResponseMessage result = await _httpClientFactory.CreateClient().GetAsync(formattedUrl);

            //The above call returns a bad request whenever there is a bad access token.
            if (result.IsSuccessStatusCode)
            {
                string responseAsString = await result.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<FacebookTokenValidationResult>(responseAsString);
            }

            //Escape null pointers while checking for data (default for IsValid will be false)..
            return new FacebookTokenValidationResult
            {
                Data = new FacebookTokenValidationData()
            };
        }

        public async Task<FacebookUserInfoResult> GetUserInfoAsync(string accessToken)
        {
            string formattedUrl = string.Format(UserInfoUrl, accessToken);

            HttpResponseMessage result = await _httpClientFactory.CreateClient().GetAsync(formattedUrl);
            
            //There shouldn't be a non 200OK http response as Validation is required to run first before running this method.
            result.EnsureSuccessStatusCode();

            string responseAsString = await result.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<FacebookUserInfoResult>(responseAsString);
        }
    }
}
