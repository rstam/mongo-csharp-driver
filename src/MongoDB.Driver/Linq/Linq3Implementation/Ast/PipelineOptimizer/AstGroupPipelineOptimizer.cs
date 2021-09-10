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
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Expressions;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Filters;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Stages;
using MongoDB.Driver.Linq.Linq3Implementation.Ast.Visitors;

namespace MongoDB.Driver.Linq.Linq3Implementation.Ast.PipelineOptimizer
{
    internal class AstGroupPipelineOptimizer
    {
        private int _nextAccumulatorFieldNumber;
        private List<AstAccumulatorField> _newAccumulatorFields = new();

        public AstPipeline Optimize(AstPipeline pipeline)
        {
            for (var i = 0; i < pipeline.Stages.Count; i++)
            {
                var stage = pipeline.Stages[i];
                if (stage is AstGroupStage groupStage)
                {
                    pipeline = OptimizeGroupStage(pipeline, i, groupStage);
                }
            }

            return pipeline;
        }

        private void AddAccumulatorField(string name, AstAccumulatorExpression value)
        {
            _newAccumulatorFields.Add(AstExpression.AccumulatorField(name, value));
        }

        private string GetNextAccumulatorFieldName()
        {
            return $"__agg{_nextAccumulatorFieldNumber++}";
        }

        private AstPipeline OptimizeGroupStage(AstPipeline pipeline, int i, AstGroupStage groupStage)
        {
            if (IsOptimizableGroupStage(groupStage))
            {
                var followingStages = GetFollowingStagesToOptimize(pipeline, i + 1);
                if (followingStages == null)
                {
                    return pipeline;
                }

                var mappings = OptimizeGroupAndFollowingStages(groupStage, followingStages);
                if (mappings.Length > 0 && NoReferencesToElementsRemain(mappings))
                {
                    return (AstPipeline)AstNodeReplacer.Replace(pipeline, mappings);
                }
            }

            return pipeline;

            static bool IsOptimizableGroupStage(AstGroupStage groupStage)
            {
                // { $group : { _id : ?, _elements : { $push : "$$ROOT" } } }
                if (groupStage.Fields.Count == 1)
                {
                    var field = groupStage.Fields[0];
                    if (field.Path == "_elements" &&
                        field.Value.Operator == AstAccumulatorOperator.Push &&
                        field.Value.Arg is AstVarExpression varExpression &&
                        varExpression.Name == "ROOT")
                    {
                        return true;
                    }
                }

                return false;
            }

            static List<AstStage> GetFollowingStagesToOptimize(AstPipeline pipeline, int from)
            {
                var stages = new List<AstStage>();

                for (var j = from; j < pipeline.Stages.Count; j++)
                {
                    var stage = pipeline.Stages[j];
                    if (StageCanBeOptimized(stage))
                    {
                        stages.Add(stage);
                    }

                    if (IsLastStageThatCanBeOptimized(stage))
                    {
                        return stages;
                    }
                }

                return null;

                static bool StageCanBeOptimized(AstStage stage)
                {
                    return stage.NodeType switch
                    {
                        AstNodeType.LimitStage => true,
                        AstNodeType.MatchStage => true,
                        AstNodeType.ProjectStage => true,
                        AstNodeType.SampleStage => true,
                        AstNodeType.SkipStage => true,
                        _ => false
                    };
                }

                static bool IsLastStageThatCanBeOptimized(AstStage stage)
                {
                    return stage.NodeType switch
                    {
                        AstNodeType.ProjectStage => true,
                        _ => false
                    };
                }
            }

            static bool NoReferencesToElementsRemain((AstNode, AstNode)[] mappings)
            {
                return true; // TODO: figure out how to correctly determine that no references to _elements remain
            }
        }

        private (AstNode, AstNode)[] OptimizeGroupAndFollowingStages(AstGroupStage groupStage, List<AstStage> followingStages)
        {
            var mappings = new List<(AstNode, AstNode)>();

            foreach (var stage in followingStages)
            {
                var optimizedStage = OptimizeFollowingStage(stage);
                if (optimizedStage != stage)
                {
                    mappings.Add((stage, optimizedStage));
                }
            }

            if (_newAccumulatorFields.Count > 0)
            {
                var newGroupStage = AstStage.Group(groupStage.Id, _newAccumulatorFields);
                mappings.Add((groupStage, newGroupStage));
            }

            return mappings.ToArray();
        }

