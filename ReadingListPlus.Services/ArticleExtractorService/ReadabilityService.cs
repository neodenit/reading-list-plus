using System.Threading.Tasks;
using SmartReader;

namespace ReadingListPlus.Services.ArticleExtractorService
{
    public class ReadabilityService : IArticleExtractorService
    {
        public async Task<string> GetArticleTextAsync(string url)
        {
            var reader = new Reader(url);
            var article = await reader.GetArticleAsync();

            if (article.IsReadable)
            {
                return article.TextContent;
            }
            else
            {
                return url;
            }
        }
    }
}
