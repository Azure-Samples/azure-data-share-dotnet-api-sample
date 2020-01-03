---
page_type: sample
languages:
- csharp
products:
- azure
extensions:
- platforms: dotnet
- service: Azure Data Share
description: This sample will give you an e2e experience of the whole work flow for data share. It should include creating data share, adding datasets, synchronizaition etc.
urlFragment: data-share

---

## Azure Data Share API Sample


<!-- 
Guidelines on README format: https://review.docs.microsoft.com/help/onboard/admin/samples/concepts/readme-template?branch=master

Guidance on onboarding samples to docs.microsoft.com/samples: https://review.docs.microsoft.com/help/onboard/admin/samples/process/onboarding?branch=master

Taxonomies for products and languages: https://review.docs.microsoft.com/new-hope/information-architecture/metadata/taxonomies?branch=master
-->

In this tutorial user will have an e2e experience of the whole work flow for data share. This should include creating data share, adding datasets, synchronizaition etc.

## Prerequisites

* **Azure subscription**. If you don't have a subscription, you can create a [free trial](http://azure.microsoft.com/pricing/free-trial/) account.
* **Azure Storage account**. You use the blob storage as **source** data store. If you don't have an Azure storage account, see the [Create a storage account](../storage/common/storage-create-storage-account.md#create-a-storage-account) article for steps to create one.
* **Azure Data Share account**. You use the data share to perform data sharing operations. If you don't have an Azure Data Share account, see the [Create an Azure Data Share Account](https://docs.microsoft.com/en-us/azure/data-share/share-your-data) article for steps to create one.
* **Permission to add role assignment to the storage account**. This is present in the Microsoft.Authorization/role assignments/write permission. This permission exists in the owner role.
* **Visual Studio** 2013, 2015, or 2017. The walkthrough in this article uses Visual Studio 2017.
* **Download and install [Azure .NET SDK](http://azure.microsoft.com/downloads/)**.
* **Create an application in Azure Active Directory** following [this instruction](../azure-resource-manager/resource-group-create-service-principal-portal.md#create-an-azure-active-directory-application). Make note of the following values that you use in later steps: **application ID**, **authentication key**, and **tenant ID**. Assign application to "**Contributor**" role by following instructions in the same article.

## Runnning the sample

1. In the **DataShareSample.csproj**, update the version to the most recent one, you can refer this to this [Azure Data Share Nuget package version](https://www.nuget.org/packages/Microsoft.Azure.Management.DataShare):

    ```
    <PackageReference Include="Microsoft.Azure.Management.DataShare" Version="1.0.0" />
    ```
2. Set values for variables in the **AppSetting.json** file, these values are supposed to be created following the patterns mentioned above in prerequisities: 

    ```json

    "configs": {
        "provider": {
            "tenantId": "",
            "clientId": "",
            "objectId": "",
            "secret": "",
            "subscriptionId": "",

            "dataShareResourceGroup": "",
            "dataShareAccountName": "",
            "dataShareShareName": "",
            "dataShareInvitation": "",
            "dataShareDataSetName": "",
            "dataShareDataSetMappingName": "",

            "storageResourceGroup": "",
            "storageAccountName": "",
            "storageContainerName": "",
            "storageBlobName": ""
          },
        "consumer": {
            "tenantId": "",
            "clientId": "",
            "objectId": "",
            "secret": "",
            "subscriptionId": "",

            "dataShareResourceGroup": "",
            "dataShareAccountName": "",
            "dataShareShareSubscriptionName": "",
            "dataShareInvitation": "",
            "dataShareDataSetName": "",
            "dataShareDataSetMappingName": "",

            "storageResourceGroup": "",
            "storageAccountName": "",
            "storageContainerName": "",
            "storageBlobName": ""
          }
      }

    ```

## See Also

For the further information you can refer to this [Tutorial](https://docs.microsoft.com/en-us/azure/data-share/share-your-data).

