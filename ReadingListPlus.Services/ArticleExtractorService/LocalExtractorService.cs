using System;
using System.Threading.Tasks;
using Boilerpipe.Net.Extractors;

namespace ReadingListPlus.Services.ArticleExtractorService
{
    public class LocalExtractorService : ILocalExtractorService
    {
        private readonly IHttpClientWrapper httpClientWrapper;

        public LocalExtractorService(IHttpClientWrapper httpClientWrapper)
        {
            this.httpClientWrapper = httpClientWrapper ?? throw new ArgumentNullException(nameof(httpClientWrapper));
        }

        public async Task<string> GetArticleText(string url)
        {
            var uri = new Uri(url);
            var response = await httpClientWrapper.GetAsync(uri);
            var html = await response.Content.ReadAsStringAsync();

            var text = CommonExtractors.ArticleExtractor.GetText(html);

            var formattedText = text.Replace("\n", Environment.NewLine + Environment.NewLine);
            return formattedText;
        }
    }
}