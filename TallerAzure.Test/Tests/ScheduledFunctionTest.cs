using System;
using TallerAzure.Functions.Entities;
using TallerAzure.Functions.Functions;
using TallerAzure.Test.Helpers;
using Xunit;

namespace TallerAzure.Test.Tests
{
    public class ScheduledFunctionTest
    {
        [Fact]
        public void ScheduleFunction_Should_Log_Message()
        {

            //arrange
            ListLogger logger = (ListLogger)TestFactory.CreateLogger(LoggerTypes.List);
            MockCloudTableTimes<TimeEntity> timeTable = new MockCloudTableTimes<TimeEntity>(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            MockCloudTableTimes<ConsolidateEntity> consolidateTable = new MockCloudTableTimes<ConsolidateEntity>(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));


            //act
            ScheduleFunction.Run(null, timeTable, consolidateTable, logger);
            string message = logger.Logs[0];

            //assert
            Assert.Contains("Recieved a new consolidate", message);
        }
    }
}
