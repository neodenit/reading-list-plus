using System;
using System.Threading.Tasks;

namespace ReadingListPlus.Web.Services.ArticleExtractor
{
    public class CombinedExtractor : IArticleExtractor
    {
        private IArticleExtractor remoteExtractor = new RemoteExtractor();
        private IArticleExtractor localExtractor = new LocalExtractor();

        public async Task<string> GetArticleText(string url)
        {
            try
            {
                return await remoteExtractor.GetArticleText(url);
            }
            catch (Exception)
            {
                return await localExtractor.GetArticleText(url);
            }
        }
    }
}