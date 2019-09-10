// -----------------------------------------------------------------------
//  <copyright file="Principal.cs" company="Microsoft Corporation">
//      Copyright (C) Microsoft Corporation. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace DataShareSample
{
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    public class Principal
    {
        public string TenantId { get; set; }

        public string ClientId { get; set; }

        public string ObjectId { get; set; }

        public string Secret { get; set; }

        public string SubscriptionId { get; set; }

        public string DataShareResourceGroup { get; set; }

        public string DataShareAccountName { get; set; }

        public string DataShareShareName { get; set; }

        public string DataShareInvitation { get; set; }

        public string DataShareDataSetName { get; set; }

        public string DataShareShareSubscriptionName { get; set; }

        public string DataShareDataSetMappingName { get; set; }

        public string StorageResourceGroup { get; set; }

        public string StorageAccountName { get; set; }

        public string StorageContainerName { get; set; }

        public string StorageBlobName { get; set; }

        public string GetProviderToken()
        {
            var context = new AuthenticationContext(Configuration.AuthorizationEndpoint + this.TenantId);
            var clientCredential = new ClientCredential(this.ClientId, this.Secret);

            return context.AcquireTokenAsync(Configuration.ArmEndpoint.ToString(), clientCredential).Result.AccessToken;
        }
    }
}