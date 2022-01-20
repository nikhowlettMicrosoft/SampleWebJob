using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace AnE.ExP.Anarepo.Lib.Azure
{
    /// <summary>
    /// Azure Service calls are in this class. 
    /// Appropriate services (MetricMapping and others) get an instance of this class which they can use to make calls to azure blob store. 
    /// This class does instantiate CloudBlobClient in the constructor, so its difficult to unit test this class. 
    /// But since its only thin layer 'read' and 'write' from azure, we don't have to.
    /// </summary>
    public class AzureServiceBlobContainer
    {
        private readonly BlobContainerClient _azureBlobContainer;
        //CloudBlobContainer

        /// <inheritdoc />
        public AzureServiceBlobContainer(string connectionString, string containerName)
        {
            //BlobContainerClient container = new BlobContainerClient(connectionString, containerName);
            /*container = new BlobServiceClient(
                    new Uri($"https://{_options.AccountName}.blob.core.windows.net"),
                    new ManagedIdentityCredential());
            */
            BlobServiceClient client = new BlobServiceClient(
                    //new Uri(uriString: $"https://{connectionString}.blob.core.windows.net"),
                    new Uri($"https://testhipaa543.blob.core.windows.net"),
                    new DefaultAzureCredential());//new ManagedIdentityCredential());
            BlobContainerClient container = client.GetBlobContainerClient(containerName);
            _azureBlobContainer = container;
            container.CreateIfNotExists();
            var three = 3;
        }

        /// <inheritdoc />
        public async Task SaveData(string filePath, string data)
        {
            var blockBlob = _azureBlobContainer.GetBlobClient(filePath);
            await blockBlob.UploadAsync(data).ConfigureAwait(false);
        }

        /// <inheritdoc />
        public async Task<string> GetData(string filePath)
        {
            var blockBlob = _azureBlobContainer.GetBlobClient(filePath);
            if (!blockBlob.Exists()) return default(string);
            using (Stream s = new MemoryStream())
            {
                blockBlob.DownloadTo(s);
                s.Position = 0;
                var streamReader = new StreamReader(s);
                return streamReader.ReadToEnd();
            }
        }

        /// <inheritdoc />
        public async Task<IEnumerable<string>> GetBatchData(string dirPath, bool useFlatBlobListing = false, Func<BlobItem, bool> additionalFilter = null)
        {
            var listOfBlobs = _azureBlobContainer.GetBlobs();

            if (additionalFilter != null)
            {
                listOfBlobs = (global::Azure.Pageable<BlobItem>)listOfBlobs.Where<BlobItem>(additionalFilter as Func<BlobItem, bool>);
            }

            List<string> listOfData = new List<string>();
            foreach (var blockBlob in listOfBlobs)
            {
                listOfData.Add(await GetData(blockBlob.Name));
            }
            return listOfData;
        }

        /// <inheritdoc />
        public bool DoesBlobExist(string filePath)
        {
            var blockBlob = _azureBlobContainer.GetBlobClient(filePath);
            if (blockBlob.Exists()) return true;
            return false;
        }

        /// <inheritdoc />
        public async Task DeleteIntegrationTestData(string filePath)
        {
            if (!filePath.Contains("999999999"))
            {
                throw new Exception("Cannot delete data for arbitrary scorecard. Only integration test scorecards");
            }

            foreach (var blob in _azureBlobContainer.GetBlobs())
            {
                BlobClient blobClient = _azureBlobContainer.GetBlobClient(blob.Name);
                await blobClient.DeleteAsync();
            }
        }

        /// <inheritdoc />
        public async Task<bool> DeleteData(string filePath)
        {
            var blockBlob = _azureBlobContainer.GetBlobClient(filePath);
            return await DeleteBlob(blockBlob);
        }

        /// <inheritdoc />
        public async Task<bool> DeleteDataUnderPath(string storagePath)
        {
            var tasks = new List<Task<bool>>();

            foreach (BlobItem blob in _azureBlobContainer.GetBlobs())
            {
                if (blob.Name == storagePath)
                {
                    BlobClient blobClient = _azureBlobContainer.GetBlobClient(blob.Name);

                    tasks.Add(DeleteBlob(blobClient));
                }
            }

            var results = await Task.WhenAll(tasks);
            return results.All(res => res.Equals(true));
        }

        private async Task<bool> DeleteBlob(BlobClient blob)
        {
            _ = await blob.DeleteIfExistsAsync();
            var fileExists = await blob.ExistsAsync();
            return !fileExists;
        }
    }
}
