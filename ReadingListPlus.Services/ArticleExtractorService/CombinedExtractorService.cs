using System;
using System.Threading.Tasks;

namespace ReadingListPlus.Services.ArticleExtractorService
{
    public class CombinedExtractorService : IArticleExtractorService
    {
        private readonly IRemoteExtractorService remoteExtractor;
        private readonly ILocalExtractorService localExtractor;

        public CombinedExtractorService(IRemoteExtractorService remoteExtractor, ILocalExtractorService localExtractor)
        {
            this.remoteExtractor = remoteExtractor ?? throw new ArgumentNullException(nameof(remoteExtractor));
            this.localExtractor = localExtractor ?? throw new ArgumentNullException(nameof(localExtractor));
        }

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