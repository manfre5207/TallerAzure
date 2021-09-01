using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TallerAzure.Common.Responses;
using Microsoft.WindowsAzure.Storage.Table;
using TallerAzure.Functions.Entities;
using TallerAzure.Common.Models;

namespace TallerAzure.Functions.Functions
{
    public static class ConsolidateApi
    {
        /******************************/
        /************CREATE************/
        /******************************/
        [FunctionName(nameof(ConsolidateProcess))]
        public static async Task<IActionResult> ConsolidateProcess(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "consolidate")] HttpRequest req,
            [Table("consolidate", Connection = "AzureWebJobsStorage")] CloudTable consolidateTable, CloudTable timeTable,
            ILogger log)
        {
            log.LogInformation("Recieved a new consolidate.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Consolidate consolidate = JsonConvert.DeserializeObject<Consolidate>(requestBody);




            TableQuery<TimeEntity> query = new TableQuery<TimeEntity>();
            TableQuerySegment<TimeEntity> allTimes = await timeTable.ExecuteQuerySegmentedAsync(query, null);


                        if (string.IsNullOrEmpty(consolidate?.EmployeeId.ToString()))
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "The request must have a consolidate."
                });
            }


            foreach (TimeEntity allTime in allTimes)
            {
                if ((allTime.EmployeeId.Equals(consolidate.EmployeeId)))
                {
                    DateTime Entry = allTime.Date;
                    DateTime Exit = allTime.Date;
                    if (allTime.Type == 0)
                    {
                        Entry = allTime.Date;
                    }
                    if (allTime.Type == 1)
                    {
                        Exit = allTime.Date;
                    }

                    TimeSpan ts = (Entry - Exit);
                    
                    ConsolidateEntity consolidateEntity = new ConsolidateEntity
                    {
                        EmployeeId = consolidate.EmployeeId,
                        Date = DateTime.UtcNow,
                        MinutesWork = ts.TotalMinutes,
                        PartitionKey = "CONSOLIDATE",
                        RowKey = Guid.NewGuid().ToString(),
                        ETag = "*"

                    };
                }
            }

            TableOperation addOperation = TableOperation.Insert(consolidateEntity);
            await consolidateTable.ExecuteAsync(addOperation);
            string message = "New consolidate stored in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = ""
            });
        }
    }
}
