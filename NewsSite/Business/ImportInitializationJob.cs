using System.Configuration;
using System.Linq;
using Configuration;
using Contracts;
using EPiServer.BaseLibrary.Scheduling;
using EPiServer.PlugIn;
using MassTransit;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace NewsSite.Business
{
    [ScheduledPlugIn(DisplayName = "Init import", SortIndex = 2000)]
    public class ImportInitializationJob : JobBase
    {
        public override string Execute()
        {
            var cn = ConfigurationManager
                    .ConnectionStrings["EPiServerAzureEvents"]
                    .ConnectionString;
            
            var container = CreateStorageContainer();

            using (var bus = AzureBusInitializer.CreateBus(
                AzureBusConfiguration.ImportDataQueueName, x => { }, cn))
            {
                foreach (var item in container.ListBlobs()
                                                .OfType<CloudBlockBlob>())
                {
                    var importFile = new ImportFile
                    {
                        Name = item.Name, Uri = item.Uri
                    };
                    bus.Publish(importFile, x => {x.SetDeliveryMode(DeliveryMode.Persistent);});
                }
            }

            return "Success";
        }

        private const string ContainerName = "epiimportdata";

        private static CloudBlobContainer CreateStorageContainer()
        {
            var cn = ConfigurationManager
                            .ConnectionStrings["EPiServerAzureBlobs"]
                            .ConnectionString;
            var storageAccount = CloudStorageAccount.Parse(cn);
            var blobClient = storageAccount.CreateCloudBlobClient();

            var container = blobClient.GetContainerReference(ContainerName);
            container.CreateIfNotExists();
            return container;
        }
    }
}