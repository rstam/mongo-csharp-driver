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

using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Linq3Implementation.Ast;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Stages;
using MongoDB.Driver.Linq.Linq3Implementation.Serializers;
using MongoDB.Driver.MqlBuilder.Translators.Context;
using MongoDB.Driver.MqlBuilder.Translators.ExpressionToAggregationExpressionTranslators;

namespace MongoDB.Driver.MqlBuilder.Translators.ExpressionToFilterTranslators
{
    internal static class MqlProjectStageTranslator
    {
        public static AstStage Translate(IMqlProjectStage stage, IBsonSerializer inputSerializer, out IBsonSerializer outputSerializer)
        {
            var context = MqlTranslationContext.Create();
            var projection = stage.Projection;
            var parameter = projection.Parameters.Single();
            var rootSymbol = context.CreateRootSymbol(parameter, inputSerializer);
            context = context.WithSymbol(rootSymbol);
            var aggregationExpression = MqlExpressionToAggregationExpressionTranslator.Translate(context, projection.Body);
            var projectSpecifications = ToProjectSpecifications(aggregationExpression, out outputSerializer);
            return AstStage.Project(projectSpecifications);
        }

        private static AstProjectStageSpecification[] ToProjectSpecifications(MqlAggregationExpression aggregationExpression, out IBsonSerializer projectSerializer)
        {
            var projectSpecifications = new List<AstProjectStageSpecification>();
            if (aggregationExpression.Ast is AstComputedDocumentExpression computedDocument)
            {
                var idWasProjected = false;
                foreach (var computedField in computedDocument.Fields)
                {
                    var projectSpecification = ToProjectSpecification(computedField);
                    projectSpecifications.Add(projectSpecification);
                    idWasProjected |= computedField.Path == "_id";
                }
                if (!idWasProjected)
                {
                    projectSpecifications.Add(AstProject.Exclude("_id"));
                }
                projectSerializer = aggregationExpression.Serializer;
            }
            else
            {
                projectSpecifications.Add(AstProject.Set("_v", aggregationExpression.Ast));
                projectSpecifications.Add(AstProject.Exclude("_id"));
                projectSerializer = WrappedValueSerializer.Create("_v", aggregationExpression.Serializer);
            }

            return projectSpecifications.ToArray();
        }

        private static AstProjectStageSpecification ToProjectSpecification(AstComputedField field)
        {
            if (field.Value is AstGetFieldExpression getFieldExpression &&
                getFieldExpression.Input is AstVarExpression inputVarExpression &&
                inputVarExpression.Name == "ROOT" &&
                getFieldExpression.FieldName is AstConstantExpression constantFieldName &&
                constantFieldName.Value is BsonString fieldName &&
                field.Path == fieldName.Value)

            {
                return AstProject.Include(field.Path);
            }

            if (field.Value is AstConstantExpression constantValueExpression &&
                ValueNeedsToBeQuoted(constantValueExpression.Value))
            {
                var quotedValue = AstExpression.Literal(constantValueExpression.Value);
                return AstProject.Set(field.Path, quotedValue);
            }

            return AstProject.Set(field.Path, field.Value);
        }

        private static bool ValueNeedsToBeQuoted(BsonValue value)
        {
            return false;
        }
    }
}
