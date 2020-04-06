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
    public class GithubUpdateService : IGithubUpdateService
    {
        private const string Url = "https://api.github.com/repos/bfahm/PersonalSafety/releases/latest";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppSettings _appSettings;

        public GithubUpdateService(IHttpClientFactory httpClientFactory, AppSettings appSettings)
        {
            _httpClientFactory = httpClientFactory;
            _appSettings = appSettings;
        }

        public async Task<bool> IsApplicationUpToDate()
        {
            var request = new HttpRequestMessage(HttpMethod.Get, Url);
            request.Headers.Add("User-Agent", "NetCore31");

            var client = _httpClientFactory.CreateClient();

            var result = await client.SendAsync(request);

            //The above call returns a bad request whenever there is a bad access token.
            if (result.IsSuccessStatusCode)
            {
                string responseAsString = await result.Content.ReadAsStringAsync();
                GithubResponse responseAsObject =  JsonConvert.DeserializeObject<GithubResponse>(responseAsString);
                if (responseAsObject.TagName == _appSettings.AppVersion)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
