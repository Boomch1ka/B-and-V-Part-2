using System;
using System.IO;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;

namespace B_and_V_Part_2.Services
{
    public interface IBlobService
    {
        Task<string> UploadAsync(Stream fileStream, string fileName, string contentType = null);
    }

    public class BlobService : IBlobService
    {
        private readonly BlobContainerClient _container;

        public BlobService(IConfiguration configuration)
        {
            var conn = configuration.GetValue<string>("BlobStorage:ConnectionString");
            var containerName = configuration.GetValue<string>("BlobStorage:Container") ?? "images";
            var client = new BlobServiceClient(conn);
            _container = client.GetBlobContainerClient(containerName);
            _container.CreateIfNotExists(PublicAccessType.Blob);
        }

        public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType = null)
        {
            var blobClient = _container.GetBlobClient(fileName);
            var headers = new BlobHttpHeaders();
            if (!string.IsNullOrEmpty(contentType)) headers.ContentType = contentType;
            await blobClient.UploadAsync(fileStream, new BlobUploadOptions { HttpHeaders = headers });
            return blobClient.Uri.ToString();
        }
    }
}
