/* Copyright 2019-present MongoDB Inc.
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

using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDB.Bson.Serialization.Conventions
{
    /// <summary>
    /// A convention that sets representation of a string id class member to ObjectId in BSON with a StringObjectIdGenerator.
    /// </summary>
    public class StringIdStoredAsObjectIdConvention : ConventionBase, IMemberMapConvention
    {
        /// <inheritdoc/>
        public void Apply(BsonMemberMap memberMap)
        {
            if (memberMap != memberMap.ClassMap.IdMemberMap)
            {
                return;
            }

            if (memberMap.MemberType != typeof(string))
            {
                return;
            }

            if (memberMap.IdGenerator != null)
            {
                return;
            }

            var serializer = memberMap.GetSerializer();
            if (serializer is StringSerializer stringSerializer && stringSerializer.Representation == BsonType.String)
            {
                memberMap.SetSerializer(new StringSerializer(representation: BsonType.ObjectId));
                memberMap.SetIdGenerator(StringObjectIdGenerator.Instance);
            }
        }
    }
}
