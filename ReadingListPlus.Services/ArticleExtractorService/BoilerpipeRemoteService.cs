using System;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ReadingListPlus.Services.ArticleExtractorService
{
    public class BoilerpipeRemoteService : IRemoteExtractorService
    {
        private readonly IHttpClientWrapper httpClientWrapper;

        public BoilerpipeRemoteService(IHttpClientWrapper httpClientWrapper)
        {
            this.httpClientWrapper = httpClientWrapper ?? throw new ArgumentNullException(nameof(httpClientWrapper));
        }

        public async Task<(string text, string title)> GetTextAndTitleAsync(string url)
        {
            var urlParameter = Uri.EscapeDataString(url);
            var fullUrl = $"https://boilerpipe-web.appspot.com/extract?url={urlParameter}&output=json";

            var uri = new Uri(fullUrl);
            var response = await httpClientWrapper.GetAsync(uri);
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
            var formattedText = text.Replace("\n", Environment.NewLine + Environment.NewLine);

            return (formattedText, title);
        }
    }
}
