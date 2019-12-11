// -----------------------------------------------------------------------
//  <copyright file="Program.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace DataShareSample
{
    using System;
    using System.IO;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.DataShare.Models;
    using Microsoft.Azure.Management.ResourceManager.Fluent;
    using Microsoft.Extensions.Configuration;

    public class Program
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("\r\n\r\nReading the configurations...");
            IConfigurationRoot configurationRoot = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory()).AddJsonFile("AppSettings.json").Build();
            var configuration = configurationRoot.GetSection("configs").Get<Configuration>();

            Console.WriteLine("\r\n\r\nIdempotent creates for provider resources...");
            var providerContext = new UserContext(configuration.Provider);
            IResourceGroup providerResourceGroup = providerContext.IdempotentCreateResourceGroup();
            Account providerAccount = providerContext.IdempotentCreateAccount();
            Share share = providerContext.IdempotentCreateShare();

            Console.WriteLine($"\r\n\r\nAssign MSI of {providerAccount.Id} as the Blob Reader on the Provider Storage...");
            await providerContext.AssignRoleTaskAsync(
                configuration.Provider,
                providerAccount.Identity.PrincipalId,
                "2a2b9908-6ea1-4ae2-8e65-a410df84e7d1");

            Console.WriteLine("\r\n\r\nCreate data set and send invitation");
            DataSet dataSet = providerContext.CreateIfNotExistDataSet(configuration.Provider);

            Invitation invitation = providerContext.CreateIfNotExistInvitation(configuration.Consumer);

            Console.WriteLine("\r\n\r\nIdempotent creates for consumer");
            var consumerContext = new UserContext(configuration.Consumer);
            IResourceGroup consumerResourceGroup = consumerContext.IdempotentCreateResourceGroup();
            Account consumerAccount = consumerContext.IdempotentCreateAccount();

            Console.WriteLine("\r\n\r\nTo accept the invitation create a share subscription/received share...");
            ShareSubscription shareSubscription = consumerContext.CreateIfNotExistShareSubscription(invitation);

            Console.WriteLine($"\r\n\r\nAssign MSI of {consumerAccount.Id} as the Blob Contributor on the consumer Storage...");
            await consumerContext.AssignRoleTaskAsync(
                configuration.Consumer,
                consumerAccount.Identity.PrincipalId,
                "ba92f5b4-2d11-453d-a403-e96b0029c9fe");

            Console.WriteLine("\r\n\r\nCreate data set mapping to setup storage for the consumer");
            ConsumerSourceDataSet consumerSourceDataSet = consumerContext.GetConsumerSourceDataSet();
            DataSetMapping dataSetMapping = consumerContext.CreateDataSetMapping(
                configuration.Consumer,
                consumerSourceDataSet);

            Console.WriteLine("\r\n\r\nInitiate a snapshot copy (duration depends on how large the data is)...");
            ShareSubscriptionSynchronization response = consumerContext.Synchronize();
            Console.WriteLine(
                $"Synchronization Status: {response.Status}. Check resource {consumerAccount.Id} on https://portal.azure.com for further details. \r\n\r\n Hit Enter to continue...");

            Console.ReadLine();
        }
    }
}