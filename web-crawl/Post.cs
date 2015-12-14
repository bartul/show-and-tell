using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace DataExtraction
{
    public class Post
    {
        public Post(HttpClient client)
        {
            _client = client;
        }
        private HttpClient _client;

        public async Task<byte[]> ExecuteAsync(byte[] postContent, string requestUri)
        {
            var message = new HttpRequestMessage(HttpMethod.Post, requestUri);
            message.Headers.Add("Cache-Control", "no-cache");
            message.Headers.Add("Accept", "*/*");
            message.Headers.Add("Accept-Encoding", "");
            message.Headers.Add("Accept-Language", "hr-HR,hr;q=0.8,en-US;q=0.5,en;q=0.3");
            message.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.3; WOW64; Trident/7.0; rv:11.0) like Gecko");

            message.Headers.Add("Referer", "https://kartica.ina.hr/Izvjesca.aspx");
            message.Headers.Add("DNT", "1");

            message.Headers.Add("Connection", "Keep-Alive");

            message.Content = new ByteArrayContent(postContent);
            message.Content.Headers.Add("Content-Type", "application/x-www-form-urlencoded; charset=utf-8");

            var response = await _client.SendAsync(message);
            response.EnsureSuccessStatusCode();

            return response.Content.ReadAsByteArrayAsync().Result;
        }
        public async Task<string> ExecuteAsStringAsync(byte[] postContent, string requestUri)
        {
            return Encoding.UTF8.GetString(await this.ExecuteAsync(postContent, requestUri));
        }
    }
}
