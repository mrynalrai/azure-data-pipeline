using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Azure.Storage.Blobs;
using System.Data.SqlTypes;
using Azure.Storage.Blobs.Models;
using System.IO.Compression;
using System.Text;

namespace HttpFunctionApp
{
    public class HttpFunctionApp
    {
        [FunctionName("HttpFunctionApp")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string tenantContainerSasString = Environment.GetEnvironmentVariable("ContainerSasUrl");
            Uri tenantContainerSasUri;

            if (Uri.TryCreate(tenantContainerSasString, UriKind.Absolute, out tenantContainerSasUri))
            {
                Console.WriteLine("Uri created successfully: " + tenantContainerSasUri);
            }
            else
            {
                // Uri creation failed
                return new ObjectResult(new { error = "InternalError", message = "Invalid Uri Detected" })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
            BlobContainerClient tenantContainerClient = new BlobContainerClient(tenantContainerSasUri);

            string csvContent = "Name, Age\nJohn, 30\nAlice, 25";

            try
            {
                // Create a MemoryStream to store the CSV content
                using (MemoryStream csvStream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent)))
                {
                    // Create a MemoryStream to store the zipped content
                    using (MemoryStream zipStream = new MemoryStream())
                    {
                        // Create a ZipArchive to write to the zip stream
                        using (ZipArchive zip = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
                        {
                            // Add the CSV file to the zip archive
                            var entry = zip.CreateEntry("dummy.csv", CompressionLevel.Optimal);
                            using (Stream entryStream = entry.Open())
                            {
                                await csvStream.CopyToAsync(entryStream);
                            }
                        }

                        // Reset the position of the zip stream to the beginning
                        zipStream.Seek(0, SeekOrigin.Begin);

                        // Upload the zipped content to Azure Blob Storage
                        // var options = new BlockBlobOpenWriteOptions { Overwrite = overwrite };
                        var blobClient = tenantContainerClient.GetBlobClient("sample.zip");
                        using (var blobStream = await blobClient.OpenWriteAsync(true))
                        {
                            await zipStream.CopyToAsync(blobStream);
                        }
                    }
                }
            } catch (Exception ex)
            {
                log.LogError(ex.Message);
                return new ObjectResult(new { error = "InternalError", message = ex.Message })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
            

            return new OkObjectResult("Success");
        }
    }
}
