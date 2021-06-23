/* Copyright 2021-present MongoDB Inc.
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

using System.Threading;
using MongoDB.Driver.Core.Bindings;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core
{
    /// <summary>
    /// Connection pinning helper.
    /// </summary>
    public static class ChannelPinningHelper
    {
        // public because it's used in the driver project
        /// <summary>
        /// Create effective read binding.
        /// </summary>
        /// <param name="cluster">The cluster,</param>
        /// <param name="session">The session.</param>
        /// <param name="readPreference">The read preference.</param>
        /// <returns>An effective read binging.</returns>
        public static IReadBinding CreateEffectiveReadBindings(ICluster cluster, ICoreSessionHandle session, ReadPreference readPreference)
        {
            // this is used on collection level to not create WritableServerBinding if a connection is already pinned
            if (session.IsInTransaction && session.CurrentTransaction.IsConnectionPinned)
            {
                return new ChannelReadWriteBinding(
                    session.CurrentTransaction.PinnedServer,
                    session.CurrentTransaction.PinnedChannel,
                    session.Fork());
            }
            else
            {
                return new ReadPreferenceBinding(cluster, readPreference, session.Fork());
            }
        }

        // public because it's used in the driver project
        /// <summary>
        /// Create effective readwrite binding.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        public static IReadWriteBinding CreateEffectiveReadWriteBindings(ICluster cluster, ICoreSessionHandle session)
        {
            // this is used on collection level to not create WritableServerBinding if a connection is already pinned
            if (session.IsInTransaction && session.CurrentTransaction.IsConnectionPinned)
            {
                return new ChannelReadWriteBinding(
                    session.CurrentTransaction.PinnedServer,
                    session.CurrentTransaction.PinnedChannel,
                    session.Fork());
            }
            else
            {
                return new WritableServerBinding(cluster, session.Fork());
            }
        }

        internal static IChannelSource CreateEffectiveGetMoreChannelSource(IChannelSourceHandle channelSource, /*should not be here*/IChannelHandle channel, long cursorId)
        {
            if (channelSource.ServerDescription.Type == ServerType.LoadBalanced && cursorId != 0)
            {
                // The below if-else if workaround, should be reconsidered
                IChannelHandle getMoreChannel;
                ICoreSessionHandle getMoreSession;
                if (channelSource.Session.IsInTransaction)
                {
                    getMoreChannel = channel;
                    getMoreSession = channelSource.Session;
                }
                else
                {
                    // GetChannel uses forking
                    getMoreChannel = channelSource.GetChannel(CancellationToken.None); // no need for cancellation token since we already have channel in the source
                    getMoreSession = channelSource.Session.Fork(); 
                }
                return new ChannelChannelSource(
                    channelSource.Server,
                    getMoreChannel,
                    getMoreSession);
            }
            else
            {
                return new ServerChannelSource(channelSource.Server, channelSource.Session.Fork());
            }
        }

        internal static bool TryCreatePinnedChannelSourceAndPinChannel(
            IChannelSourceHandle channelSource,
            IChannelHandle channel,
            ICoreSessionHandle session,
            out (IChannelSourceHandle PinnedChannelSource, IChannelHandle Channel) pinnedChannel)
        {
            pinnedChannel = default;
            if (channel.ConnectionDescription.ServiceId.HasValue) // load banced mode
            {
                var server = channelSource.Server;
                var forkedChannel = channel.Fork(); // channel is valid, but fork it to protect agains disposing in ReplaceChannel
                var forkedSession = session.Fork(); // ChannelChannelSource.Dispose calls Dispose for Session, so protect it by forking

                var pinnedChannelSource = new ChannelSourceHandle(
                    new ChannelChannelSource(
                        server,
                        forkedChannel, 
                        forkedSession));

                if (session.IsInTransaction)
                {
                    session.CurrentTransaction.PinnedChannel = forkedChannel.Fork(); // protect channel against disposing after current operation
                    session.CurrentTransaction.PinnedServer = server;
                }

                pinnedChannel = (pinnedChannelSource, forkedChannel);

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
