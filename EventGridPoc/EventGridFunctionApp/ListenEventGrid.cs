using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Azure.Messaging.EventGrid;
using System.IO;
using Azure.Storage.Blobs;
using System.IO.Compression;
using System.Collections.Generic;
using System.Globalization;
using ListenEventGrid.Models;
using CsvHelper;
using System.Linq;
using Newtonsoft.Json;

namespace ListenEventGrid
{
    public static class ListenEventGrid
    {
        [FunctionName("ListenEventGrid")]
        public static async Task Run(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            ILogger log)
        {
            log.LogInformation("C# EventGrid trigger function is processing a request...");
            log.LogInformation($"Received EventGrid event: {JsonConvert.SerializeObject(eventGridEvent)}");
            try
            {
                if (eventGridEvent.EventType == "Microsoft.Storage.BlobCreated")
                {
                    var blobUrl = (string)JObject.Parse(eventGridEvent.Data.ToString())["url"];
                    log.LogInformation($"Blob created event received. Blob URL: {blobUrl}");

                    // Create BlobContainerClient 
                    log.LogInformation("Creating Blob Container Client");
                    string tenantContainerSasString = Environment.GetEnvironmentVariable("ContainerSasUrl");
                    Uri tenantContainerSasUri;

                    if (Uri.TryCreate(tenantContainerSasString, UriKind.Absolute, out tenantContainerSasUri))
                    {
                        log.LogInformation("Uri created successfully: " + tenantContainerSasUri);
                    }
                    else
                    {
                        log.LogError("Invalid uri detected.");
                        return;
                    }
                    var tenantContainerClient = new BlobContainerClient(tenantContainerSasUri);

                    // Download the blob file
                    var blobName = GetBlobName(blobUrl);
                    log.LogInformation($"Downloading the blob file {blobName}... ");
                    // var blobClient = new BlobClient(new Uri(blobUrl));
                    var blobClient = tenantContainerClient.GetBlobClient(blobName);
                    var blobResponse = await blobClient.DownloadAsync();

                    // Create a temporary directory to store the extracted files
                    log.LogInformation($"Creating a temporary directory to store the extracted files...");
                    var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    Directory.CreateDirectory(tempDir);

                    // Extract the contents of the ZIP file
                    log.LogInformation($"Extracting the contents of the ZIP file...");
                    using (var zipArchive = new ZipArchive(blobResponse.Value.Content, ZipArchiveMode.Read))
                    {
                        // Process the first entry found in the ZIP archive
                        if (zipArchive.Entries.Count > 0)
                        {
                            var firstEntry = zipArchive.Entries[0];
                            if (!string.IsNullOrEmpty(firstEntry.Name))
                            {
                                // Extract the first file
                                var entryPath = Path.Combine(tempDir, firstEntry.FullName);
                                firstEntry.ExtractToFile(entryPath);
                            }
                        }
                    }

                    // Process the extracted CSV file(s)
                    log.LogInformation($"Processing the extracted CSV file(s)...");
                    foreach (var csvFilePath in Directory.GetFiles(tempDir, "*.csv"))
                    {
                        // Read and process the CSV file
                        using (var csvReader = new StreamReader(csvFilePath))
                        {
                            var taskDtoList = ReadCSV(csvReader);
                            // Perform further processing with the taskDtoList

                            Console.WriteLine("Showing csv's content: ");
                            foreach (var record in taskDtoList)
                            {
                                log.LogInformation($"{record.Name} | {record.Age}");
                            }
                        }
                    }

                    // Cleanup: Delete the temporary directory and its contents
                    log.LogInformation($"Deleting the temporary directory and its contents...");
                    Directory.Delete(tempDir, true);
                }
                else
                {
                    log.LogInformation($"Unsupported event type: {eventGridEvent.EventType}");
                }
            } catch (Exception ex)
            {
                log.LogError($"Failed to run {ex.Message}");
            }
        }

        public static IList<Info> ReadCSV(TextReader reader)
        {
            IList<Info> taskList = new List<Info>();
            Console.WriteLine("Reading from csv file generated by DataFetcher");
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                csv.Context.RegisterClassMap<CsvToPickUpMapper>();
                taskList = csv.GetRecords<Info>().ToList();
                Console.WriteLine("Reading from csv file completed.");

            }
            return taskList;
        }

        public static string GetBlobName(string blobUrl)
        {
            Uri uri = new Uri(blobUrl);
            string blobName = uri.Segments.Last();
            return blobName;
        }
    }
}
