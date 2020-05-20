using System.Collections.Generic;
using System.Net;

namespace HttpHelper.Interface
{
    public interface IHttpRequest
    {
        IHttpRequest SetCookies(CookieCollection cookies);
        IHttpRequest AddCookie(Cookie cookie);

        IHttpRequest SetHeaders(IEnumerable<KeyValuePair<string, string>> keyValues);
        IHttpRequest AddHeader(KeyValuePair<string, string> keyValues);

        IHttpRequest SetQuery(IEnumerable<KeyValuePair<string, string>> keyValues);
        IHttpRequest AddQuery(string key, string value);

        IHttpRequest SetForm<T>(T obj);
        IHttpRequest SetForm(IEnumerable<KeyValuePair<string, string>> keyValues);

        IHttpRequest SetJson<T>(T obj);
        IHttpRequest SetJson(IEnumerable<KeyValuePair<string, object>> keyValues);

        IRequest To(string url, bool isAllowHttpNotOK = false);
    }
}
