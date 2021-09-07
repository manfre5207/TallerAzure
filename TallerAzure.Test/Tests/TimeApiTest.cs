using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using TallerAzure.Common.Models;
using TallerAzure.Functions.Entities;
using TallerAzure.Functions.Functions;
using TallerAzure.Test.Helpers;
using Xunit;

namespace TallerAzure.Test.Tests
{
    public class TimeApiTest
    {
        private readonly ILogger logger = TestFactory.CreateLogger();

        /******************************/
        /******TEST CREATE TIME********/
        /******************************/
        [Fact]
        public async void CreateTime_Should_Return_200()
        {
            // Arrenge
            MockCloudTableTimes<TimeEntity> mockTimes = new MockCloudTableTimes<TimeEntity>(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Time todoRequest = TestFactory.GetTodoRequest();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(todoRequest);

            // Act
            IActionResult response = await TimeApi.CreateTime(request, mockTimes, logger);

            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
        /******************************/
        /****TEST UPDATE TIME**********/
        /******************************/
        [Fact]
        public async void UpdateTime_Should_Return_200()
        {
            // Arrenge
            MockCloudTableTimes<TimeEntity> mockTimes = new MockCloudTableTimes<TimeEntity>(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Time todoRequest = TestFactory.GetTodoRequest();
            Guid todoId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(todoId, todoRequest);

            // Act
            IActionResult response = await TimeApi.UpdateTime(request, mockTimes, todoId.ToString(), logger);

            // Assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
        /******************************/
        /****TEST GETBYID TIME*********/
        /******************************/
        [Fact]
        public async void GetTimeById_Should_Return_200()
        {
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
        /******************************/
        /****TEST GET ALL TIME*********/
        /******************************/
        [Fact]
        public async void GetAllTime_Should_Return_200()
        {
            //arrange
            MockCloudTableTimes<Time> mockTimes = new MockCloudTableTimes<Time>(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            DefaultHttpRequest request = TestFactory.CreateHttpRequest();

            //act
            IActionResult response = await TimeApi.GetAllTime(request, mockTimes, logger);

            //assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
        /******************************/
        /****TEST DELETE TIME**********/
        /******************************/
        [Fact]
        public async void DeleteTime_Should_Return_200()
        {
            //arrange
            MockCloudTableTimes<Time> mockTimes = new MockCloudTableTimes<Time>(new Uri("http://127.0.0.1:10002/devstoreaccount1/reports"));
            Time watchRequest = TestFactory.GetTodoRequest();
            TimeEntity watchEntity = TestFactory.GetTodoEntity();
            Guid watchId = Guid.NewGuid();
            DefaultHttpRequest request = TestFactory.CreateHttpRequest(watchRequest);

            //act
            IActionResult response = await TimeApi.DeleteTime(request, watchEntity, mockTimes, watchId.ToString(), logger);

            //assert
            OkObjectResult result = (OkObjectResult)response;
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
        }
    }
}
