using System.Net;
using System.Threading;
using Configuration;
using MassTransit;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace ImportArticleProcessor
{
    public class WorkerRole : RoleEntryPoint
    {
        readonly ManualResetEvent CompletedEvent = new ManualResetEvent(false);
        private IServiceBus _bus;

        public override bool OnStart()
        {
            ServicePointManager.DefaultConnectionLimit = 12;

            var cn = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString");

            _bus = AzureBusInitializer.CreateBus(AzureBusConfiguration.ImportArticleQueueName, sbc =>
            {
                sbc.SetConcurrentConsumerLimit(64);
                sbc.Subscribe(subs =>
                {
                    subs.Consumer<ImportArticleConsumer>().Permanent();
                });
            }, cn);

            
            return base.OnStart();
        }

        public override void OnStop()
        {
             if (_bus != null)
                _bus.Dispose();

            CompletedEvent.Set();
            base.OnStop();
        }
    }
}
