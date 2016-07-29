using System;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.DataContracts;

namespace WarehouseAvailableStock.Worker
{
    public class RequestTelemetryHelper
    {
        private static readonly TelemetryClient TelemetryClient = new TelemetryClient();
        private static string SUCCESS_CODE = "200";
        private static string FAILURE_CODE = "500";

        public static RequestTelemetry StartNewRequest(string name, DateTimeOffset startTime)
        {
            var request = new RequestTelemetry();
            request.Name = name;
            request.Timestamp = startTime;
            return request;
        }

        public static void DispatchRequest(RequestTelemetry request, TimeSpan duration, bool success)
        {
            request.Duration = duration;
            request.Success = success;
            request.ResponseCode = (success) ? SUCCESS_CODE : FAILURE_CODE;
            TelemetryClient.TrackRequest(request);
        }
    }
}
