using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Storage.Blobs;

namespace ElectronicNetCloudStorage
{
    public class Connection
    {
        public static BlobServiceClient GetBlobServiceClient()
        {
            string connectionString = Environment.GetEnvironmentVariable("SICECLOUDSTORAGE");
            BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
            return blobServiceClient;
        }

        public static string GetContainerName(string type)
        {
            string containerName = Environment.GetEnvironmentVariable("SICECLOUDSTORAGE" + type + "CONTAINER");
            return containerName;
        }

        public static string GetCloudStorageUrl()
        {
            string url = Environment.GetEnvironmentVariable("SICECLOUDSTORAGEURL");
            return url;
        }
    }
}
