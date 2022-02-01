using Microsoft.Azure.Data.SchemaRegistry.ApacheAvro;
using Azure.Messaging.EventHubs.Consumer;
using Azure.Data.SchemaRegistry;
using Azure.Identity;
using zohan.schemaregistry.events;
using System.Configuration;

Console.WriteLine("Press key to start consuming events...");
Console.ReadKey();

await ReadEventsAsync();

static async Task ReadEventsAsync()
{
    var schemaGroupName = ConfigurationManager.AppSettings["SCHEMA_GROUP"];
    var connectionString = ConfigurationManager.AppSettings["EH_CONNECTION_STRING"];
    var eventHubName = ConfigurationManager.AppSettings["EH_NAME"];

    // Create token credentials
    var credential = new ClientSecretCredential(
        ConfigurationManager.AppSettings["SCHEMA_REGISTRY_TENANT_ID"],
        ConfigurationManager.AppSettings["SCHEMA_REGISTRY_CLIENT_ID"],
        ConfigurationManager.AppSettings["SCHEMA_REGISTRY_CLIENT_SECRET"]
       );

    // Initialize schema registry client 
    var client = new SchemaRegistryClient(
            ConfigurationManager.AppSettings["SCHEMA_REGISTRY_URL"],
            credential);

    // Create avro encoder instance
    var encoder = new SchemaRegistryAvroEncoder(
        client,
        schemaGroupName,
        new SchemaRegistryAvroObjectEncoderOptions { AutoRegisterSchemas = false });

    // Create a consumer client to Event Hubs using the default consumer group
    var consumerGroup = EventHubConsumerClient.DefaultConsumerGroupName;
    var consumer = new EventHubConsumerClient(
        consumerGroup,
        connectionString,
        eventHubName);

    try
    {
        using CancellationTokenSource cancellationSource = new CancellationTokenSource();
        cancellationSource.CancelAfter(TimeSpan.FromSeconds(45));

        int eventsRead = 0;
        int maximumEvents = 10;

        await foreach (PartitionEvent partitionEvent in consumer.ReadEventsAsync(
            startReadingAtEarliestEvent: false,
            cancellationToken: cancellationSource.Token))
        {
            // Decode the event into a customer loyalty record
            CustomerLoyalty deserialized = (CustomerLoyalty)await encoder.DecodeMessageDataAsync(partitionEvent.Data, typeof(CustomerLoyalty));
            Console.WriteLine(deserialized.CustomerId);
            Console.WriteLine(deserialized.Description);

            string readFromPartition = partitionEvent.Partition.PartitionId;
            byte[] eventBodyBytes = partitionEvent.Data.EventBody.ToArray();

            Console.WriteLine($"Read event of length { eventBodyBytes.Length } from { readFromPartition }");
            eventsRead++;

            if (eventsRead >= maximumEvents)
            {
                break;
            }
        }
    }
    catch (TaskCanceledException)
    {
        Console.WriteLine("Cancellation token signaled");
    }
    catch (Exception ex)
    {
        Console.WriteLine(ex.Message);
    }
    finally
    {
        await consumer.CloseAsync();
    }
}