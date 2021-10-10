using System;
using System.Threading.Tasks;
using System.Net;

namespace AGoodSpotifyAPI.Web
{
    internal enum Method
    {
        Get, Post, Put, Delete
    }

    internal class WebPart
    {
        private static async Task<WebResult<T>> MakeWebRequest<T>(string token, string url, Method method, string plusKey = null, string plusValue = null)
        {
            if (string.IsNullOrWhiteSpace(token)) throw new ArgumentNullException(token);

            string m = method switch
            {
                Method.Get => "GET",
                Method.Post => "POST",
                Method.Put => "PUT",
                Method.Delete => "DELETE",
                _ => throw new NotImplementedException()
            };

            WebRequest request = WebRequest.Create(url);
            request.Method = m;
            request.Headers.Add("Authorization", "Bearer " + token);
            request.ContentType = "application/json; charset=utf-8";

            if(!(plusKey is null))
            {
                request.Headers.Add(plusKey, plusValue);
            }

            WebResult<T> result;
            try
            {
                var response = await request.GetResponseAsync();
                result = await new WebResult<T>(response).InitializeAsync();
            }
            catch(WebException e)
            {
                result = await new WebResult<T>(e.Response, true).InitializeAsync();
            }
            
            return result;
        }
        internal static async Task<WebResult<T>> MakeWebRequest<T>(WebHelper<T> helper)
        {
            if (helper.Token is null || helper.Url is null) throw new ArgumentNullException();
            var result = await MakeWebRequest<T>(token: helper.Token, url: helper.Url, method: helper.Method, helper.PlusKey, helper.PlusValue);
            return result;
        }
    }
}
