using HttpHelper.Model;
using System.Net.Http;
using System.Threading.Tasks;

namespace HttpHelper.Interface
{
    public interface IRequest
    {
        Task<ResponseModel> Send(HttpMethod method);
        Task<T> Send<T>(HttpMethod method);

        Task<ResponseModel> Get();
        Task<T> Get<T>();

        Task<ResponseModel> Post();
        Task<T> Post<T>();

        Task<ResponseModel> Put();
        Task<T> Put<T>();

        Task<ResponseModel> Patch();
        Task<T> Patch<T>();

        Task<ResponseModel> Delete();
        Task<T> Delete<T>();
    }
}
