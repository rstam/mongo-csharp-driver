﻿/* Copyright 2015-present MongoDB Inc.
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
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver.GridFS;
using MongoDB.TestHelpers.XunitExtensions;
using Xunit;

namespace MongoDB.Driver.Tests.GridFS
{
    public class GridFSFileInfoTests
    {
        [Fact]
        public void BackingDocument_get_should_return_the_expected_result()
        {
            var backingDocument = new BsonDocument("x", 1);
            var subject = new GridFSFileInfo(backingDocument);

            var result = subject.BackingDocument;

            result.Should().BeSameAs(backingDocument);
        }

        [Fact]
        public void ChunkSizeBytes_get_should_return_the_expected_result()
        {
            var value = 1024;
            var subject = CreateSubject(chunkSizeBytes: value);

            var result = subject.ChunkSizeBytes;

            result.Should().Be(value);
        }

        [Fact]
        public void ChunkSizeBytes_should_be_deserialized_correctly()
        {
            var document = CreateFilesCollectionDocument();

            var subject = DeserializeFilesCollectionDocument(document);

            subject.ChunkSizeBytes.Should().Be(document["chunkSize"].ToInt32());
        }

        [Fact]
        public void constructor_should_initialize_instance()
        {
            var backdocument = new BsonDocument();

            var result = new GridFSFileInfo(backdocument);

            result.BackingDocument.Should().BeSameAs(backdocument);
        }

        [Theory]
        [ParameterAttributeData]
        public void ExtraElements_should_be_deserialized_correctly(
            [Values(new string[0], new[] { "x" }, new[] { "x", "y" }, new[] { "ExtraElements" })]
            string[] names)
        {
            var document = CreateFilesCollectionDocument();

            var extraElements = new BsonDocument();
            var value = 1;
            foreach (var name in names)
            {
                extraElements.Add(name, value++);
            }

            document.Merge(extraElements, overwriteExistingElements: false);

            var subject = DeserializeFilesCollectionDocument(document);

            foreach (var element in extraElements)
            {
                subject.BackingDocument[element.Name].Should().Be(element.Value);
            }
        }

        [Fact]
        public void Filename_get_should_return_the_expected_result()
        {
            var value = "abc";
            var subject = CreateSubject(filename: value);

            var result = subject.Filename;

            result.Should().Be(value);
        }

        [Fact]
        public void Filename_should_be_deserialized_correctly()
        {
            var document = CreateFilesCollectionDocument();

            var result = DeserializeFilesCollectionDocument(document);

            result.Filename.Should().Be(document["filename"].AsString);
        }

        [Fact]
        public void Id_get_should_return_the_expected_result()
        {
            var value = ObjectId.GenerateNewId();
            var subject = CreateSubject(idAsBsonValue: value);

            var result = subject.Id;

            result.Should().Be(value);
        }

        [Fact]
        public void Id_get_should_throw_when_id_is_not_an_ObjectId()
        {
            var value = (BsonValue)123;
            var subject = CreateSubject(idAsBsonValue: value);

            Action action = () => { var id = subject.Id; };

            action.ShouldThrow<InvalidCastException>();
        }

        [Fact]
        public void Id_should_be_deserialized_correctly()
        {
            var document = CreateFilesCollectionDocument();

            var result = DeserializeFilesCollectionDocument(document);

            result.Id.Should().Be(document["_id"].AsObjectId);
#pragma warning disable 618
            result.IdAsBsonValue.Should().Be(document["_id"]);
#pragma warning restore
        }

        [Fact]
        public void Id_should_be_deserialized_correctly_when_id_is_not_an_ObjectId()
        {
            var document = CreateFilesCollectionDocument();
            document["_id"] = 123;

            var result = DeserializeFilesCollectionDocument(document);

#pragma warning disable 618
            result.IdAsBsonValue.Should().Be(document["_id"]);
#pragma warning restore
        }

        [Fact]
        public void IdAsBsonValue_get_should_return_the_expected_result()
        {
            var value = (BsonValue)123;
            var subject = CreateSubject(idAsBsonValue: value);

#pragma warning disable 618
            var result = subject.IdAsBsonValue;
#pragma warning restore

            result.Should().Be(value);
        }

        [Fact]
        public void Length_get_should_return_the_expected_result()
        {
            var value = 123;
            var subject = CreateSubject(length: value);

            var result = subject.Length;

            result.Should().Be(value);
        }

        [Fact]
        public void Length_should_be_deserialized_correctly()
        {
            var document = CreateFilesCollectionDocument();

            var subject = DeserializeFilesCollectionDocument(document);

            subject.Length.Should().Be(document["length"].ToInt64());
        }

        [Fact]
        public void Metadata_get_should_return_the_expected_result()
        {
            var value = new BsonDocument("x", 1);
            var subject = CreateSubject(metadata: value);

            var result = subject.Metadata;

            result.Should().Be(value);
        }

        [Theory]
        [ParameterAttributeData]
        public void Metadata_should_be_deserialized_correctly(
            [Values(null, "{ }", "{ x : 1 }")]
            string json)
        {
            var document = CreateFilesCollectionDocument();
            BsonDocument metadata = null;
            if (json != null)
            {
                metadata = BsonDocument.Parse(json);
                document["metadata"] = metadata;
            }

            var result = DeserializeFilesCollectionDocument(document);

            result.Metadata.Should().Be(metadata);
        }

        [Fact]
        public void UploadDateTime_get_should_return_the_expected_result()
        {
            var value = (new BsonDateTime(DateTime.UtcNow)).ToUniversalTime(); // truncated to millisecond precision
            var subject = CreateSubject(uploadDateTime: value);

            var result = subject.UploadDateTime;

            result.Should().Be(value);
        }

        [Fact]
        public void UploadDateTime_should_be_deserialized_correctly()
        {
            var document = CreateFilesCollectionDocument();

            var subject = DeserializeFilesCollectionDocument(document);

            subject.UploadDateTime.Should().Be(document["uploadDate"].ToUniversalTime());
        }

        // private methods
        private BsonDocument CreateFilesCollectionDocument()
        {
            return new BsonDocument
            {
                { "_id", ObjectId.GenerateNewId() },
                { "length", 123 },
                { "chunkSize", 1024 },
                { "uploadDate", DateTime.UtcNow },
                { "filename", "name" }
            };
        }

        private GridFSFileInfo CreateSubject(
            int? chunkSizeBytes = null,
            BsonDocument extraElements = null,
            string filename = null,
            BsonValue idAsBsonValue = null,
            long? length = null,
            BsonDocument metadata = null,
            DateTime? uploadDateTime = null)
        {
            var backingDocument = new BsonDocument
            {
                { "_id", idAsBsonValue ?? (BsonValue)ObjectId.GenerateNewId() },
                { "length", length ?? 0 },
                { "chunkSize", chunkSizeBytes ?? 255 * 1024 },
                { "uploadDate", uploadDateTime ?? DateTime.UtcNow },
                { "filename", filename ?? "filename" },
                { "metadata", metadata, metadata != null }
            };
            if (extraElements != null)
            {
                backingDocument.Merge(extraElements, overwriteExistingElements: false);
            }

            return new GridFSFileInfo(backingDocument);
        }

        private GridFSFileInfo DeserializeFilesCollectionDocument(BsonDocument document)
        {
            return BsonSerializer.Deserialize<GridFSFileInfo>(document);
        }
    }
}
