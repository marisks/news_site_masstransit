using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Contracts;
using MassTransit;
using Newtonsoft.Json;

namespace ImportArticleProcessor
{
    public class ImportArticleConsumer : Consumes<Article>.Context
    {
        public void Consume(IConsumeContext<Article> message)
        {
            var article = message.Message;

            using (var client = CreateClient())
            {
                var str = JsonConvert.SerializeObject(article);
                var content = new StringContent(str, Encoding.UTF8, "text/json");
                var result = client.PostAsync("api/article", content).Result;
                result.EnsureSuccessStatusCode();
            }
        }

        private HttpClient CreateClient()
        {
            var client = new HttpClient { BaseAddress = new Uri("http://epinewssite.azurewebsites.net/") };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }
    }
}