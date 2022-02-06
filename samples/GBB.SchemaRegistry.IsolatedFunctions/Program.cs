using Microsoft.Azure.Functions.Worker.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Threading.Tasks;

namespace GBB.SchemaRegistry.IsolatedFunctions
{
    public class Program
    {
        public static void Main()
        {
            var host = new HostBuilder()
                .ConfigureFunctionsWorkerDefaults()
                .Build();

            host.Run();
        }
    }
}

// References
// https://docs.microsoft.com/en-us/azure/azure-functions/dotnet-isolated-process-guide#start-up-and-configuration
// https://docs.microsoft.com/en-us/aspnet/core/fundamentals/configuration/?view=aspnetcore-5.0&preserve-view=true
// https://github.com/Azure/azure-functions-dotnet-worker/tree/main/samples/CustomMiddleware