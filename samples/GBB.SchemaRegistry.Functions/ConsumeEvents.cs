using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Data.SchemaRegistry;
using Azure.Identity;
using Azure.Messaging.EventHubs;
using Microsoft.Azure.Data.SchemaRegistry.ApacheAvro;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using zohan.schemaregistry.events;

namespace GBB.SchemaRegistry.Functions
{
    public class ConsumeEvents
    {
        private static readonly SchemaRegistryAvroEncoder _encoder = InitializeEncoder();

        [FunctionName("ConsumeEvents")]
        public async Task Run(
            [EventHubTrigger("%EventHubName%", Connection = "EventHubsConnection")] EventData[] events, 
            ILogger log)
        {
            log.LogInformation("ConsumerEvents function invoked");

            var exceptions = new List<Exception>();

            // Iterate through the collection of events and 
            // decode each of them with the schema registry
            foreach (EventData eventData in events)
            {
                try
                {
                    // Decode the event
                    CustomerLoyalty loyalty = (CustomerLoyalty)await _encoder.DecodeMessageDataAsync(eventData, typeof(CustomerLoyalty));
                    log.LogInformation($"Customer ID: {loyalty.CustomerId} - {loyalty.PointsAdded} points added");
                }
                catch (Exception e)
                {
                    exceptions.Add(e);
                }
            }            

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }

        private static SchemaRegistryAvroEncoder InitializeEncoder()
        {
            // Instantiate a schema registry client with default Azure credentials
            // See: https://docs.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet
            var schemaRegistryUrl = Environment.GetEnvironmentVariable("SchemaRegistryUrl");
            var schemaGroup = Environment.GetEnvironmentVariable("SchemaGroup");
            var client = new SchemaRegistryClient(schemaRegistryUrl, new DefaultAzureCredential());

            // Create an instance of the avro encoder that for the schema registry           
            return new SchemaRegistryAvroEncoder(
                client,
                schemaGroup,
                new SchemaRegistryAvroObjectEncoderOptions { AutoRegisterSchemas = false });
        }
    }
}
