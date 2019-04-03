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

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Bson.TestHelpers.JsonDrivenTests;
using MongoDB.Driver.GridFS;

namespace MongoDB.Driver.Tests.JsonDrivenTests
{
    public sealed class JsonDrivenDownloadByNameTest : JsonDrivenGridFSTest
    {
        // private fields
        private GridFSDownloadByNameOptions _downloadOptions = new GridFSDownloadByNameOptions();
        private string _fileName;
        private FilterDefinition<BsonDocument> _filter = new BsonDocument();
        private GridFSBucketOptions _options = new GridFSBucketOptions();
        private IClientSessionHandle _session;

        // public constructors
        public JsonDrivenDownloadByNameTest(IMongoDatabase database, string bucketName, Dictionary<string, object> objectMap)
            : base(database, objectMap)
        {
            _options.BucketName = bucketName;
        }

        // public methods
        public override void Arrange(BsonDocument document)
        {
            JsonDrivenHelper.EnsureAllFieldsAreValid(document, "name", "object", "databaseOptions", "arguments", "result", "error");
            base.Arrange(document);
        }

        // protected methods
        protected override void AssertResult()
        {    
        }

        protected override void CallMethod(CancellationToken cancellationToken)
        {
            var bucket = new GridFSBucket(_database, _options);
            bucket.DownloadAsBytesByName(_fileName, _downloadOptions, cancellationToken);
        }

        protected override async Task CallMethodAsync(CancellationToken cancellationToken)
        {
            var bucket = new GridFSBucket(_database, _options);
            await bucket.DownloadAsBytesByNameAsync(_fileName, _downloadOptions, cancellationToken).ConfigureAwait(false);           
        }

        protected override void SetArgument(string name, BsonValue value)
        {
            switch (name)
            {
                case "session":
                    _session = (IClientSessionHandle)_objectMap[value.AsString];
                    return;
                case "filename":
                    _fileName = value.AsString;
                    return; 
            }

            base.SetArgument(name, value);
        }
    }
}
