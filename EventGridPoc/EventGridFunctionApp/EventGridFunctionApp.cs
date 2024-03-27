using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Azure.Messaging.EventGrid;

namespace EventGridFunctionApp
{
    public static class EventGridFunctionApp
    {
        [FunctionName("EventGridFunctionApp")]
        public static Task<IActionResult> Run(
            [EventGridTrigger] EventGridEvent eventGridEvent,
            ILogger log)
        {
            log.LogInformation("C# EventGrid trigger function processed a request.");

            try
            {
                // Check if the event type is a blob creation event
                if (eventGridEvent.EventType == "Microsoft.Storage.BlobCreated")
                {
                    // Extract the blob URL from the event data
                    string blobUrl = (string)JObject.Parse(eventGridEvent.Data.ToString())["url"];

                    // Write a meaningful message to the console
                    log.LogInformation($"Blob created event received. Blob URL: {blobUrl}");

                    // Optionally, you can perform additional processing here, such as downloading the blob content or triggering another workflow
                }
                else
                {
                    // Log a warning for unsupported event types
                    log.LogError($"Unsupported event type: {eventGridEvent.EventType}");
                }
            } catch (Exception ex)
            {
                log.LogError($"Failed to run {ex.Message}");
            }
            
            return Task.FromResult<IActionResult>(new OkObjectResult("Success"));
        }
    }
}
