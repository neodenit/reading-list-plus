using System;
using System.Threading.Tasks;

namespace ReadingListPlus.Services.ArticleExtractorService
{
    public class CombinedExtractor : IArticleExtractorService
    {
        private IArticleExtractorService remoteExtractor = new RemoteExtractor();
        private IArticleExtractorService localExtractor = new LocalExtractor();

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