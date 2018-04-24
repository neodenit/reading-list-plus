using System.Threading.Tasks;

namespace ReadingListPlus.Web.Services.ArticleExtractor
{
    interface IArticleExtractor
    {
        Task<string> GetArticleText(string url);
    }
}
