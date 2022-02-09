using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Data.SchemaRegistry;
using Azure.Identity;
using Azure.Messaging.EventHubs;
using Microsoft.Azure.Data.SchemaRegistry.ApacheAvro;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using zohan.schemaregistry.events;

namespace GBB.SchemaRegistry.IsolatedFunctions
{
    public class ConsumeEvents
    {
        private readonly ILogger _logger;
        private static readonly SchemaRegistryAvroEncoder _encoder = InitializeEncoder();

        public ConsumeEvents(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ConsumeEvents>();
        }

        [Function("ConsumeEvents")]
        public async Task Run(
            [EventHubTrigger("%EventHubName%", Connection = "EventHubsConnection")] byte[][] events,
            DateTime[] enqueuedTimeUtcArray,
            long[] sequenceNumberArray,
            string[] offsetArray,
            Dictionary<string, JsonElement>[] propertiesArray,
            Dictionary<string, JsonElement>[] systemPropertiesArray,
            FunctionContext functionContext)
        {
            _logger.LogInformation($"ConsumeEvents function invoked");

            var exceptions = new List<Exception>();

            // Create a tuple to hold the event body and index from the 
            // array so that we don't have to use a counter variable for
            // the index when we iterate the collection (fancy).
            foreach (var (eventBody, index) in events.Select((v, i) => (v, i)))
            {
                // We lost EventData during the invocation of the function and have to rebuild it
                // in order to decode the payload with the schema registry. The system properties 
                // bag contains the content type needed to decode. 
                
                // Get system properties for the current element in the array
                var props = systemPropertiesArray[index];
                
                // Create an instance of the event data class and set the content type
                // with the value from the system properties collection. 
                var eventData = new EventData(eventBody);

                // The content type will include the type along with the schema ID from
                // the registry (example: avro/binary+fe7d144ca29b4b887bf66)
                eventData.ContentType = props["content-type"].GetProperty("Value").GetString();                
                
                // Decode the event data
                CustomerLoyalty loyalty = (CustomerLoyalty)await _encoder.DecodeMessageDataAsync(eventData, typeof(CustomerLoyalty));
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
            var client =  new SchemaRegistryClient(schemaRegistryUrl, new DefaultAzureCredential());

            // Create an instance of the avro encoder that for the schema registry           
            return new SchemaRegistryAvroEncoder(
                client,
                schemaGroup,
                new SchemaRegistryAvroObjectEncoderOptions { AutoRegisterSchemas = false });
        }       
    }
}