        private AstStage OptimizeFollowingStage(AstStage stage)
        {
            return stage switch
            {
                AstLimitStage limitStage => OptimizeLimitStage(limitStage),
                AstMatchStage matchStage => OptimizeMatchStage(matchStage),
                AstProjectStage projectStage => OptimizeProjectStage(projectStage),
                AstSampleStage sampleStage => OptimizeSampleStage(sampleStage),
                AstSkipStage skipStage => OptimizeSkipStage(skipStage),
                _ => throw new InvalidOperationException($"Unexpected node type: {stage.NodeType}.")
            };
        }

        private AstStage OptimizeLimitStage(AstLimitStage stage)
        {
            return stage;
        }

        private AstStage OptimizeMatchStage(AstMatchStage stage)
        {
            var optimizedFilter = AccumulatorMover.MoveAccumulators(this, stage.Filter);
            return stage.Update(optimizedFilter);
        }

        private AstStage OptimizeProjectStage(AstProjectStage stage)
        {
            var optimizedSpecifications = new List<AstProjectStageSpecification>();

            foreach (var specification in stage.Specifications)
            {
                var optimizedSpecification = OptimizeProjectStageSpecification(specification);
                optimizedSpecifications.Add(optimizedSpecification);
            }

            return stage.Update(optimizedSpecifications);
        }

        private AstProjectStageSpecification OptimizeProjectStageSpecification(AstProjectStageSpecification specification)
        {
            return specification switch
            {
                AstProjectStageSetFieldSpecification setFieldSpecification => OptimizeProjectStageSetFieldSpecification(setFieldSpecification),
                _ => specification
            };
        }

        private AstProjectStageSpecification OptimizeProjectStageSetFieldSpecification(AstProjectStageSetFieldSpecification specification)
        {
            var optimizedValue = AccumulatorMover.MoveAccumulators(this, specification.Value);
            return specification.Update(optimizedValue);
        }

        private AstStage OptimizeSampleStage(AstSampleStage stage)
        {
            return stage;
        }

        private AstStage OptimizeSkipStage(AstSkipStage stage)
        {
            return stage;
        }

        private class AccumulatorMover : AstNodeVisitor
        {
            #region static
            public static TNode MoveAccumulators<TNode>(AstGroupPipelineOptimizer optimizer, TNode node)
                where TNode : AstNode
            {
                var mover = new AccumulatorMover(optimizer);
                return mover.VisitAndConvert(node);
            }
            #endregion

            private AstGroupPipelineOptimizer _groupOptimizer;

            public AccumulatorMover(AstGroupPipelineOptimizer groupOptimizer)
            {
                _groupOptimizer = groupOptimizer;
            }

            public override AstNode VisitFilterField(AstFilterField node)
            {
                // "_elements.0.X" => { __agg0 : { $first : "$$ROOT" } } + "__agg0.X"
                if (node.Path.StartsWith("_elements.0."))
                {
                    var accumulatorFieldName = _groupOptimizer.GetNextAccumulatorFieldName();
                    var accumulatorExpression = AstExpression.AccumulatorExpression(AstAccumulatorOperator.First, AstExpression.Var("ROOT"));
                    _groupOptimizer.AddAccumulatorField(accumulatorFieldName, accumulatorExpression);
                    var restOfPath = node.Path.Substring("_elements.0.".Length);
                    var rewrittenPath = $"{accumulatorFieldName}.{restOfPath}";
                    return AstFilter.Field(rewrittenPath, node.Serializer);
                }

                return base.VisitFilterField(node);
            }

