// -----------------------------------------------------------------------
//  <copyright file="UserContext.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace DataShareSample
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Threading.Tasks;
    using Microsoft.Azure.Management.DataShare;
    using Microsoft.Azure.Management.DataShare.Models;
    using Microsoft.Azure.Management.Fluent;
    using Microsoft.Azure.Management.ResourceManager.Fluent;
    using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;
    using Microsoft.Rest;
    using Microsoft.Rest.Azure.Authentication;

    public class UserContext
    {
        private static readonly Bogus.Faker Faker = new Bogus.Faker();

        public IAzure AzureClient { get; }

        public DataShareManagementClient DataShareClient { get; }

        public ServiceClientCredentials ClientCredentials { get; }

        public AzureCredentials AzureCredentials { get; }

        public Principal Principal { get; }

        public UserContext(Principal principal)
        {
            this.Principal = principal;

            var loginSettings = new ActiveDirectoryServiceSettings
            {
                AuthenticationEndpoint = Configuration.AuthorizationEndpoint,
                TokenAudience = new Uri("https://management.core.windows.net/")
            };

            this.ClientCredentials = ApplicationTokenProvider.LoginSilentAsync(
                this.Principal.TenantId,
                new ClientCredential(this.Principal.ClientId, this.Principal.Secret),
                loginSettings).Result;

            this.AzureCredentials = SdkContext.AzureCredentialsFactory.FromServicePrincipal(
                this.Principal.ClientId,
                this.Principal.Secret,
                this.Principal.TenantId,
                AzureEnvironment.AzureGlobalCloud);

            this.AzureClient = Azure.Configure().Authenticate(this.AzureCredentials)
                .WithSubscription(principal.SubscriptionId);
            this.DataShareClient =
                new DataShareManagementClient(Configuration.ArmEndpoint, this.ClientCredentials)
                {
                    SubscriptionId = principal.SubscriptionId
                };

            if (string.IsNullOrWhiteSpace(this.Principal.DataShareResourceGroup))
            {
                this.Principal.DataShareResourceGroup = UserContext.GenerateName();
            }

            if (string.IsNullOrWhiteSpace(this.Principal.DataShareAccountName))
            {
                this.Principal.DataShareAccountName = UserContext.GenerateName();
            }

            if (string.IsNullOrWhiteSpace(this.Principal.DataShareShareName))
            {
                this.Principal.DataShareShareName = UserContext.GenerateName();
            }

            if (string.IsNullOrWhiteSpace(this.Principal.DataShareInvitation))
            {
                this.Principal.DataShareInvitation = UserContext.GenerateName();
            }

            if (string.IsNullOrWhiteSpace(this.Principal.DataShareShareSubscriptionName))
            {
                this.Principal.DataShareShareSubscriptionName = UserContext.GenerateName();
            }

            if (string.IsNullOrWhiteSpace(this.Principal.DataShareDataSetName))
            {
                this.Principal.DataShareDataSetName = UserContext.GenerateName();
            }

            if (string.IsNullOrWhiteSpace(this.Principal.DataShareDataSetMappingName))
            {
                this.Principal.DataShareDataSetMappingName = UserContext.GenerateName();
            }
        }

        public IResourceGroup IdempotentCreateResourceGroup()
        {
            IResourceGroup resourceGroup = this.AzureClient.ResourceGroups.Define(this.Principal.DataShareResourceGroup)
                .WithRegion(Configuration.Location).Create();
            Console.WriteLine($"\r\n\r\nCreated resource group {resourceGroup.Id}");

            return resourceGroup;
        }

        public Account IdempotentCreateAccount()
        {
            Console.WriteLine($"\r\n\r\nCreating data share account (can take up to 30 seconds)");

            var accountPayload = new Account
            {
                Location = Configuration.Location, Identity = new Identity { Type = "SystemAssigned" }
            };

            Account dataShareAccount = this.DataShareClient.Accounts.Create(
                this.Principal.DataShareResourceGroup,
                this.Principal.DataShareAccountName,
                accountPayload);

            Console.WriteLine($"\r\n\r\nCreated data share account {dataShareAccount.Id}");

            return dataShareAccount;
        }

        public Share IdempotentCreateShare()
        {
            var sharePayload = new Share { Terms = "Terms", Description = "Test Share", ShareKind = "CopyBased" };

            Share share = this.DataShareClient.Shares.Create(
                this.Principal.DataShareResourceGroup,
                this.Principal.DataShareAccountName,
                this.Principal.DataShareShareName,
                sharePayload);

            Console.WriteLine($"\r\n\r\nCreated share {share.Id}");

            return share;
        }

        public DataSet CreateIfNotExistDataSet(Principal principal)
        {
            DataSet dataSet;
            try
            {
                dataSet = this.DataShareClient.DataSets.Get(
                    this.Principal.DataShareResourceGroup,
                    this.Principal.DataShareAccountName,
                    this.Principal.DataShareShareName,
                    this.Principal.DataShareDataSetName);

                Console.WriteLine($"\r\n\r\nReturning existing data set {dataSet.Id}...");

                return dataSet;
            }
            catch (DataShareErrorException exception)
            {
                if (exception.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine("\r\n\r\nData set does not exist, creating new...");
                    var containerDataSetPayload = new BlobContainerDataSet
                    {
                        SubscriptionId = principal.SubscriptionId,
                        ResourceGroup = principal.StorageResourceGroup,
                        StorageAccountName = principal.StorageAccountName,
                        ContainerName = principal.StorageContainerName
                    };

                    dataSet = this.DataShareClient.DataSets.Create(
                        this.Principal.DataShareResourceGroup,
                        this.Principal.DataShareAccountName,
                        this.Principal.DataShareShareName,
                        this.Principal.DataShareDataSetName,
                        containerDataSetPayload);

                    Console.WriteLine($"\r\n\r\nReturning new data set {dataSet.Id}...");

                    return dataSet;
                }

                Console.WriteLine($"\r\n\r\nUnexpected error occured while creating data set - {exception.Body.Error.Message}");

                throw;
            }
        }

        public Invitation CreateIfNotExistInvitation(Principal consumer)
        {
            Invitation invitation;
            try
            {
                invitation = this.DataShareClient.Invitations.Get(
                    this.Principal.DataShareResourceGroup,
                    this.Principal.DataShareAccountName,
                    this.Principal.DataShareShareName,
                    this.Principal.DataShareInvitation);

                Console.WriteLine($"\r\n\r\nReturning existing invitation {invitation.Id}...");

                return invitation;
            }
            catch (DataShareErrorException exception)
            {
                if (exception.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine("\r\n\r\nInvitation does not exist, creating new...");
                    var invitationPayload = new Invitation
                    {
                        TargetActiveDirectoryId = consumer.TenantId, TargetObjectId = consumer.ObjectId
                    };

                    invitation = this.DataShareClient.Invitations.Create(
                        this.Principal.DataShareResourceGroup,
                        this.Principal.DataShareAccountName,
                        this.Principal.DataShareShareName,
                        this.Principal.DataShareInvitation,
                        invitationPayload);

                    Console.WriteLine($"\r\n\r\nReturning new invitation {invitation.Id}...");

                    return invitation;
                }

                Console.WriteLine($"\r\n\r\nUnexpected error occured while creating data set - {exception.Body.Error.Message}");

                throw;
            }
        }

        public ShareSubscription CreateIfNotExistShareSubscription(Invitation invitation)
        {
            if (invitation.InvitationStatus == "Accepted")
            {
                Console.WriteLine(
                    $"Invitation {invitation.InvitationId} is already accepted. Trying to get Share Subscription...");

                try
                {
                    ShareSubscription shareSubscription = this.DataShareClient.ShareSubscriptions.Get(
                        this.Principal.DataShareResourceGroup,
                        this.Principal.DataShareAccountName,
                        this.Principal.DataShareShareSubscriptionName);

                    Console.WriteLine($"\r\n\r\nFound share subscription {shareSubscription.Id}...");

                    if(shareSubscription.InvitationId !=  invitation.InvitationId)
                    {
                        Console.WriteLine(
                            $"{shareSubscription.Id} was not created for InvitationId {invitation.InvitationId}. Make sure the configuration is valid.");

                        throw new Exception("Invalid configuration.");
                    }

                    return shareSubscription;
                }
                catch (DataShareErrorException exception)
                {
                    if (exception.Response.StatusCode == HttpStatusCode.NotFound)
                    {
                        Console.WriteLine(
                            $"Share subscription {this.Principal.DataShareShareSubscriptionName} does not exist. Make sure the configuration is valid.");
                    }

                    throw;
                }
            }

            var shareSubscriptionPayload = new ShareSubscription { InvitationId = invitation.InvitationId };

            return this.DataShareClient.ShareSubscriptions.Create(
                this.Principal.DataShareResourceGroup,
                this.Principal.DataShareAccountName,
                this.Principal.DataShareShareSubscriptionName,
                shareSubscriptionPayload);
        }

        public ConsumerSourceDataSet GetConsumerSourceDataSet()
        {
            return this.DataShareClient.ConsumerSourceDataSets.ListByShareSubscription(
                this.Principal.DataShareResourceGroup,
                this.Principal.DataShareAccountName,
                this.Principal.DataShareShareSubscriptionName).First();
        }

        public DataSetMapping CreateDataSetMapping(Principal principal, ConsumerSourceDataSet consumerSourceDataSet)
        {
            DataSetMapping dataSetMapping;

            try
            {
                dataSetMapping = this.DataShareClient.DataSetMappings.Get(
                    this.Principal.DataShareResourceGroup,
                    this.Principal.DataShareAccountName,
                    this.Principal.DataShareShareSubscriptionName,
                    this.Principal.DataShareDataSetMappingName);

                Console.WriteLine("\r\n\r\nFound existing data set mapping.");

                return dataSetMapping;

            }
            catch(DataShareErrorException exception)
            {
                if(exception.Response.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine("\r\n\r\nData set mapping does not exist. Creating a new one...");

                    var dataSetMappingPayload = new BlobContainerDataSetMapping
                    {
                        DataSetId = consumerSourceDataSet.DataSetId,
                        StorageAccountName = principal.StorageAccountName,
                        ContainerName = principal.StorageContainerName,
                        SubscriptionId = principal.SubscriptionId,
                        ResourceGroup = principal.StorageResourceGroup
                    };

                    dataSetMapping = this.DataShareClient.DataSetMappings.Create(
                        this.Principal.DataShareResourceGroup,
                        this.Principal.DataShareAccountName,
                        this.Principal.DataShareShareSubscriptionName,
                        this.Principal.DataShareDataSetMappingName,
                        dataSetMappingPayload);

                    Console.WriteLine($"\r\n\r\nCreated data set mapping {dataSetMapping.Id}...");

                    return dataSetMapping;
                }

                throw;
            }
        }

        public OperationResponse Synchronize()
        {
            try
            {
                return this.DataShareClient.ShareSubscriptions.SynchronizeMethod(
                    this.Principal.DataShareResourceGroup,
                    this.Principal.DataShareAccountName,
                    this.Principal.DataShareShareSubscriptionName,
                    new Synchronize { SynchronizationMode = "FullSync" });
            }
            catch (Exception exception)
            {
                Console.WriteLine($"\r\n\r\nError in running snapshot copy - {exception.Message}");

                throw;
            }
        }

        public async Task AssignRoleTaskAsync(Principal principal, string msiId, string roleDefinition)
        {
            string storageResourceId =
                $"/subscriptions/{principal.SubscriptionId}/resourceGroups/{principal.StorageResourceGroup}/providers/Microsoft.Storage/storageAccounts/{principal.StorageAccountName}";
            string endpoint =
                $"{Configuration.ArmEndpoint}/{storageResourceId}/providers/Microsoft.Authorization/roleAssignments/{Guid.NewGuid()}?api-version=2018-09-01-preview";

            string jsonBody = @"{
              'properties': {
              'roleDefinitionId': '/subscriptions/{subscriptionId}/providers/Microsoft.Authorization/roleDefinitions/{role}',
               'principalId': '{principalId}'
              }
            }";

            jsonBody = jsonBody.Replace("{principalId}", msiId);
            jsonBody = jsonBody.Replace("{subscriptionId}", principal.SubscriptionId);
            jsonBody = jsonBody.Replace("{role}", roleDefinition);

            // Assign Role to MSI
            using (var httpClient = new HttpClient(new RetryHandler(new HttpClientHandler())))
            {
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", principal.GetProviderToken());
                HttpContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await httpClient.PutAsync(endpoint, content).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    string errorMessage = response.Content.ReadAsStringAsync().Result;
                    if (response.StatusCode == HttpStatusCode.Conflict && errorMessage.Contains(
                            "The role assignment already exists",
                            StringComparison.OrdinalIgnoreCase))
                    {
                        Console.WriteLine(
                            $"Role assignment with {roleDefinition} for DataShare account with msi {msiId} already exists.");

                        return;
                    }
                    else
                    {
                        Console.WriteLine(
                            $"The user principal with app id {this.Principal.ClientId} does not permissions to add role assignments on storage account {storageResourceId}. Please grant Owner permissions to the appid.");

                        throw new Exception(
                            "Unable to assign role to MSI - " + response.Content.ReadAsStringAsync().Result);
                    }
                }

                Console.WriteLine($"\r\n\r\nSuccessfully role definition for MSI {msiId} for role {roleDefinition}");
            }
        }

        private static string GenerateName(string prefix = "AdsSample", int length = 8)
        {
            return $"{prefix}{UserContext.Faker.Random.String2(length)}".ToLowerInvariant();
        }
    }
}