using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Skynet.Wiki.API.Models;

namespace Skynet.Wiki.API
{
    public class WikiApi
    {
        private readonly HttpClient _client;
        private readonly ILogger<WikiApi> _logger;
        private readonly string _address;
            
        /// <summary>
        /// Formats request endpoints
        /// </summary>
        /// <param name="endpoint"></param>
        /// <returns></returns>
        private string Endpoint(string endpoint) => $"{_address}/{endpoint}";
        
        public WikiApi(IConfiguration config, ILogger<WikiApi> logger)
        {
            _logger = logger;
            _address = config["Wiki:Address"];
            _client = new HttpClient();
            _client.BaseAddress = new Uri(_address);
        }
    
        /// <summary>
        /// Retrieve tags from Engineer's Notebook
        /// </summary>
        /// <returns>Array of tags</returns>
        public async Task<Tag[]> GetTagsAsync()
        {
            HttpRequestMessage message = new(HttpMethod.Get, new Uri(Endpoint("tag/getall")));
            HttpResponseMessage response = await _client.SendAsync(message);
            string result = await response.Content.ReadAsStringAsync();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Tag[]>(result);
        }

        /// <summary>
        /// Convert <typeparamref name="T"/> into JSON, then convert the JSON into a byte array
        /// </summary>
        /// <param name="value"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns>Byte array based on JSON representation of <paramref name="value"/></returns>
        public static byte[] Create<T>(T value)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(value);
            using var stream = new MemoryStream(Encoding.ASCII.GetBytes(json));
            return stream.ToArray();
        }
        
        /// <summary>
        /// Find the documentation associated with the <paramref name="tags"/> the user has in their message
        /// </summary>
        /// <param name="tags"></param>
        /// <returns>(byte[] file, string nameOfFile)</returns>
        public async Task<(byte[] bytes, string filename)> FindDocs(string[] tags)
        {
            // This will be sent as part of request body
            byte[] data = Create(new PdfRequest
            {
                TagNames = tags
            });
            
            var uri = new Uri(Endpoint("documentation/FilterByTagPDF"));
            var request = WebRequest.CreateHttp(uri);
            
            // The body's content will be in JSON format
            request.ContentType = "application/json";
            request.Method = "POST";
            
            // This must match the length of data from above
            request.ContentLength = data.Length;
            
            // Must write our data object into the request stream
            using (Stream requestStream = request.GetRequestStream())
            {
                await requestStream.WriteAsync(data,0,data.Length);
            }
            
            var response = await request.GetResponseAsync();

            // We shall determine if a filename was provided to us from the API or if a default one will be given
            ContentDispositionHeaderValue contentDisposition;

            var filename =
                ContentDispositionHeaderValue.TryParse(response.Headers["Content-Disposition"], out contentDisposition)
                    ? contentDisposition.FileName
                    : "Guide.pdf";

            using MemoryStream stream = new();
            await response.GetResponseStream().CopyToAsync(stream);

            return (stream.ToArray(), filename);
        }
    }
}