            public override AstNode VisitUnaryExpression(AstUnaryExpression node)
            {
                var root = AstExpression.Var("ROOT", isCurrent: true);

                if (TryOptimizeSizeOfElements(out var optimizedExpression))
                {
                    return optimizedExpression;
                }

                if (TryOptimizeAccumulatorOfElements(out optimizedExpression))
                {
                    return optimizedExpression;
                }

                if (TryOptimizeAccumulatorOfMappedElements(out optimizedExpression))
                {
                    return optimizedExpression;
                }

                return base.VisitUnaryExpression(node);

                bool TryOptimizeSizeOfElements(out AstExpression optimizedExpression)
                {
                    // { $size : "$_elements" } => { __agg0 : { $sum : 1 } } + "$__agg0"
                    if (node.Operator == AstUnaryOperator.Size)
                    {
                        if (node.Arg is AstGetFieldExpression argGetFieldExpression &&
                            argGetFieldExpression.FieldName is AstConstantExpression constantFieldNameExpression &&
                            constantFieldNameExpression.Value.IsString &&
                            constantFieldNameExpression.Value.AsString == "_elements")
                        {
                            var accumulatorFieldName = _groupOptimizer.GetNextAccumulatorFieldName();
                            var accumulatorExpression = AstExpression.AccumulatorExpression(AstAccumulatorOperator.Sum, 1);
                            _groupOptimizer.AddAccumulatorField(accumulatorFieldName, accumulatorExpression);
                            optimizedExpression = AstExpression.GetField(root, accumulatorFieldName);
                            return true;
                        }
                    }

                    optimizedExpression = null;
                    return false;
                }

                bool TryOptimizeAccumulatorOfElements(out AstExpression optimizedExpression)
                {
                    // { $accumulator : { $getField : { input : "$$ROOT", field : "_elements" } } } => { __agg0 : { $accumulator : "$$ROOT" } } + "$__agg0"
                    if (node.Operator.IsAccumulator(out var accumulatorOperator) &&
                        node.Arg is AstGetFieldExpression getFieldExpression &&
                        getFieldExpression.FieldName is AstConstantExpression getFieldConstantFieldNameExpression &&
                        getFieldConstantFieldNameExpression.Value.IsString &&
                        getFieldConstantFieldNameExpression.Value == "_elements" &&
                        getFieldExpression.Input is AstVarExpression getFieldInputVarExpression &&
                        getFieldInputVarExpression.Name == "ROOT")
                    {
                        var accumulatorFieldName = _groupOptimizer.GetNextAccumulatorFieldName();
                        var accumulatorExpression = AstExpression.AccumulatorExpression(accumulatorOperator, root);
                        _groupOptimizer.AddAccumulatorField(accumulatorFieldName, accumulatorExpression);
                        optimizedExpression = AstExpression.GetField(root, accumulatorFieldName);
                        return true;
                    }

                    optimizedExpression = null;
                    return false;

                }

                bool TryOptimizeAccumulatorOfMappedElements(out AstExpression optimizedExpression)
                {
                    // { $accumulator : { $map : { input : { $getField : { input : "$$ROOT", field : "_elements" } }, as : "x", in : f(x) } } } => { __agg0 : { $accumulator : f(x => root) } } + "$__agg0"
                    if (node.Operator.IsAccumulator(out var accumulatorOperator) &&
                        node.Arg is AstMapExpression mapExpression &&
                        mapExpression.Input is AstGetFieldExpression mapInputGetFieldExpression &&
                        mapInputGetFieldExpression.FieldName is AstConstantExpression mapInputconstantFieldExpression &&
                        mapInputconstantFieldExpression.Value.IsString &&
                        mapInputconstantFieldExpression.Value.AsString == "_elements" &&
                        mapInputGetFieldExpression.Input is AstVarExpression mapInputGetFieldVarExpression &&
                        mapInputGetFieldVarExpression.Name == "ROOT")
                    {
                        var accumulatorFieldName = _groupOptimizer.GetNextAccumulatorFieldName();
                        var rewrittenArg = (AstExpression)AstNodeReplacer.Replace(mapExpression.In, (mapExpression.As, root));
                        var accumulatorExpression = AstExpression.AccumulatorExpression(accumulatorOperator, rewrittenArg);
                        _groupOptimizer.AddAccumulatorField(accumulatorFieldName, accumulatorExpression);
                        optimizedExpression = AstExpression.GetField(root, accumulatorFieldName);
                        return true;
                    }

                    optimizedExpression = null;
                    return false;
                }
            }
        }
    }
}
