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
    public static class TimeApi
    {
        /******************************/
        /************CREATE************/
        /******************************/
        [FunctionName(nameof(CreateTime))]
        public static async Task<IActionResult> CreateTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "time")] HttpRequest req,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            ILogger log)
        {
            log.LogInformation("Recieved a new time.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Time time = JsonConvert.DeserializeObject<Time>(requestBody);


            //TableQuery<TimeEntity> query = new TableQuery<TimeEntity>();
            //TableQuerySegment<TimeEntity> allTimes = await timeTable.ExecuteQuerySegmentedAsync(query, null);


            if (string.IsNullOrEmpty(time?.EmployeeId.ToString()))
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "The request must have a time."
                });
            }


            TimeEntity timeEntity = new TimeEntity
            {
                EmployeeId = time.EmployeeId,
                Date = time.Date,
                Type = time.Type,
                IsConsolidate = false,
                PartitionKey = "TIME",
                RowKey = Guid.NewGuid().ToString(),
                ETag = "*"

            };

            TableOperation addOperation = TableOperation.Insert(timeEntity);
            await timeTable.ExecuteAsync(addOperation);
            string message = "New time stored in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = timeEntity
            });
        }
        /******************************/
        /************UPDATE************/
        /******************************/
        [FunctionName(nameof(UpdateTime))]
        public static async Task<IActionResult> UpdateTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "put", Route = "time/{id}")] HttpRequest req,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Update for time: {id}, received.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            Time time = JsonConvert.DeserializeObject<Time>(requestBody);

            //Validate todo id

            TableOperation findOperation = TableOperation.Retrieve<TimeEntity>("TIME", id);
            TableResult findResult = await timeTable.ExecuteAsync(findOperation);
            if (findResult.Result == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Time not found."
                });
            }

            //Update todo
            TimeEntity timeEntity = (TimeEntity)findResult.Result;

            if (!string.IsNullOrEmpty(time.EmployeeId.ToString()))
            {
                timeEntity.Date = time.Date;
                timeEntity.Type = time.Type;
            }


            TableOperation addOperation = TableOperation.Replace(timeEntity);
            await timeTable.ExecuteAsync(addOperation);
            string message = $"Time: {id}, update in table";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = timeEntity
            });
        }
        ///******************************/
        ///************GETALL************/
        ///******************************/
        [FunctionName(nameof(GetAllTime))]
        public static async Task<IActionResult> GetAllTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "time")] HttpRequest req,
            [Table("TIME", Connection = "AzureWebJobsStorage")] CloudTable watchTable,
            ILogger log)
        {
            log.LogInformation("Getting all the times.");

            TableQuery<TimeEntity> query = new TableQuery<TimeEntity>();
            TableQuerySegment<TimeEntity> watches = await watchTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Retrieving all the times";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = watches
            });
        }

        ///******************************/
        ///************GETBYID************/
        ///******************************/
        [FunctionName(nameof(GetTimeById))]
        public static IActionResult GetTimeById(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "time/{id}")] HttpRequest req,
            [Table("time", "TIME", "{id}", Connection = "AzureWebJobsStorage")] TimeEntity timeEntity,
            string id,
            ILogger log)
        {
            log.LogInformation($"Get time by id: {id} received.");

            if (timeEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Time not found"
                });
            }

            string message = $"Time: {timeEntity.RowKey}, retreived.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = timeEntity
            });
        }
        ///******************************/
        ///************DELETE************/
        ///******************************/
        [FunctionName(nameof(DeleteTime))]
        public static async Task<IActionResult> DeleteTime(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "time/{id}")] HttpRequest req,
            [Table("time", "TIME", "{id}", Connection = "AzureWebJobsStorage")] TimeEntity timeEntity,
            [Table("time", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
            string id,
            ILogger log)
        {
            log.LogInformation($"Delete time: {id} received.");

            if (timeEntity == null)
            {
                return new BadRequestObjectResult(new Response
                {
                    IsSuccess = false,
                    Message = "Time not found"
                });
            }

            await timeTable.ExecuteAsync(TableOperation.Delete(timeEntity));
            string message = $"Time: {timeEntity.RowKey}, deleted.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = timeEntity
            });
        }
    }
}
