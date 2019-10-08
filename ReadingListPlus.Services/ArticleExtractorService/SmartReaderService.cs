using System.Threading.Tasks;
using SmartReader;

namespace ReadingListPlus.Services.ArticleExtractorService
{
    public class SmartReaderService : IArticleExtractorService
    {
        public async Task<(string text, string title)> GetTextAndTitleAsync(string url)
        {
            var reader = new Reader(url);
            var article = await reader.GetArticleAsync();

            if (article.IsReadable)
            {
                return (article.TextContent.Trim(), article.Title);
            }
            else
            {
                return (url, string.Empty);
            }
        }
    }
}
