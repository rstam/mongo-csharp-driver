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

using System.Linq.Expressions;
using MongoDB.Bson.Serialization;

namespace MongoDB.Driver.MqlBuilder.Translators.Misc
{
    internal static class MqlArrayHelper
    {
        public static IBsonSerializer GetItemSerializer(Expression expression, Expression containingExpression, IBsonSerializer serializer)
        {
            if (serializer is IBsonArraySerializer arraySerializer)
            {
                if (arraySerializer.TryGetItemSerializationInfo(out var itemSerializationInfo))
                {
                    return itemSerializationInfo.Serializer;
                }

                throw new MqlExpressionNotSupportedException(expression, containingExpression, because: $"the TryGetItemSerializationInfo method of the {serializer.GetType()} class returned false");
            }

            throw new MqlExpressionNotSupportedException(expression, containingExpression, because: $"the {serializer.GetType()} class does not implement IBsonArraySerializer");
        }
    }
}
