using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureKeyVault;

namespace ServerCore
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
       .ConfigureAppConfiguration((ctx, builder) =>
       {
           var keyVaultEndpoint = GetKeyVaultEndpoint();
           if (!string.IsNullOrEmpty(keyVaultEndpoint))
           {
               var azureServiceTokenProvider = new AzureServiceTokenProvider();
               var keyVaultClient = new KeyVaultClient(
                   new KeyVaultClient.AuthenticationCallback(
                       azureServiceTokenProvider.KeyVaultTokenCallback));
               builder.AddAzureKeyVault(
                   keyVaultEndpoint, keyVaultClient, new DefaultKeyVaultSecretManager());
           }
       }
    ).UseStartup<Startup>()
     .Build();

        private static string GetKeyVaultEndpoint() => "https://PuzzleServerTestKeyVault.vault.azure.net";
    }
}
