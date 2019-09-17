using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ReadingListPlus.Services
{
    public class HttpClientWrapper : IHttpClientWrapper
    {

        private readonly HttpClient HttpClient = new HttpClient();

        public Task<HttpResponseMessage> GetAsync(Uri requestUri) => HttpClient.GetAsync(requestUri);
    }
}
