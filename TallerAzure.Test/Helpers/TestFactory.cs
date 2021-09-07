using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using TallerAzure.Common.Models;
using TallerAzure.Functions.Entities;

namespace TallerAzure.Test.Helpers
{
    public class TestFactory
    {
        public static TimeEntity GetTodoEntity()
        {
            return new TimeEntity
            {
                ETag = "*",
                PartitionKey = "TIME",
                RowKey = Guid.NewGuid().ToString(),
                EmployeeId = 1,
                Date = DateTime.UtcNow,
                IsConsolidate = false,
                Type = 0
            };
        }
        public static List<TimeEntity> GetWatchesEntities()
        {
            return new List<TimeEntity>();
        }
        public static DefaultHttpRequest CreateHttpRequest(Guid timeId, Time timeRequest)
        {
            string request = JsonConvert.SerializeObject(timeRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
                Path = $"/{timeId}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(Guid timeId)
        {
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Path = $"/{timeId}"
            };
        }

        public static DefaultHttpRequest CreateHttpRequest(Time timeRequest)
        {
            string request = JsonConvert.SerializeObject(timeRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request)
            };
        }
        public static DefaultHttpRequest CreateHttpRequest(TimeEntity timeEntityRequest)
        {
            string request = JsonConvert.SerializeObject(timeEntityRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request)
            };
        }

        public static DefaultHttpRequest CreateHttpRequest()
        {
            return new DefaultHttpRequest(new DefaultHttpContext());
        }

        public static Time GetTodoRequest()
        {
            return new Time
            {
                EmployeeId = 1,
                Date = DateTime.UtcNow,
                Type = 0,
                IsConsolidate = false
            };
        }
        public static Consolidate GetConsolidateRequest()
        {
            return new Consolidate
            {
                EmployeeId = 2,
                Date = Convert.ToDateTime("2021-09-03T05:00:00.0000000Z"),
                MinutesWork = 890
            };
        }
        public static DefaultHttpRequest CreateHttpRequest(Consolidate consolidateRequest)
        {
            string request = JsonConvert.SerializeObject(consolidateRequest);
            return new DefaultHttpRequest(new DefaultHttpContext())
            {
                Body = GenerateStreamFromString(request),
            };
        }
        public static Stream GenerateStreamFromString(string stringToConvert)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(stringToConvert);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        public static ILogger CreateLogger(LoggerTypes type = LoggerTypes.Null)
        {
            ILogger logger;
            if (type == LoggerTypes.List)
            {
                logger = new ListLogger();
            }
            else
            {
                logger = NullLoggerFactory.Instance.CreateLogger("Null Logger");
            }

            return logger;
        }
    }
}
