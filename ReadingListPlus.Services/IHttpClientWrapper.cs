using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace ReadingListPlus.Services
{
    public interface IHttpClientWrapper
    {
        Task<HttpResponseMessage> GetAsync(Uri requestUri);
    }
}