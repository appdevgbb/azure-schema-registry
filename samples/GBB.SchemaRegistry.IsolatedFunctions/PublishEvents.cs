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
        private static readonly SchemaRegistryClient _schemaRegistryClient = InitializeSchemaRegistryClient();
        private static readonly string _schemaGroup = Environment.GetEnvironmentVariable("SchemaGroup");

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

            // Create an instance of the avro encoder and set the auto flag
            // to true in case the schema does not exists in the registry.
            var encoder = new SchemaRegistryAvroEncoder(
                _schemaRegistryClient,
                _schemaGroup,
                new SchemaRegistryAvroObjectEncoderOptions { AutoRegisterSchemas = true });

            // Create a customer loyalty record
            var loyalty = new CustomerLoyalty
            {
                CustomerId = 1,
                Description = "10 points added",
                PointsAdded = 10,
            };

            // Encode the loyalty record using the schema registry
            EventData encodedLoyalty = await encoder.EncodeMessageDataAsync<EventData>(loyalty);

            // Publish to Event Hubs
            // Note: Having issues with multiple output bindings and this working
            // property. For now, let's just use the Event Hubs SDK directly.
            var connectionString = Environment.GetEnvironmentVariable("EventHubConnectionString");
            var eventHubName = Environment.GetEnvironmentVariable("EventHubName");
            await using (var producer = new EventHubProducerClient(connectionString, eventHubName))
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

        private static SchemaRegistryClient InitializeSchemaRegistryClient()
        {
            // Instantiate a schema registry client with client secret credentials
            var schemaRegistryUrl = Environment.GetEnvironmentVariable("SchemaRegistryUrl");
            var tenantId = Environment.GetEnvironmentVariable("SchemaRegistryTenantId");
            var clientId = Environment.GetEnvironmentVariable("SchemaRegistryClientId");
            var clientSecret = Environment.GetEnvironmentVariable("SchemaRegistryClientSecret");

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);

            return new SchemaRegistryClient(schemaRegistryUrl, credential);
        }
    }

    public class EventOutput
    {
        [EventHubOutput("%EventHubName%", Connection = "EventHubConnectionString")]
        public EventData[] Events { get; set; }

        public HttpResponseData HttpResponse { get; set; }
    }

}
