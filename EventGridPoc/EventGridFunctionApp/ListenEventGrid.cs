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
using CsvHelper.Configuration;
using ListenEventGrid.Services.InfoService;

namespace ListenEventGrid
{
    public class ListenEventGrid
    {
        private readonly IInfoService _infoService;

        public ListenEventGrid(
            IInfoService infoService
        )
        {
            this._infoService = infoService;
        }
        [FunctionName("ListenEventGrid")]
        public async Task Run(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            ILogger log)
        {
            log.LogInformation("C# EventGrid trigger function is processing a request...");
            log.LogInformation($"Received EventGrid event: {JsonConvert.SerializeObject(eventGridEvent)}");
            log.LogInformation($"Received EventGrid data: {eventGridEvent.Data}");
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

                    #region Metadata
                    // Fetch blob properties and metadata
                    var properties = await blobClient.GetPropertiesAsync();
                    var metadata = properties.Value.Metadata;

                    // Check if metadata contains 'tenantId' and log it
                    if (metadata.TryGetValue("tenantId", out string tenantId))
                    {
                        log.LogInformation($"Metadata tenantId: {tenantId}");
                    }
                    else
                    {
                        log.LogWarning("Metadata 'tenantId' not found.");
                    }
                    #endregion

                    // Create a temporary directory to store the extracted files
                    log.LogInformation($"Creating a temporary directory to store the extracted files...");
                    var tempDir = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    Directory.CreateDirectory(tempDir);

                    // Extract the contents of the ZIP file
                    log.LogInformation($"Extracting the contents of the ZIP file...");
                    using (var zipArchive = new ZipArchive(blobResponse.Value.Content, ZipArchiveMode.Read))
                    {
                        // Process the first entry found in the ZIP archive
                        // if (zipArchive.Entries.Count > 0)
                        foreach (var entry in zipArchive.Entries)
                        {
                            if (!string.IsNullOrEmpty(entry.Name))
                            {
                                // Extract the first file
                                var entryPath = Path.Combine(tempDir, entry.FullName);
                                entry.ExtractToFile(entryPath);
                            }
                        }
                    }

                    // Process the extracted CSV file(s)
                    /*
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
                    */
                    log.LogInformation($"Processing the extracted CSV file(s)...");
                    string[] txtFiles = Directory.GetFiles(tempDir, "dummy.txt");
                    string txtFilePath = txtFiles.FirstOrDefault();
                    IList<CreateInfoDto> infoDtoList = null;
                    if (txtFilePath != null)
                    {
                        // Read and process the CSV file
                        using (var csvReader = new StreamReader(txtFilePath))
                        {
                            infoDtoList = ReadCSV(csvReader);
                            // Perform further processing with the taskDtoList

                            Console.WriteLine("Showing csv's content: ");
                            foreach (var record in infoDtoList)
                            {
                                log.LogInformation($"{record.Name} | {record.Age}");
                            }
                        }
                    }

                    foreach (var infoDto in infoDtoList)
                    {
                        await _infoService.CreateInfo(infoDto, log);
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

        public static IList<CreateInfoDto> ReadCSV(TextReader reader)
        {
            IList<CreateInfoDto> taskList = new List<CreateInfoDto>();
            Console.WriteLine("Reading from csv file generated by DataFetcher");
            using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                Delimiter = ";", // Set the delimiter to ";"
            }))
            {
                csv.Context.RegisterClassMap<CsvToPickUpMapper>();
                taskList = csv.GetRecords<CreateInfoDto>().ToList();
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
