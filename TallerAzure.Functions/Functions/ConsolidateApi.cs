using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Threading.Tasks;
using TallerAzure.Common.Models;
using TallerAzure.Common.Responses;
using TallerAzure.Functions.Entities;

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
            [Table("consolidate", Connection = "AzureWebJobsStorage")] CloudTable consolidateTable,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            ILogger log)
        {
            log.LogInformation("Recieved a new consolidate.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Consolidate consolidate = JsonConvert.DeserializeObject<Consolidate>(requestBody);


            ConsolidateEntity consolidateEntity = new ConsolidateEntity();


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


            double partial = 0;
            double totalminu = 0;

            foreach (TimeEntity allTime in allTimes)
            {

                if (allTime.IsConsolidate.Equals(false))
                {
                    var Entry = new DateTime();
                    var Exit = new DateTime();
                    if ((allTime.EmployeeId.Equals(consolidate.EmployeeId)))
                    {
                        if (allTime.Type == 0)
                        {
                            Entry = allTime.Date;
                        }
                        if (allTime.Type == 1)
                        {
                            Exit = allTime.Date;
                        }
                    }
                    else
                    {
                        return new BadRequestObjectResult(new Response
                        {
                            IsSuccess = false,
                            Message = "Time not found."
                        });
                    }
                    var minutes = (Exit - Entry).TotalMinutes;
                    partial += minutes;

                    TableOperation findOperation = TableOperation.Retrieve<TimeEntity>("TIME", allTime.RowKey);
                    TableResult findResult = await timeTable.ExecuteAsync(findOperation);
                    //Update todo
                    TimeEntity timeEntity = (TimeEntity)findResult.Result;

                    if (!string.IsNullOrEmpty(allTime.EmployeeId.ToString()))
                    {
                        timeEntity.IsConsolidate = true;
                    }
                    TableOperation addOperationUpdate = TableOperation.Replace(timeEntity);
                    await timeTable.ExecuteAsync(addOperationUpdate);
                }
            }

                
            totalminu = totalminu + partial;
            consolidateEntity = new ConsolidateEntity
            {
                EmployeeId = consolidate.EmployeeId,
                Date = DateTime.UtcNow,
                MinutesWork = totalminu,
                PartitionKey = "CONSOLIDATE",
                RowKey = Guid.NewGuid().ToString(),
                ETag = "*"
            };

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
