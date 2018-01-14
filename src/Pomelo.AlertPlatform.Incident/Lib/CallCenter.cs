using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Pomelo.AlertPlatform.Incident.Lib;

namespace Pomelo.AlertPlatform.Incident.Lib
{
    public class CallCenter
    {
        public CallCenter(IConfiguration config)
        {
            _config = config;
        }

        private IConfiguration _config;

        public HttpClient client = new HttpClient() { BaseAddress = new Uri("http://alert.pomelo.cloud") };

        public async Task<Guid> TriggerAlertAsync(string text, string type, string to)
        {
            using (var response = await client.PostAsync("/api/message", new FormUrlEncodedContent(new Dictionary<string, string> {
                { "appId", _config["CallCenter:AppId"] },
                { "secret", _config["CallCenter:Secret"] },
                { "type", type },
                { "to", to },
                { "text", text },
                { "retry", "3" },
                { "replay", "3" }
            })))
            {
                var str = await response.Content.ReadAsStringAsync();
                var json = JsonConvert.DeserializeObject<dynamic>(str);
                return Guid.Parse(json.data);
            }
        }
    }
}

namespace Microsoft.Extensions.DependencyInjection
{
    public static class CallCenterExtension
    {
        public static IServiceCollection AddCallCenter(this IServiceCollection self)
        {
            return self.AddSingleton<CallCenter>();
        }
    }
}