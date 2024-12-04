using Azure.Data.SchemaRegistry;
using Azure.Identity;
using Azure.Messaging.EventHubs;
using Azure.Messaging.EventHubs.Producer;
using Microsoft.Azure.Data.SchemaRegistry.ApacheAvro;
using System.Configuration;
using zohan.schemaregistry.events;

var schemaGroupName = ConfigurationManager.AppSettings["SCHEMA_GROUP"];
var eventHubsNamespace = ConfigurationManager.AppSettings["EH_NAMESPACE"];
var eventHubName = ConfigurationManager.AppSettings["EH_NAME"];

// Initialize schema registry client 
var client = new SchemaRegistryClient(
        ConfigurationManager.AppSettings["SCHEMA_REGISTRY_URL"],
        new DefaultAzureCredential());

// Create a schema registry avro serializer and set the auto register flag
var serializer = new SchemaRegistryAvroSerializer(client, schemaGroupName, new SchemaRegistryAvroSerializerOptions { AutoRegisterSchemas = true });

// Encode an instance of a customer loyalty record
var loyalty = new CustomerLoyalty { CustomerId = 1, Description = "100 points", PointsAdded = 100 };
EventData eventData = (EventData) await serializer.SerializeAsync(loyalty, messageType: typeof(EventData));

// Publish to Event Hubs
await using (var producer = new EventHubProducerClient(eventHubsNamespace, eventHubName, new DefaultAzureCredential()))
{
    using EventDataBatch eventBatch = await producer.CreateBatchAsync();

    if (!eventBatch.TryAdd(eventData))
    {
        throw new Exception($"Event could not be added");
    }

    await producer.SendAsync(eventBatch);
}
