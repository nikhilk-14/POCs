using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Prebatching_Automation
{
    public static class Utilities
    {
        public static async Task<HttpResponseMessage> PostApiCall(string url, string input, Dictionary<string, string> headers = null)
        {
            using HttpClient client = new HttpClient();
            if (headers != null && headers.Count > 0)
            {
                foreach (var header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
            var inputBody = new StringContent(input, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, inputBody);
            return response;
        }

        public static async Task<HttpResponseMessage> GetApiCall(string url, Dictionary<string, string> headers = null)
        {
            using HttpClient client = new HttpClient();
            if (headers != null && headers.Count > 0)
            {
                foreach (var header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }
            }
            var response = await client.GetAsync(url);
            return response;
        }

        public static AppInsightsData ReadAppInsightsData(string inputData)
        {
            var data = JsonConvert.DeserializeObject<AppInsightsResponseModel>(inputData);

            var output = new AppInsightsData()
            {
                data = new List<List<AppInsightsItem>>()
            };
            foreach (var item in data.tables[0].rows)
            {
                var row = new List<AppInsightsItem>();
                for (int counter = 0; counter < data.tables[0].columns.Count; counter += 1)
                {
                    var appInsightsItem = new AppInsightsItem()
                    {
                        name = data.tables[0].columns[counter].name.ToString(),
                        value = item[counter],
                        type = data.tables[0].columns[counter].type.ToString()
                    };
                    row.Add(appInsightsItem);
                }
                output.data.Add(row);
            }

            return output;
        }
    }
}
