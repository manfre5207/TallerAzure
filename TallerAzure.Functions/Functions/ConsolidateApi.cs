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
        ///******************************/
        ///************GETBYID***********/
        ///******************************/
        [FunctionName(nameof(GetAllByIdConsolidate))]
        public static async Task<IActionResult> GetAllByIdConsolidate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "consolidate/{id}")] HttpRequest req,
            [Table("consolidate", Connection = "AzureWebJobsStorage")] CloudTable consolidateTable,
            string id,
            ILogger log)
        {
            log.LogInformation("Get all times received.");

            string dateSearch = id + "T05:00:00.0000000Z";
            DateTime Date = Convert.ToDateTime(dateSearch);

            TableQuery<ConsolidateEntity> query = new TableQuery<ConsolidateEntity>();
            query.Where(TableQuery.GenerateFilterConditionForDate("Date", QueryComparisons.Equal, Date));
            TableQuerySegment<ConsolidateEntity> retrievedResults = await consolidateTable.ExecuteQuerySegmentedAsync(query, null);

            string message = "Retrieved all times.";
            log.LogInformation(message);

            return new OkObjectResult(new Response
            {
                IsSuccess = true,
                Message = message,
                Result = retrievedResults
            });
        }
    }
}
