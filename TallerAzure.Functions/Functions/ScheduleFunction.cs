using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TallerAzure.Functions.Entities;

namespace TallerAzure.Functions.Functions
{
    public static class ScheduleFunction
    {
        ///******************************/
        ///*****TIMER CONSOLIDATE********/
        ///******************************/
        [FunctionName("ScheduleFunction")]
        public static async Task Run([TimerTrigger("*/59 * * * *")] TimerInfo myTimer,
        [Table("time", Connection = "AzureWebJobsStorage")] CloudTable timeTable,
        [Table("consolidate", Connection = "AzureWebJobsStorage")] CloudTable consolidateTable,
        ILogger log)
        {
            log.LogInformation($"Recieved a new consolidate");

            string filter = TableQuery.GenerateFilterConditionForBool("IsConsolidate", QueryComparisons.Equal, false);
            TableQuery<TimeEntity> query = new TableQuery<TimeEntity>().Where(filter);
            TableQuerySegment<TimeEntity> consolidateTime = await timeTable.ExecuteQuerySegmentedAsync(query, null);
           
            int Upd = 0;
            int Add = 0;

            List<IGrouping<int, TimeEntity>> groupConsolidate = consolidateTime
                .GroupBy(u => u.EmployeeId)
                .OrderBy(u => u.Key).ToList();
            foreach (IGrouping<int, TimeEntity> group in groupConsolidate)
            {
                TimeSpan minutes;
                double totalMinutes = 0;
                List<TimeEntity> listOrder = group.OrderBy(g => g.Date).ToList();
                int cant = listOrder.Count % 2 == 0 ? listOrder.Count : listOrder.Count - 1;
                TimeEntity[] timeArray = listOrder.ToArray();
                try
                {
                    for (int i = 0; i < cant; i++)
                    {
                        await SetIsConsolidateAsync(timeArray[i].RowKey, timeTable);
                        if (i % 2 != 0 && timeArray.Length > 1)
                        {
                            minutes = timeArray[i].Date - timeArray[i - 1].Date;
                            totalMinutes += minutes.TotalMinutes;
                            TableQuerySegment<ConsolidateEntity> allConsolidate = await consolidateTable.ExecuteQuerySegmentedAsync(new TableQuery<ConsolidateEntity>(), null);
                            IEnumerable<ConsolidateEntity> Consolidate = allConsolidate
                                .Where(x => x.EmployeeId == timeArray[i].EmployeeId);
                            if (Consolidate == null || Consolidate.Count() == 0)
                            {
                                ConsolidateEntity consolidate = new ConsolidateEntity
                                {
                                    ETag = "*",
                                    PartitionKey = "CONSOLIDATE",
                                    RowKey = timeArray[i].RowKey,
                                    EmployeeId = timeArray[i].EmployeeId,
                                    Date = DateTime.Today,
                                    MinutesWork = (int)totalMinutes                                  
                                };
                                TableOperation addOperation = TableOperation.Insert(consolidate);
                                await consolidateTable.ExecuteAsync(addOperation);
                                Add++;
                            }
                            else
                            {
                                TableOperation consolidateOperation = TableOperation.Retrieve<ConsolidateEntity>("CONSOLIDATE", Consolidate.First().RowKey);
                                TableResult findRes = await consolidateTable.ExecuteAsync(consolidateOperation);
                                ConsolidateEntity consolidateEntity = (ConsolidateEntity)findRes.Result;
                                consolidateEntity.MinutesWork += Consolidate.First().MinutesWork;
                                TableOperation addOperation = TableOperation.Replace(consolidateEntity);
                                await consolidateTable.ExecuteAsync(addOperation);
                                Upd++;
                            }
                        }
                    }
                }
                catch (Exception error)
                {
                    string errorMessage = error.Message;
                    throw;
                }
            }
            log.LogInformation($"New data saved : {Add} \n Data updated : {Upd}.");

        }

        private static async Task SetIsConsolidateAsync(string id, CloudTable timeTable)
        {
            TableOperation timeOperation = TableOperation.Retrieve<TimeEntity>("TIME", id);
            TableResult findResult = await timeTable.ExecuteAsync(timeOperation);
            TimeEntity timeEntity = (TimeEntity)findResult.Result;
            timeEntity.IsConsolidate = true;
            TableOperation addOperation = TableOperation.Replace(timeEntity);
            await timeTable.ExecuteAsync(addOperation);
        }
    }
}


