/* Copyright 2010-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using FluentAssertions;
using MongoDB.Driver.Linq;
using MongoDB.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Jira.CSharp4588
{
    public class CSharp4588Tests : Linq3IntegrationTest
    {
        private static DateTime __startTime = DateTime.Parse("2023-03-30T00:00:00Z", null, DateTimeStyles.AdjustToUniversal);
        private static DateTime __endTime = DateTime.Parse("2023-03-30T00:01:00Z", null, DateTimeStyles.AdjustToUniversal);
        private static TimeSpan __timeout = TimeSpan.FromMinutes(60);

        [Theory]
        [ParameterAttributeData]
        public void Client_side_projection_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = CreateCollection(linqProvider);
            var filter = Builders<ExecutionDocument>.Filter.Eq(x => x.Id, 1);

            List<ExecutionCommonDto> results;
            if (linqProvider == LinqProvider.V2)
            {
                var find = collection
                    .Find(filter)
                    .Project(execution => execution.ToExecutionCommonDto());

                results = find.ToList();
            }
            else
            {
                var find = collection
                    .Find(filter)
                    .ToEnumerable()
                    .Select(execution => execution.ToExecutionCommonDto());

                results = find.ToList();
            }

            var result = results.Single();
            result.Id.Should().Be(1);
            result.StartTime.Should().Be(__startTime);
            result.EndTime.Should().Be(__endTime);
            result.Timeout.Should().Be(__timeout);
            result.Status.Should().Be(Status.Done);
            result.Result.Should().Be("result");
        }

        private IMongoCollection<ExecutionDocument> CreateCollection(LinqProvider linqProvider)
        {
            var collection = GetCollection<ExecutionDocument>("test", linqProvider);
            CreateCollection(
                collection,
                new ExecutionDocument
                {
                    Id = 1,
                    Time = new Time
                    {
                        StartTime = __startTime,
                        EndTime = __endTime,
                        Timeout = __timeout
                    },
                    Status = new StatusClass { Status = Status.Done },
                    ExecutionResult = new ExecutionResult
                    {
                        Result = "result"
                    }
                });
            return collection;
        }

    }

    public class ExecutionDocument
    {
        public int Id { get; set; }
        public Time Time { get; set; }
        public StatusClass Status { get; set; }
        public ExecutionResult ExecutionResult { get; set; }
    }

    public class Time
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Timeout { get; set; }
    }

    public class StatusClass
    {
        public Status Status { get; set; }
    }

#pragma warning disable CA1717 // Only FlagsAttribute enums should have plural names
    public enum Status
    {
        Done
    }
#pragma warning restore CA1717 // Only FlagsAttribute enums should have plural names

    public class ExecutionResult
    {
        public string Result { get; set; }
    }

    public class ExecutionCommonDto
    {
        public int Id { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public TimeSpan? Timeout { get; set; }
        public Status? Status { get; set; }
        public string Result { get; set; }
    }

    public static class CSharp4588TestsExtensionMethods
    {
        public static ExecutionCommonDto ToExecutionCommonDto(this ExecutionDocument execution)
        {
            return new ExecutionCommonDto
            {
                Id = execution.Id,
                StartTime = execution.Time?.StartTime,
                EndTime = execution.Time?.EndTime,
                Timeout = execution.Time?.Timeout,
                Status = execution.Status?.Status,
                Result = execution.ExecutionResult?.Result
            };
        }
    }
}
