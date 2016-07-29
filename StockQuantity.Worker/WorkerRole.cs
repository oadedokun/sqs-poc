using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure;
using Microsoft.Azure.Documents.Client;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.ServiceRuntime;
using StockQuantity.Data;
using StockQuantity.Worker.Messaging;
using WarehouseAvailableStock.Worker;

namespace StockQuantity.Worker
{
    public class WorkerRole : RoleEntryPoint
    {
        private WarehouseAvailableStockChangedV1Handler _warehouseAvailableStockChangedV1Handler;
        private IStockQuantityAggregateStore _stockQuantityAggregateStore;
        private TopicClient _topicClient;
        private SubscriptionClient[] _subscriptionClients;
        private Task[] _receiveTasks;
        private ConnectionPolicy _connectionPolicy;
        private ManualResetEvent[] _completedEvents;

        public override void Run()
        {
            Trace.WriteLine("Starting processing of messages");

            // Initiates the message pump and callback is invoked for each message that is received, calling close on the client will stop the pump.

        }

        public override bool OnStart()
        {
            TelemetryConfiguration.Active.InstrumentationKey = RoleEnvironment.GetConfigurationSettingValue("APPINSIGHTS_INSTRUMENTATIONKEY");
            TelemetryConfiguration.Active.TelemetryInitializers.Add(new ItemCorrelationTelemetryInitializer());
            ServicePointManager.DefaultConnectionLimit = Environment.ProcessorCount * 12;

            _connectionPolicy = new ConnectionPolicy();
            _connectionPolicy.PreferredLocations.Add("North Europe"); // first preference
            _connectionPolicy.PreferredLocations.Add("West Europe"); // second preference 
            var sqServiceBusConnectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString.StockQuantity");

            var docDbName = CloudConfigurationManager.GetSetting("Microsoft.DocumentDB.StockQuantity.DBName");
            var docDbsqColName = CloudConfigurationManager.GetSetting("Microsoft.DocumentDB.StockQuantity.DBCollectionName");
            var docDbsvColName = CloudConfigurationManager.GetSetting("Microsoft.DocumentDB.SkuVariantMap.DBCollectionName");
            var docDbEndpointName = CloudConfigurationManager.GetSetting("Microsoft.DocumentDB.StockQuantity.EUN.AccountEndpoint");
            var docDbEndpointKey = CloudConfigurationManager.GetSetting("Microsoft.DocumentDB.StockQuantity.EUN.AccountKey");

            _topicClient = TopicClient.CreateFromConnectionString(sqServiceBusConnectionString);
            InitialiseSubscriptionClients();
            _stockQuantityAggregateStore = new StockQuantityAggregateDocDb(docDbName, docDbsqColName, docDbsvColName, docDbEndpointName, docDbEndpointKey, _connectionPolicy);
            _warehouseAvailableStockChangedV1Handler = new WarehouseAvailableStockChangedV1Handler(_topicClient, _stockQuantityAggregateStore);
            return base.OnStart();
        }

        public override void OnStop()
        {
            // Close the connection to Service Bus Queue
            //Client.Close();
            //CompletedEvent.Set();
            base.OnStop();
        }

        private void InitialiseSubscriptionClients()
        {
            var wasServiceBusConnectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString.WarehouseAvailableStock");
            var wasSubscriptionName = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString.WarehouseAvailableStock.SubscritpionName");
            var wasTopicPath = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString.WarehouseAvailableStock.TopicPath");
            int concurrencyLimit = Convert.ToInt16(CloudConfigurationManager.GetSetting("MaximumConcurrency"));
            _completedEvents = new ManualResetEvent[concurrencyLimit];
            _receiveTasks = new Task[concurrencyLimit];
            _subscriptionClients = new SubscriptionClient[concurrencyLimit];
            for (var i = 0; i < concurrencyLimit; i++)
            {
                _subscriptionClients[i] = SubscriptionClient.CreateFromConnectionString(wasServiceBusConnectionString,
                    wasTopicPath, wasSubscriptionName);
            }
        }

        private void ReceiveWarehouseStockChanged()
        {
            for (var i = 0; i < _subscriptionClients.Length; i++)
            {
                var ce = new ManualResetEvent(false);
                var sc = _subscriptionClients[i];
                _receiveTasks[i] = Task.Run(() => {

                    sc.OnMessage(receivedMessage =>
                    {
                        try
                        {
                            // Process the message
                            //Trace.WriteLine("Processing Service Bus message: " + receivedMessage.SequenceNumber.ToString());
                        }
                        catch
                        {
                            // Handle any message processing specific exceptions here
                        }
                    });

                    ce.WaitOne();
                });

                _completedEvents[i] = ce;
            }
        }
    }
}
