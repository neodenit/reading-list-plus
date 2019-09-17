using System;
using System.Threading.Tasks;

namespace ReadingListPlus.Services.ArticleExtractorService
{
    public class RemoteExtractorService : IRemoteExtractorService
    {
        private readonly IHttpClientWrapper httpClientWrapper;

        public RemoteExtractorService(IHttpClientWrapper httpClientWrapper)
        {
            this.httpClientWrapper = httpClientWrapper ?? throw new ArgumentNullException(nameof(httpClientWrapper));
        }

        public async Task<string> GetArticleText(string url)
        {
            var urlParameter = Uri.EscapeDataString(url);
            var fullUrl = $"https://boilerpipe-web.appspot.com/extract?url={urlParameter}&output=text";

            var uri = new Uri(fullUrl);
            var response = await httpClientWrapper.GetAsync(uri);
            var text = await response.Content.ReadAsStringAsync();

            var formattedText = text.Replace("\n", Environment.NewLine + Environment.NewLine);
            return formattedText;
        }
    }
}