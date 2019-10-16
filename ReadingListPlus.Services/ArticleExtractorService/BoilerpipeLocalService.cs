using System;
using System.Threading.Tasks;
using Boilerpipe.Net.Extractors;
using HtmlAgilityPack;

namespace ReadingListPlus.Services.ArticleExtractorService
{
    public class BoilerpipeLocalService : ILocalExtractorService
    {
        private readonly IHttpClientWrapper httpClientWrapper;

        public BoilerpipeLocalService(IHttpClientWrapper httpClientWrapper)
        {
            this.httpClientWrapper = httpClientWrapper ?? throw new ArgumentNullException(nameof(httpClientWrapper));
        }

        public async Task<(string text, string title)> GetTextAndTitleAsync(string url)
        {
            var uri = new Uri(url);
            var response = await httpClientWrapper.GetAsync(uri);
            var html = await response.Content.ReadAsStringAsync();

            var text = CommonExtractors.ArticleExtractor.GetText(html);
            var formattedText = text.Replace("\n", Environment.NewLine + Environment.NewLine);

            var doc = new HtmlDocument();
            doc.LoadHtml(html);
            var title = doc.DocumentNode.SelectSingleNode("//title").InnerText;

            return (formattedText, title);
        }
    }
}
