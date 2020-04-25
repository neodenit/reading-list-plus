using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ReadingListPlus.Services.ArticleExtractorService
{
    public class BoilerpipeRemoteService : IRemoteExtractorService
    {
        private readonly HttpClient httpClient;

        private readonly Dictionary<string, string> replacements = new Dictionary<string, string>
        {
            { "\n", Environment.NewLine + Environment.NewLine },
            { " ,", "," },
            { " .", "." },
            { " !", "!" },
            { " ?", "?" }
        };

        public BoilerpipeRemoteService(IHttpClientFactory httpClientFactory)
        {
            httpClient = httpClientFactory is null ? throw new ArgumentNullException(nameof(httpClientFactory)) : httpClientFactory.CreateClient();
        }

        public async Task<(string text, string title)> GetTextAndTitleAsync(string url)
        {
            var urlParameter = Uri.EscapeDataString(url);
            var fullUrl = $"https://boilerpipe-web.appspot.com/extract?url={urlParameter}&output=json";

            var uri = new Uri(fullUrl);
            var response = await httpClient.GetAsync(uri);
            var jsonString = await response.Content.ReadAsStringAsync();

            var json = JsonConvert.DeserializeAnonymousType(jsonString, new
            {
                response = new
                {
                    title = string.Empty,
                    content = string.Empty
                }
            });

            var title = json.response.title;
            var text = json.response.content;

            var formattedText = replacements.Aggregate(text, (s, r) =>
                s.Replace(r.Key, r.Value));

            return (formattedText, title);
        }
    }
}
