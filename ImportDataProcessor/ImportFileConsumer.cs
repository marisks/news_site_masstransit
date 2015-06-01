using System.Collections.Generic;
using System.IO;
using System.Linq;
using Contracts;
using MassTransit;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ImportDataProcessor
{
    public class ImportFileConsumer : Consumes<ImportFile>.Context
    {
        public void Consume(IConsumeContext<ImportFile> message)
        {
            var importFile = message.Message;
            var container = CreateStorageContainer();
            var blob = container.GetBlockBlobReference(importFile.Name);

            var articles = ReadArticles(blob).ToList();

            articles.ForEach(article =>
            {
                message.Bus.Publish(article);
            });
        }

        private static IEnumerable<Article> ReadArticles(CloudBlockBlob blob)
        {
            var text = blob.DownloadText();

            using (var sr = new StringReader(text))
            {
                string line;
                var row = 0;
                while ((line = sr.ReadLine()) != null)
                {
                    row++;
                    if (row == 1) continue;

                    var fields = line.Split(',');
                    yield return new Article
                    {
                        Name = fields[0],
                        Intro = fields[1],
                        Content = fields[2],
                        ImageUrl = fields[3]
                    };
                }
            }
        }

        private const string ContainerName = "epiimportdata";
        private static CloudBlobContainer CreateStorageContainer()
        {
            var connectionString = CloudConfigurationManager.GetSetting("EPiServerAzureBlobs");
            var storageAccount = CloudStorageAccount.Parse(connectionString);
            var blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(ContainerName);
            container.CreateIfNotExists();
            return container;
        }
    }
}
