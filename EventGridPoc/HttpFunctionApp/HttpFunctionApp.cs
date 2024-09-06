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
using System.Collections.Generic;

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
                log.LogInformation("Uri created successfully: " + tenantContainerSasUri);
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

            // string csvContent = "Name, Age\nJohn, 30\nAlice, 25";
            string txtContent = "Name; Age\nJohn; 30\nAlice; 25";
            // Generate large dataset
            string largeContent = GenerateLargeDataset(10000);
            try
            {
                // Create a MemoryStream to store the text content
                using (MemoryStream txtStream = new MemoryStream(Encoding.UTF8.GetBytes(largeContent)))
                {
                    // Create a MemoryStream to store the zipped content
                    using (MemoryStream zipStream = new MemoryStream())
                    {
                        // Create a ZipArchive to write to the zip stream
                        using (ZipArchive zip = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
                        {
                            // Add the text file to the zip archive
                            // var entry = zip.CreateEntry("dummy.csv", CompressionLevel.Optimal);
                            var entry = zip.CreateEntry("dummy.txt", CompressionLevel.Optimal);
                            using (Stream entryStream = entry.Open())
                            {
                                await txtStream.CopyToAsync(entryStream);
                            }
                        }

                        // Reset the position of the zip stream to the beginning
                        zipStream.Seek(0, SeekOrigin.Begin);

                        // Upload the zipped content to Azure Blob Storage
                        // var options = new BlockBlobOpenWriteOptions { Overwrite = overwrite };
                        var blobClient = tenantContainerClient.GetBlobClient("sample.zip");
                        // using (var blobStream = await blobClient.OpenWriteAsync(true))
                        // {
                        //     await zipStream.CopyToAsync(blobStream);
                        // }
                        // Set metadata dictionary
                        var metadata = new Dictionary<string, string>
                        {
                            { "tenantId", "30c665d7-f159-4825-860a-562ee9e1b64a" }
                        };

                        // Create BlobUploadOptions with metadata
                        var uploadOptions = new BlobUploadOptions
                        {
                            Metadata = metadata
                        };
                        await blobClient.UploadAsync(zipStream, uploadOptions);
                    }
                }
            } catch (Exception ex)
            {
                return new ObjectResult(new { error = "InternalError", message = ex.Message })
                {
                    StatusCode = StatusCodes.Status500InternalServerError
                };
            }
            

            return new OkObjectResult("Success");
        }
        
        static string GenerateLargeDataset(int numberOfRows)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Name; Age");

            string[] names = { "John", "Alice", "Bob", "Eve", "Jane", "Tom", "Sam", "Sue", "Joe", "Ann", "Jack", "Mary", "Bill", "Nina", "Chris", "Diana", "Paul", "Ruth", "Matt", "Liz", "Adam", "Rose", "Mark", "Mia", "Luke", "Beth", "Ryan", "Fay", "Phil", "Ivy", "Owen", "Amy", "Kyle", "Gina", "Sean", "Tara", "Ben", "Jill", "Eric", "Kara", "Todd", "Eva", "Glenn", "Maya", "Greg", "Jade", "Cody", "Lynn", "Max", "Ella", "Dean", "Cara", "Eli", "Leah", "Drew", "Mona", "Ross", "Nell", "Gabe", "Hope", "Leo", "Nora", "Wade", "Kate", "Reed", "Tina", "Vince", "Meg", "Kurt", "Sara", "Fred", "Lila", "Chad", "Kira", "Neil", "Ruby", "Dane", "Rita", "Zane", "Lana" };

            Random random = new Random();

            for (int i = 0; i < numberOfRows; i++)
            {
                string name = names[random.Next(names.Length)];
                int age = random.Next(18, 100);
                sb.AppendLine($"{name}; {age}");
            }

            return sb.ToString();
        }
    }
}
