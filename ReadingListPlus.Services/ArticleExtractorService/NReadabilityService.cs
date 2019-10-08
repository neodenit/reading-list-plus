using System.Threading.Tasks;
using HtmlAgilityPack;
using NReadability;

namespace ReadingListPlus.Services.ArticleExtractorService
{
    public class NReadabilityService : IArticleExtractorService
    {
        public async Task<(string text, string title)> GetTextAndTitleAsync(string url)
        {
            string GetTextFromHtml(string html)
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(html);

                var result = doc.DocumentNode.SelectSingleNode("//div[@id='readInner']").InnerText;
                return result;
            }

            var transcoder = new NReadabilityWebTranscoder();

            var article = await Task.FromResult(transcoder.Transcode(new WebTranscodingInput(url)));

            var text = article.ContentExtracted ? GetTextFromHtml(article.ExtractedContent) : url;
            var title = article.TitleExtracted ? article.ExtractedTitle : string.Empty;

            return (text, title);
        }
    }
}
