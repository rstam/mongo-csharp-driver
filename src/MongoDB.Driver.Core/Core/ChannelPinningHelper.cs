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
using MongoDB.Driver.Core.Operations;
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
        public static IReadBindingHandle CreateEffectiveReadBinding(ICluster cluster, ICoreSessionHandle session, ReadPreference readPreference)
        {
            // this is used on collection level to not create WritableServerBinding if a connection is already pinned
            if (session.IsInTransaction && session.CurrentTransaction.IsConnectionPinned)
            {
                return
                    new ReadWriteBindingHandle(
                        new ChannelReadWriteBinding(
                        session.CurrentTransaction.PinnedServer,
                        session.CurrentTransaction.PinnedChannel,
                        session));
            }
            else
            {
                return new ReadBindingHandle(new ReadPreferenceBinding(cluster, readPreference, session));
            }
        }

        // public because it's used in the driver project
        /// <summary>
        /// Create effective readwrite binding.
        /// </summary>
        /// <param name="cluster">The cluster.</param>
        /// <param name="session">The session.</param>
        /// <returns></returns>
        public static IReadWriteBindingHandle CreateEffectiveReadWriteBinding(ICluster cluster, ICoreSessionHandle session)
        {
            // this is used on collection level to not create WritableServerBinding if a connection is already pinned
            if (session.IsInTransaction && session.CurrentTransaction.IsConnectionPinned)
            {
                return
                    new ReadWriteBindingHandle(
                        new ChannelReadWriteBinding(
                        session.CurrentTransaction.PinnedServer,
                        session.CurrentTransaction.PinnedChannel,
                        session));
            }
            else
            {
                return new ReadWriteBindingHandle(new WritableServerBinding(cluster, session));
            }
        }

        internal static IChannelSourceHandle CreateEffectiveGetMoreChannelSource(IChannelSourceHandle channelSource, IChannelHandle channel, long cursorId)
        {
            if (cursorId == 0)
            {
                return null;
            }

            if (channelSource.ServerDescription.Type == ServerType.LoadBalanced)
            {
                var getMoreChannel = channel.Fork();
                var getMoreSession = channelSource.Session.Fork();

                return
                    new ChannelSourceHandle(
                        new ChannelChannelSource(
                        channelSource.Server,
                        getMoreChannel,
                        getMoreSession));
            }
            else
            {
                return new ChannelSourceHandle(new ServerChannelSource(channelSource.Server, channelSource.Session.Fork()));
            }
        }

        internal static bool IsLoadBalanced(IChannelHandle channel)
        {
            return channel.ConnectionDescription.ServiceId.HasValue;
        }

        internal static (IChannelSourceHandle, IChannelHandle) CreatePinnedChannelSource(RetryableReadContext context)
        {
            return CreatePinnedChannelSource(context.ChannelSource.Server, context.Channel.Fork(), context.Binding.Session.Fork());
        }

        internal static (IChannelSourceHandle, IChannelHandle) CreatePinnedChannelSource(RetryableWriteContext context)
        {
            return CreatePinnedChannelSource(context.ChannelSource.Server, context.Channel.Fork(), context.Binding.Session.Fork());
        }

        private static (IChannelSourceHandle, IChannelHandle) CreatePinnedChannelSource(
            IServer server,
            IChannelHandle channel,
            ICoreSessionHandle session)
        {
            var pinnedChannelSource = new ChannelSourceHandle(new ChannelChannelSource(server, channel, session));
            var pinnedChannel = pinnedChannelSource.GetChannel(CancellationToken.None);

            if (session.IsInTransaction)
            {
                session.CurrentTransaction.PinnedServer = server;
                session.CurrentTransaction.PinConnection(pinnedChannel.Fork());
            }

            return (pinnedChannelSource, pinnedChannel);
        }
    }
}
