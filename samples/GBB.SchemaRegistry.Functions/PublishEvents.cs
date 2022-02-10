using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.Data.SchemaRegistry;
using Microsoft.Azure.Data.SchemaRegistry.ApacheAvro;
using zohan.schemaregistry.events;
using Azure.Messaging.EventHubs;

namespace GBB.SchemaRegistry.Functions
{
    public static class PublishEvents
    {
        private static readonly SchemaRegistryAvroEncoder _encoder = InitializeEncoder();

        [FunctionName("PublishEvents")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [EventHub("%EventHubName%", Connection = "EventHubsConnection")] IAsyncCollector<EventData> events,
            ILogger log)
        {
            log.LogInformation("PublishEvents function invoked");

            // Create a customer loyalty record
            var loyalty = new CustomerLoyalty
            {
                CustomerId = 1,
                Description = "10 points added",
                PointsAdded = 10,
            };

            // Encode the loyalty record using the schema registry
            EventData eventData = await _encoder.EncodeMessageDataAsync<EventData>(loyalty);

            // Add the loyalty record to the event hub output binding
            await events.AddAsync(eventData);

            return new OkResult();
        }

        private static SchemaRegistryAvroEncoder InitializeEncoder()
        {
            // Instantiate a schema registry client with default Azure credentials
            // See: https://docs.microsoft.com/en-us/dotnet/api/azure.identity.defaultazurecredential?view=azure-dotnet
            var schemaRegistryUrl = Environment.GetEnvironmentVariable("SchemaRegistryUrl");
            var schemaGroup = Environment.GetEnvironmentVariable("SchemaGroup");
            var client = new SchemaRegistryClient(schemaRegistryUrl, new DefaultAzureCredential());

            // Create an instance of the avro encoder and set the auto flag
            // to true in case the schema does not exists in the registry.      
            return new SchemaRegistryAvroEncoder(
                client,
                schemaGroup,
                new SchemaRegistryAvroObjectEncoderOptions { AutoRegisterSchemas = true });
        }
    }
}
