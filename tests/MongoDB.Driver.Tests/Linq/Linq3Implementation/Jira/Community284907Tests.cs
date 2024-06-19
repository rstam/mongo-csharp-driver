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
using System.Reflection;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver.Linq;
using MongoDB.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Tests.Linq.Linq3Implementation.Jira
{
    public class Community284907Tests : Linq3IntegrationTest
    {
        [Theory]
        [ParameterAttributeData]
        public void Where_should_work(
            [Values(LinqProvider.V2, LinqProvider.V3)] LinqProvider linqProvider)
        {
            var collection = GetCollection(linqProvider);

            var queryable = collection.AsQueryable()
                .Where(x => x.Age.Value == 40);

            if (linqProvider == LinqProvider.V2)
            {
                var exception = Record.Exception(() => Translate(collection, queryable));
                exception.Should().BeOfType<InvalidOperationException>();
            }
            else
            {
                var stages = Translate(collection, queryable);
                AssertStages(stages, "{ $match : { Age : 40 } }");
            }
        }

        private IMongoCollection<Person> GetCollection(LinqProvider linqProvider)
        {
            var collection = GetCollection<Person>("test", linqProvider);
            return collection;
        }

        private class Person
        {
            public ObjectId Id { get; set; }

            public string Name { get; set; }

            public Age Age { get; set; }
        }

        public abstract class PrimitiveValueObject<TValueObject, TValue>
        {
            public PrimitiveValueObject(TValue value) { Value = value; }
            public TValue Value { get; set; }
        }

        [BsonSerializer(typeof(PrimitiveValueObjectSerializer<Age, int>))]
        public sealed class Age : PrimitiveValueObject<Age, int>
        {
            public Age(int value) : base(value)
            {
            }
        }

        public sealed class PrimitiveValueObjectSerializer<TValueObject, TValue> : SerializerBase<TValueObject>, IClassRepresentedAsScalarSerializer
            where TValueObject : PrimitiveValueObject<TValueObject, TValue>
            where TValue : IComparable
        {
            public IBsonSerializer ScalarSerializer => BsonSerializer.LookupSerializer(typeof(TValue));

            /// <inheritdoc />
            public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TValueObject value)
            {
                if (value is null)
                {
                    context.Writer.WriteNull();
                }
                else
                {
                    BsonSerializer.Serialize(context.Writer, value.Value);
                }
            }

            /// <inheritdoc />
            public override TValueObject Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
            {
                if (context.Reader.CurrentBsonType == BsonType.Null)
                {
                    context.Reader.ReadNull();
                    return null;
                }

                TValue value = BsonSerializer.Deserialize<TValue>(context.Reader);
                object instance = Activator.CreateInstance(args.NominalType, BindingFlags.Public | BindingFlags.Instance, null, [value], null);
                return (TValueObject)instance;
            }
        }
    }
}
