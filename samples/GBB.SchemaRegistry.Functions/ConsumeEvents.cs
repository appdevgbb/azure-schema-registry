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
        private static readonly SchemaRegistryClient _schemaRegistryClient = InitializeSchemaRegistryClient();
        private static readonly string _schemaGroup = Environment.GetEnvironmentVariable("SchemaGroup");

        [FunctionName("ConsumeEvents")]
        public async Task Run(
            [EventHubTrigger("%EventHubName%", Connection = "EventHubsConnection")] EventData[] events, 
            ILogger log)
        {
            log.LogInformation("ConsumerEvents function invoked");

            var exceptions = new List<Exception>();

            // Create an instance of the avro encoder
            var encoder = new SchemaRegistryAvroEncoder(
                _schemaRegistryClient,
                _schemaGroup,
                new SchemaRegistryAvroObjectEncoderOptions { AutoRegisterSchemas = false });

            // Iterate through the collection of events and 
            // decode each of them with the schema registry
            foreach (EventData eventData in events)
            {
                try
                {
                    // Decode the event
                    CustomerLoyalty loyalty = (CustomerLoyalty)await encoder.DecodeMessageDataAsync(eventData, typeof(CustomerLoyalty));
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

        private static SchemaRegistryClient InitializeSchemaRegistryClient()
        {
            var schemaRegistryUrl = Environment.GetEnvironmentVariable("SchemaRegistryUrl");
            return new SchemaRegistryClient(schemaRegistryUrl, new DefaultAzureCredential());
        }
    }
}
