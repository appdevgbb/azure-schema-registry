# Azure Schema Registry samples

The [Azure Schema Registry](https://docs.microsoft.com/azure/event-hubs/schema-registry-overview) provides a repository for developers that wish to store, define and enforce schemas in their distributed applications and services.

![Azure Schema Registry](https://docs.microsoft.com/azure/event-hubs/media/schema-registry-overview/schema-registry.svg)

These samples demonstrate how to use the schema registry to encode and decode events. The samples also show how to use the schema registry with [Azure Event Hubs](https://docs.microsoft.com/azure/event-hubs/event-hubs-about) when producing and consuming events.

## Prerequisites

The following prerequisites are needed to run the samples in this repository:

- Azure Event Hubs namespace: [Create an Event Hubs namespace and an event hub](https://docs.microsoft.com/azure/event-hubs/event-hubs-create)
- Schema registry: [Create an Azure Event Hubs schema registry](https://docs.microsoft.com/azure/event-hubs/create-schema-registry)
- App registrations: [Register an application with Azure Active Directory](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)
- Role assignments: [Assign schema registry roles for each application](https://docs.microsoft.com/azure/event-hubs/schema-registry-overview#azure-role-based-access-control)
  
## Console application settings

Settings for both the consumer and producer applications are stored in a local `App.config` file:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
   <appSettings>
     <add key="EH_CONNECTION_STRING" value="{event-hub-connection-string}"/>
     <add key="EH_NAME" value="{event-hub-name}"/>
     <add key="SCHEMA_GROUP" value="{schema-group-name}"/>
     <add key="SCHEMA_REGISTRY_URL" value="{event-hubs-namespace-name}.servicebus.window.net"/>
     <add key="SCHEMA_REGISTRY_TENANT_ID" value="{azure-tenant-id"/>
     <add key="SCHEMA_REGISTRY_CLIENT_ID" value="{application-client-id}"/>
     <add key="SCHEMA_REGISTRY_CLIENT_SECRET" value="{application-client-secret"/>
   </appSettings>
</configuration>
```

## Azure Functions settings

Function settings for `local.settings.json`:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "EventHubConnectionString": "{event-hub-connection-string}",
    "EventHubName": "{event-hub-name}",
    "SchemaGroup": "{schema-group-name}",
    "SchemaRegistryUrl": "{event-hubs-namespace-name}.servicebus.window.net",
    "SchemaRegistryTenantId": "{azure-tenant-id}",
    "SchemaRegistryClientId": "{application-client-id}",
    "SchemaRegistryClientSecret": "{application-client-secret}"
  }
}
```

### Azure Functions in an isolated process

Functions that run in an [isolated process](https://docs.microsoft.com/azure/azure-functions/dotnet-isolated-process-guide) have the same settings except for the worker runtime value:

`"FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"`

## References

- [Azure schema registry overview](https://docs.microsoft.com/azure/event-hubs/schema-registry-overview)
- [Azure Functions isolated process guide](https://docs.microsoft.com/azure/azure-functions/dotnet-isolated-process-guide)
