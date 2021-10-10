using AGoodSpotifyAPI.JsonSchema;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace AGoodSpotifyAPI.Web
{
    internal class WebResult<T> : IDisposable
    {
        public WebResponse Response { get; }
        public bool IsError { get; private set; }
        public T Result { get; private set; }
        public ErrorJSON Error { get; private set; }

        private string text;

        public WebResult(WebResponse response, bool isError= false)
        {
            Response = response;
            IsError = isError;
        }

        public async Task<WebResult<T>> InitializeAsync()
        {
            using var stream = Response.GetResponseStream();
            using var reader = new StreamReader(stream);

            text = await reader.ReadToEndAsync();
            
            //Console.WriteLine(text);
            if(IsError)
            {
                Error = JsonConvert.DeserializeObject<ErrorHelperJSON>(text).Error;
            }
            else
            {
                Result = JsonConvert.DeserializeObject<T>(text);
                Error = null;                             
            }
            Response.Dispose();
            return this;
        }

        public void Dispose()
        {
            try
            {
                Response.Dispose();
            }
            catch { }
        }

        private class ErrorHelperJSON
        {
            public ErrorJSON Error { get; set; }
        }
    }
}
