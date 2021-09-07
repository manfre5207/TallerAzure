using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using TallerAzure.Functions.Entities;
using TallerAzure.Functions.Functions;
using TallerAzure.Test.Helpers;
using Xunit;

namespace TallerAzure.Test.Tests
{
    public class ConsolidateApiTest
    {
        [Fact]
        public async void GetConsolidateByDate_Should_Return_200()
        {
            ILogger logger = TestFactory.CreateLogger();
            //arrange
            MockCloudTableTimes<TimeEntity> mockTimes = new MockCloudTableTimes<TimeEntity>(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            TimeEntity timeEntity = TestFactory.GetTodoEntity();
            Guid todoId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest();

            //act
            IActionResult response = TimeApi.GetTimeById(request, timeEntity, todoId.ToString(), logger);

            //assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
    }
}
