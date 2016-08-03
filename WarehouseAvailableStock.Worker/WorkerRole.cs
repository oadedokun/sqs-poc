using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using Microsoft.Azure;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.Azure.Documents.Client;
using StockQuantity2.Data;

namespace WarehouseAvailableStock.Worker
{
    public class WorkerRole : RoleEntryPoint
    {
        
        private volatile bool _onStopCalled;
        private volatile bool _returnedFromRunMethod;
        private WarehouseAvailableStockChangedPublisher _publisher;
        private IStockQuantityAggregateStore _stockQuantityAggregateStore;
        private TopicClient _topicClient;
        private ConnectionPolicy _connectionPolicy;
        public override void Run()
        {
            Trace.WriteLine("Starting processing of messages");
            
            while (true)
            {
                try
                {
                    if (_onStopCalled)
                    {
                        Trace.TraceInformation("onStopCalled Warehouse Available Stock");
                        _returnedFromRunMethod = true;
                        return;
                    }
                    
                    _publisher.Publish();
                }
                catch (Exception ex)
                {
                    string err = ex.Message;
                    if (ex.InnerException != null)
                    {
                        err += " Inner Exception: " + ex.InnerException.Message;
                    }
                    Trace.TraceError(err, ex);
                }
            }
        }

        public override bool OnStart()
        {
            TelemetryConfiguration.Active.InstrumentationKey = RoleEnvironment.GetConfigurationSettingValue("APPINSIGHTS_INSTRUMENTATIONKEY");
            TelemetryConfiguration.Active.TelemetryInitializers.Add(new ItemCorrelationTelemetryInitializer());
            ServicePointManager.DefaultConnectionLimit = Environment.ProcessorCount * 12;

            _connectionPolicy = new ConnectionPolicy();
            _connectionPolicy.PreferredLocations.Add("North Europe"); // first preference
            _connectionPolicy.PreferredLocations.Add("West Europe"); // second preference 
            var wasServiceBusConnectionString = CloudConfigurationManager.GetSetting("Microsoft.ServiceBus.ConnectionString.WarehouseAvailableStock");
            var docDbName = CloudConfigurationManager.GetSetting("Microsoft.DocumentDB.StockQuantity.DBName");
            var docDbsqColName = CloudConfigurationManager.GetSetting("Microsoft.DocumentDB.StockQuantity.DBCollectionName");
            var docDbsvColName = CloudConfigurationManager.GetSetting("Microsoft.DocumentDB.SkuVariantMap.DBCollectionName");
            var docDbEndpointName = CloudConfigurationManager.GetSetting("Microsoft.DocumentDB.StockQuantity.EUN.AccountEndpoint");
            var docDbEndpointKey = CloudConfigurationManager.GetSetting("Microsoft.DocumentDB.StockQuantity.EUN.AccountKey");
            int concurrencyLimit = Convert.ToInt16(CloudConfigurationManager.GetSetting("MaximumConcurrency"));
            _topicClient = TopicClient.CreateFromConnectionString(wasServiceBusConnectionString);
            _stockQuantityAggregateStore = new RegionStockPostionAggregateRepository(docDbName, docDbsqColName, docDbsvColName, docDbEndpointName, docDbEndpointKey, _connectionPolicy);
            _publisher = new WarehouseAvailableStockChangedPublisher(_topicClient, _stockQuantityAggregateStore, concurrencyLimit, RequestTelemetryHelper.StartNewRequest("PublishWarehouseAvailableStockChanged", DateTimeOffset.UtcNow));
            return base.OnStart();
        }

        public override void OnStop()
        {
            _onStopCalled = true;
            while (_returnedFromRunMethod == false)
            {
                Thread.Sleep(1000);
            }
            _topicClient.Close();
            base.OnStop();
        }
    }
}
