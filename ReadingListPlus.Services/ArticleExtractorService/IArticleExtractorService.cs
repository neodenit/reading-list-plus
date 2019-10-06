using System.Threading.Tasks;

namespace ReadingListPlus.Services.ArticleExtractorService
{
    public interface IArticleExtractorService
    {
        Task<string> GetArticleTextAsync(string url);
    }
}
