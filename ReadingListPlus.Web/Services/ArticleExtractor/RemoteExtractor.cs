using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ReadingListPlus.Web.Services.ArticleExtractor
{
    public class RemoteExtractor : IArticleExtractor
    {
        public async Task<string> GetArticleText(string url)
        {
            var urlParameter = Uri.EscapeDataString(url);
            var fullUrl = $"https://boilerpipe-web.appspot.com/extract?url={urlParameter}&output=text";

            var request = WebRequest.Create(fullUrl);
            var response = await request.GetResponseAsync();
            var responseStream = response.GetResponseStream();
            var streamReader = new StreamReader(responseStream);

            var text = await streamReader.ReadToEndAsync();

            return text;
        }
    }
}