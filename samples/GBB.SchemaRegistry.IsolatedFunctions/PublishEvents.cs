using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Azure.Data.SchemaRegistry;
using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Azure.Data.SchemaRegistry.ApacheAvro;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using zohan.schemaregistry.events;

namespace GBB.SchemaRegistry.IsolatedFunctions
{
    public class PublishEvents
    {
        private readonly ILogger _logger;
        private static readonly SchemaRegistryAvroEncoder _encoder = InitializeEncoder();

        public PublishEvents(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<PublishEvents>();
        }

        [Function("PublishEvents")]
        public async Task<HttpResponseData> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            FunctionContext context)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            response.WriteString("Welcome to Azure Functions!");

            // Create a customer loyalty record
            var loyalty = new CustomerLoyalty
            {
                CustomerId = 1,
                Description = "10 points added",
                PointsAdded = 10,
            };

            // Encode the loyalty record using the schema registry
            EventData encodedLoyalty = await _encoder.EncodeMessageDataAsync<EventData>(loyalty);

            // Publish to Event Hubs
            // Note: Having issues with multiple output bindings and this working
            // property. For now, let's just use the Event Hubs SDK directly.
            var eventHubsFQNamespace = Environment.GetEnvironmentVariable("EventHubsConnection__fullyQualifiedNamespace");
            var eventHubName = Environment.GetEnvironmentVariable("EventHubName");
            await using (var producer = new EventHubProducerClient(eventHubsFQNamespace, eventHubName, new DefaultAzureCredential()))
            {
                using EventDataBatch eventBatch = await producer.CreateBatchAsync();
                if (!eventBatch.TryAdd(encodedLoyalty))
                {
                    throw new Exception($"Event could not be added");
                }

                await producer.SendAsync(eventBatch);
            }

            return response;
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

    public class EventOutput
    {
        [EventHubOutput("%EventHubName%", Connection = "EventHubConnectionString")]
        public EventData[] Events { get; set; }

        public HttpResponseData HttpResponse { get; set; }
    }

}
