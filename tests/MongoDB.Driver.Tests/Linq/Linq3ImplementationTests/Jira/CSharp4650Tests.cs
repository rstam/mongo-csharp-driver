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

using System.Linq;
using System;
using FluentAssertions;
using MongoDB.Driver.Linq;
using MongoDB.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3ImplementationTests.Jira
{
    public class CSharp4650Tests : Linq3IntegrationTest
    {
        [Theory]
        [ParameterAttributeData]
        public void Example_using_GetCarDescription_Works_using_car_collection_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCarCollection(linqProvider);

            var queryable = GetCarDescription_Works(collection.AsQueryable(), "5555XXX");

            var stages = Translate(collection, queryable);
            if (linqProvider == LinqProvider.V2)
            {
                AssertStages(
                    stages,
                    "{ $match : { _id : '5555XXX' } }",
                    "{  $project : { Description : '$Description', _id : 0 }}");
            }
            else
            {
                AssertStages(
                    stages,
                    "{ $match : { _id : '5555XXX' } }",
                    "{  $project : { _v : '$Description', _id : 0 }}");
            }

            var result = queryable.First();
            result.Should().Be("Car description");
        }

        [Theory]
        [ParameterAttributeData]
        public void Example_using_GetCarDescription_Works_using_deleted_car_collection_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetDeletedCarCollection(linqProvider);

            var queryable = GetCarDescription_Works(collection.AsQueryable(), "5555XXX");

            var stages = Translate(collection, queryable);
            if (linqProvider == LinqProvider.V2)
            {
                AssertStages(
                    stages,
                    "{ $match : { _id : '5555XXX' } }",
                    "{  $project : { Description : '$Description', _id : 0 }}");
            }
            else
            {
                AssertStages(
                    stages,
                    "{ $match : { _id : '5555XXX' } }",
                    "{  $project : { _v : '$Description', _id : 0 }}");
            }

            var result = queryable.First();
            result.Should().Be("Deleted car description");
        }

        [Theory]
        [ParameterAttributeData]
        public void Example_using_GetCarDescription_Fails_using_car_collection_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCarCollection(linqProvider);

            var queryable = GetCarDescription_Fails(collection.AsQueryable(), "5555XXX");

            var stages = Translate(collection, queryable);
            if (linqProvider == LinqProvider.V2)
            {
                AssertStages(
                    stages,
                    "{ $match : { _id : '5555XXX' } }",
                    "{  $project : { Description : '$Description', _id : 0 }}");
            }
            else
            {
                AssertStages(
                    stages,
                    "{ $match : { _id : '5555XXX' } }",
                    "{  $project : { _v : '$Description', _id : 0 }}");
            }

            var result = queryable.First();
            result.Should().Be("Car description");
        }

        [Theory]
        [ParameterAttributeData]
        public void Example_using_GetCarDescription_Fails_using_deleted_car_collection_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetDeletedCarCollection(linqProvider);

            var queryable = GetCarDescription_Fails(collection.AsQueryable(), "5555XXX");

            var stages = Translate(collection, queryable);
            if (linqProvider == LinqProvider.V2)
            {
                AssertStages(
                    stages,
                    "{ $match : { _id : '5555XXX' } }",
                    "{  $project : { Description : '$Description', _id : 0 }}");
            }
            else
            {
                AssertStages(
                    stages,
                    "{ $match : { _id : '5555XXX' } }",
                    "{  $project : { _v : '$Description', _id : 0 }}");
            }

            var result = queryable.First();
            result.Should().Be("Deleted car description");
        }

        private IQueryable<string> GetCarDescription_Works<T>(IQueryable<T> queryable, string licensePlate) where T : Car
        {
            return queryable.Where(d => d.Id == licensePlate).Select(d => d.Description);
        }

        private IQueryable<string> GetCarDescription_Fails(IQueryable<Car> queryable, string licensePlate)
        {
            return queryable.Where(d => d.Id == licensePlate).Select(d => d.Description);
        }

        private IMongoCollection<Car> GetCarCollection(LinqProvider linqProvider)
        {
            var collection = GetCollection<Car>("cars", linqProvider);
            CreateCollection(
                collection,
                new Car { Id = "5555XXX", Description = "Car description" });
            return collection;
        }

        private IMongoCollection<DeletedCar> GetDeletedCarCollection(LinqProvider linqProvider)
        {
            var collection = GetCollection<DeletedCar>("deletedcars", linqProvider);
            CreateCollection(
                collection,
                new DeletedCar { Id = "5555XXX", Description = "Deleted car description", DeletedDate = new DateTime(2023, 5, 12, 0, 0, 0, DateTimeKind.Utc) });
            return collection;
        }

        private class Car
        {
            public string Id { get; set; }
            public string Description { get; set; }
        }

        private class DeletedCar : Car
        {
            public DateTime DeletedDate { get; set; }
        }
    }
}
