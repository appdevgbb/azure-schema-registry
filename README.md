# Azure Schema Registry samples

The [Azure Schema Registry](https://docs.microsoft.com/azure/event-hubs/schema-registry-overview) provides a repository for developers that wish to store, define and enforce schemas in their distributed applications and services.

![Azure Schema Registry](https://docs.microsoft.com/azure/event-hubs/media/schema-registry-overview/schema-registry.svg)

These samples demonstrate how to use the schema registry to encode and decode events. The samples also show how to use the schema registry with [Azure Event Hubs](https://docs.microsoft.com/azure/event-hubs/event-hubs-about) when producing and consuming events.

## Prerequisites

The following prerequisites are needed to run the samples in this repository:

- Azure Event Hubs namespace: [Create an Event Hubs namespace and an event hub](https://docs.microsoft.com/azure/event-hubs/event-hubs-create)
- Schema registry: [Create an Azure Event Hubs schema registry](https://docs.microsoft.com/azure/event-hubs/create-schema-registry)
- Role assignments: Included samples use the [DefaultAzureCredential](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme#defaultazurecredential) for all Azure SDK clients and Azure Function bindings. Your [local developer account](https://docs.microsoft.com/en-us/dotnet/api/overview/azure/identity-readme#authenticate-the-client), [application registration(s)](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app), and/or [Azure Function system-assigned managed identities](https://docs.microsoft.com/en-us/azure/app-service/overview-managed-identity?tabs=portal%2Chttp#add-a-system-assigned-identity) will all need to be assigned the following roles:
  - [Schema Registry Contributor](https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#schema-registry-contributor-preview)
  - [Azure Event Hubs Data Receiver](https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#azure-event-hubs-data-receiver)
  - [Azure Event Hubs Data Sender](https://docs.microsoft.com/en-us/azure/role-based-access-control/built-in-roles#azure-event-hubs-data-sender)

  
## Console application settings

Settings for both the consumer and producer applications are stored in a local `App.config` file:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration>
   <appSettings>
     <add key="EH_NAMESPACE" value="{event-hubs-namespace-name}.servicebus.windows.net"/>
     <add key="EH_NAME" value="{event-hub-name}"/>
     <add key="SCHEMA_GROUP" value="{schema-group-name}"/>
     <add key="SCHEMA_REGISTRY_URL" value="{event-hubs-namespace-name}.servicebus.windows.net"/>
   </appSettings>
</configuration>
```

To specify the application identity for Azure SDK client credentials, add the following to the `<appSettings>` element:

```xml
     <add key="AZURE_TENANT_ID" value="{azure-tenant-id}"/>
     <add key="AZURE_CLIENT_ID" value="{application-client-id}"/>
     <add key="AZURE_CLIENT_SECRET" value="{application-client-secret}"/>
```

## Azure Functions settings

Function settings for `local.settings.json` when using Managed Identity or local developer account credentials:

```json
{
  "IsEncrypted": false,
  "Values": {
    "AzureWebJobsStorage": "UseDevelopmentStorage=true",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "EventHubsConnection__fullyQualifiedNamespace": "{event-hubs-connection-string}.servicebus.windows.net",
    "EventHubName": "{event-hub-name}",
    "SchemaGroup": "{schema-group-name}",
    "SchemaRegistryUrl": "{event-hubs-namespace-name}.servicebus.windows.net",
  }
}
```

To specify the application identity for Azure SDK client and Azure Function bindings credentials, add the following elements to the `Values` object:

```json
    "EventHubsConnection__tenantId": "{azure-tenant-id}",
    "EventHubsConnection__clientId": "{application-client-id}",
    "EventHubsConnection__clientSecret": "{application-client-secret}",
    "AZURE_TENANT_ID": "{azure-tenant-id}",
    "AZURE_CLIENT_ID": "{application-client-id}",
    "AZURE_CLIENT_SECRET": "{application-client-secret}"
```

### Azure Functions in an isolated process

Functions that run in an [isolated process](https://docs.microsoft.com/azure/azure-functions/dotnet-isolated-process-guide) have the same settings except for the worker runtime value:

`"FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated"`

## References

- [Azure schema registry overview](https://docs.microsoft.com/azure/event-hubs/schema-registry-overview)
- [Azure Functions isolated process guide](https://docs.microsoft.com/azure/azure-functions/dotnet-isolated-process-guide)
