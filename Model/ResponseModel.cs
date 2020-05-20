using System.Net;
using System.Net.Http.Headers;

namespace HttpHelper.Model
{
    public struct ResponseModel
    {
        public string Content;
        public HttpStatusCode StatusCode;
        public HttpResponseHeaders Header;
        public CookieCollection Cookies;
    }

}
