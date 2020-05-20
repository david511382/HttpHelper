using HttpHelper.Interface;
using HttpHelper.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace HttpHelper
{
    public sealed class HttpRequest : IRequest, IHttpRequest
    {
        public static HttpRequest New()
        {
            return new HttpRequest();
        }

        private CookieCollection _cookies;
        private List<KeyValuePair<string, string>> _headers;
        private List<KeyValuePair<string, string>> _querys;
        private ByteArrayContent _body;
        private Uri _url;
        private bool _isAllowHttpNotOK;

        public HttpRequest()
        {
            _cookies = new CookieCollection();
            _body = null;
            _headers = new List<KeyValuePair<string, string>>();
            _querys = new List<KeyValuePair<string, string>>();
            _url = null;
        }

        public async Task<ResponseModel> Send(HttpMethod method)
        {
            HttpResponseMessage response;
            ResponseModel result = new ResponseModel();
            using (HttpClientHandler handler = new HttpClientHandler())
            {
                handler.AllowAutoRedirect = false;

                if (_cookies != null)
                    foreach (Cookie cookie in _cookies)
                        handler.CookieContainer.Add(cookie);

                using (HttpClient client = new HttpClient(handler))
                {
                    setServicePoint(_url);

                    if (_body != null && (await _body.ReadAsByteArrayAsync()).Length == 0)
                        _body = null;
                    HttpRequestMessage requestMessage = createRequestMessage(_url, method, _body);
                    if (_headers != null)
                        foreach (KeyValuePair<string, string> header in _headers)
                            requestMessage.Headers.Add(header.Key, header.Value);

                    response = await client.SendAsync(requestMessage);

                    // 返回 *非* 200 系列的 Http Status Code 則丟出例外
                    if (!_isAllowHttpNotOK)
                        response.EnsureSuccessStatusCode();

                    result.StatusCode = response.StatusCode;
                    result.Cookies = handler.CookieContainer.GetCookies(_url);
                    result.Content = await response.Content.ReadAsStringAsync();
                    result.Header = response.Headers;
                }
            }

            return result;
        }
        public async Task<T> Send<T>(HttpMethod method)
        {
            ResponseModel result = await Send(method);
            return JsonConvert.DeserializeObject<T>(result.Content);
        }

        public Task<ResponseModel> Delete()
        {
            return Send(HttpMethod.Delete);
        }
        public async Task<T> Delete<T>()
        {
            return await Send<T>(HttpMethod.Delete);
        }

        public Task<ResponseModel> Get()
        {
            return Send(HttpMethod.Get);
        }

        public async Task<T> Get<T>()
        {
            return await Send<T>(HttpMethod.Get);
        }

        public Task<ResponseModel> Post()
        {
            return Send(HttpMethod.Post);
        }
        public async Task<T> Post<T>()
        {
            return await Send<T>(HttpMethod.Post);
        }

        public Task<ResponseModel> Put()
        {
            return Send(HttpMethod.Put);
        }
        public async Task<T> Put<T>()
        {
            return await Send<T>(HttpMethod.Put);
        }

        public Task<ResponseModel> Patch()
        {
            return Send(new HttpMethod("PATCH"));
        }
        public async Task<T> Patch<T>()
        {
            return await Send<T>(new HttpMethod("PATCH"));
        }

        public IHttpRequest SetCookies(CookieCollection cookies)
        {
            _cookies = cookies;
            return this;
        }

        public IHttpRequest AddCookie(Cookie cookie)
        {
            _cookies.Add(cookie);
            return this;
        }

        public IHttpRequest SetHeaders(IEnumerable<KeyValuePair<string, string>> keyValues)
        {
            _headers = keyValues.ToList();
            return this;
        }

        public IHttpRequest AddHeader(KeyValuePair<string, string> keyValue)
        {
            _headers.Add(keyValue);
            return this;
        }

        public IHttpRequest SetQuery(IEnumerable<KeyValuePair<string, string>> keyValues)
        {
            _querys.Clear();
            _querys.AddRange(keyValues);
            return this;
        }

        public IHttpRequest AddQuery(string key, string value)
        {
            _querys.Add(new KeyValuePair<string, string>(key, value));
            return this;
        }

        public IHttpRequest SetForm<T>(T obj)
        {
            string jsonStr = JsonConvert.SerializeObject(obj);
            Dictionary<string, string> dic = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStr);
            IEnumerable<KeyValuePair<string, string>> kvs = dic.Select(c => c);
            return SetForm(kvs);
        }

        public IHttpRequest SetForm(IEnumerable<KeyValuePair<string, string>> keyValues)
        {
            _body = new FormUrlEncodedContent(keyValues);
            return this;
        }

        public IHttpRequest SetJson<T>(T obj)
        {
            string jsonStr = JsonConvert.SerializeObject(obj);
            _body = new StringContent(jsonStr, Encoding.UTF8, "application/json");
            return this;
        }

        public IHttpRequest SetJson(IEnumerable<KeyValuePair<string, object>> keyValues)
        {
            return SetJson<IEnumerable<KeyValuePair<string, object>>>(keyValues);
        }

        public IRequest To(string url, bool isAllowHttpNotOK = false)
        {
            string query = string.Join("&", _querys.Select(kv => $"{kv.Key}={kv.Value}"));
            url = $"{url}?{query}";
            _url = new Uri(url);

            _isAllowHttpNotOK = isAllowHttpNotOK;

            return this;
        }

        private static void setServicePoint(Uri uri)
        {
            ServicePoint serverPoint = ServicePointManager.FindServicePoint(uri);

            // 設定 30 秒沒有活動即關閉連線，預設 -1 (永不關閉)
            serverPoint.ConnectionLeaseTimeout = (int)TimeSpan.FromSeconds(30).TotalMilliseconds;

            // 停用 100-Continue
            // https://docs.microsoft.com/zh-tw/dotnet/api/system.net.servicepointmanager.expect100continue?view=netstandard-2.0
            serverPoint.Expect100Continue = false;
        }

        private static HttpRequestMessage createRequestMessage(Uri uri, HttpMethod method, ByteArrayContent content)
        {
            HttpRequestMessage requestMessage = new HttpRequestMessage
            {
                // 使用 HTTP/1.0
                Version = HttpVersion.Version10,
                Method = method,
                RequestUri = uri
            };

            if (content != null)
            {
                requestMessage.Content = content;
            }

            // 完成後關閉連接, 預設為 false (Keep-Alive)
            requestMessage.Headers.ConnectionClose = true;

            CacheControlHeaderValue cacheControl = new CacheControlHeaderValue
            {
                NoCache = true
            };

            requestMessage.Headers.CacheControl = cacheControl;
            return requestMessage;
        }
    }
}