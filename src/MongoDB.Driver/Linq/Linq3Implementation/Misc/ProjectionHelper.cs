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
using System.Linq;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.Linq.Linq3Implementation.Ast;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Optimizers;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Stages;
using MongoDB.Driver.Linq.Linq3Implementation.Serializers;
using MongoDB.Driver.Linq.Linq3Implementation.Translators.ExpressionToAggregationExpressionTranslators;

namespace MongoDB.Driver.Linq.Linq3Implementation.Misc
{
    internal static class ProjectionHelper
    {
        // public static methods
        public static (AstProjectStage, IBsonSerializer) CreateProjectStage(AggregationExpression expression)
        {
            var (specifications, projectionSerializer) = CreateAggregationProjection(expression);
            var projectStage = AstStage.Project(specifications);
            return (projectStage, projectionSerializer);
        }

        public static (List<AstProjectStageSpecification>, IBsonSerializer) CreateAggregationProjection(AggregationExpression expression)
        {
            return CreateProjection(expression, CreateAggregationSpecifications, new AstSimplifier());
        }

        public static (List<AstProjectStageSpecification>, IBsonSerializer) CreateFindProjection(AggregationExpression expression)
        {
            return CreateProjection(expression, CreateFindSpecifications, new AstFindProjectionSimplifier());
        }

        // private static methods
        private static (List<AstProjectStageSpecification>, IBsonSerializer) CreateComputedDocumentSpecifications(AggregationExpression expression)
        {
            var computedDocument = (AstComputedDocumentExpression)expression.Ast;

            var specifications = new List<AstProjectStageSpecification>();
            var isIdProjected = false;
            foreach (var computedField in computedDocument.Fields)
            {
                var path = computedField.Path;
                var value = QuoteIfNecessary(computedField.Value);
                specifications.Add(AstProject.Set(path, value));
                isIdProjected |= path == "_id";
            }
            if (!isIdProjected)
            {
                specifications.Add(AstProject.ExcludeId());
            }

            return (specifications, expression.Serializer);
        }

        private static (List<AstProjectStageSpecification>, IBsonSerializer) CreateAggregationSpecifications(AggregationExpression expression)
        {
            return expression.Ast.NodeType switch
            {
                AstNodeType.ComputedDocumentExpression => CreateComputedDocumentSpecifications(expression),
                _ => CreateWrappedValueSpecifications(expression)
            };
        }

        private static (List<AstProjectStageSpecification>, IBsonSerializer) CreateFindSpecifications(AggregationExpression expression)
        {
            return expression.Ast.NodeType switch
            {
                AstNodeType.ComputedDocumentExpression => CreateComputedDocumentSpecifications(expression),
                AstNodeType.GetFieldExpression => CreateWrappedFindFieldSpecifications(expression),
                _ => CreateWrappedValueSpecifications(expression)
            };
        }

        private static (List<AstProjectStageSpecification>, IBsonSerializer) CreateProjection(
            AggregationExpression expression,
            Func<AggregationExpression, (List<AstProjectStageSpecification>, IBsonSerializer)> specificationsCreator,
            AstSimplifier simplifier)
        {
            var (specifications, projectionSerializer) = specificationsCreator(expression);
            specifications = specifications.Select(specification => simplifier.VisitAndConvert(specification)).ToList();
            return (specifications, projectionSerializer);
        }

        private static (List<AstProjectStageSpecification>, IBsonSerializer) CreateWrappedFindFieldSpecifications(AggregationExpression expression)
        {
            var getFieldExpressionAst = (AstGetFieldExpression)expression.Ast;
            if (getFieldExpressionAst.HasSafeFieldName(out var fieldName))
            {
                var specifications = fieldName == "_id" ?
                    new List<AstProjectStageSpecification> { AstProject.Include(fieldName) } :
                    new List<AstProjectStageSpecification> { AstProject.Include(fieldName), AstProject.Exclude("_id") };

                var wrappedValueSerializer = WrappedValueSerializer.Create(fieldName, expression.Serializer);
                return (specifications, wrappedValueSerializer);
            }

            return CreateWrappedValueSpecifications(expression);
        }

        private static (List<AstProjectStageSpecification>, IBsonSerializer) CreateWrappedValueSpecifications(AggregationExpression expression)
        {
            var specifications = new List<AstProjectStageSpecification>
            {
                AstProject.Set("_v", QuoteIfNecessary(expression.Ast)),
                AstProject.ExcludeId()
            };
            var wrappedValueSerializer = WrappedValueSerializer.Create("_v", expression.Serializer);

            return (specifications, wrappedValueSerializer);
        }

        private static AstExpression QuoteIfNecessary(AstExpression expression)
        {
            if (expression is AstConstantExpression constantExpression)
            {
                if (ValueNeedsToBeQuoted(constantExpression.Value))
                {
                    return AstExpression.Literal(constantExpression);
                }
            }

            return expression;

            bool ValueNeedsToBeQuoted(BsonValue value)
            {
                switch (value.BsonType)
                {
                    case BsonType.Boolean:
                    case BsonType.Decimal128:
                    case BsonType.Double:
                    case BsonType.Int32:
                    case BsonType.Int64:
                        return true;

                    default:
                        return false;
                }
            }
        }
    }
}
