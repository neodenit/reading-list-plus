﻿using System.IO;
using System.Net;
using System.Threading.Tasks;
using Boilerpipe.Net.Extractors;

namespace ReadingListPlus.Web.Services.ArticleExtractor
{
    public class LocalExtractor : IArticleExtractor
    {
        public async Task<string> GetArticleText(string url)
        {
            var request = WebRequest.Create(url);
            var response = await request.GetResponseAsync();
            var responseStream = response.GetResponseStream();
            var streamReader = new StreamReader(responseStream);

            var html = await streamReader.ReadToEndAsync();

            var text = CommonExtractors.ArticleExtractor.GetText(html);

            return text;
        }
    }
}