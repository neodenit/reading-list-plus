using System.Threading.Tasks;

namespace ReadingListPlus.Services.ArticleExtractorService
{
    public interface IArticleExtractorService
    {
        Task<(string text, string title)> GetTextAndTitleAsync(string url);
    }
}
