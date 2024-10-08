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

using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDB.Driver.Core.Connections
{
    /// <summary>
    /// Represents a stream factory.
    /// </summary>
    public interface IStreamFactory
    {
        /// <summary>
        /// Creates a stream.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Stream.</returns>
        Stream CreateStream(EndPoint endPoint, CancellationToken cancellationToken);

        /// <summary>
        /// Creates a stream.
        /// </summary>
        /// <param name="endPoint">The end point.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A Task whose result is the Stream.</returns>
        Task<Stream> CreateStreamAsync(EndPoint endPoint, CancellationToken cancellationToken);
    }
}
