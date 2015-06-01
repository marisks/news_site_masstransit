using System;
using MassTransit;
using MassTransit.BusConfigurators;
using MassTransit.LibLog;
using MassTransit.Transports.AzureServiceBus;
using Microsoft.ServiceBus;

namespace Configuration
{
    public class AzureBusInitializer
    {
        public static IServiceBus CreateBus(
            string queueName,
            Action<ServiceBusConfigurator> moreInitialization,
            string connectionString)
        {
            var bus = ServiceBusFactory.New(sbc =>
            {
                sbc.UseLibLog();

                var queueUri = "azure-sb://" + AzureBusConfiguration.Namespace + "/" + queueName;

                sbc.ReceiveFrom(queueUri);

                sbc.UseAzureServiceBus(a => a.ConfigureNamespace(AzureBusConfiguration.Namespace, h =>
                {
                    h.SetKeyName("RootManageSharedAccessKey");
                    h.SetKey(CnBuilder(connectionString).SharedAccessKey);
                }));
                sbc.UseAzureServiceBusRouting();

                moreInitialization(sbc);
            });

            return bus;
        }

        private static ServiceBusConnectionStringBuilder CnBuilder(string connectionString)
        {
            return new ServiceBusConnectionStringBuilder(connectionString);
        }
    }
}
