/* Copyright 2018-present MongoDB Inc.
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
using FluentAssertions;
using MongoDB.Bson.TestHelpers;
using MongoDB.Driver.Core.Authentication;
using MongoDB.Driver.Core.Connections;
using MongoDB.Driver.Core.Servers;
using Xunit;

namespace MongoDB.Driver.Core.Configuration
{
    public class ClusterBuilderTests
    {
        [Theory]
        [InlineData(-1, 123, 30000)]
        [InlineData(0, 456, 30000)]
        [InlineData(60000, 789, 60000)]
        public void CreateServerMonitorFactory_should_return_expected_result(int connectTimeoutMilliseconds, int heartbeatMillieconds, int expectedServerMonitorConnectTimeoutMilliseconds)
        {
            var connectTimeout = TimeSpan.FromMilliseconds(connectTimeoutMilliseconds);
            var authenticators = new[] { new DefaultAuthenticator(new UsernamePasswordCredential("source", "username", "password")) };
            var heartbeatTimeout = TimeSpan.FromMilliseconds(heartbeatMillieconds);
            var expectedServerMonitorConnectTimeout = TimeSpan.FromMilliseconds(expectedServerMonitorConnectTimeoutMilliseconds);
            var subject = new ClusterBuilder()
                .ConfigureTcp(s => s.With(connectTimeout: connectTimeout))
                .ConfigureConnection(s => s.With(authenticators: authenticators))
                .ConfigureServer(s => s.With(heartbeatTimeout: heartbeatTimeout));

            var result = (ServerMonitorFactory)subject.CreateServerMonitorFactory();

            var serverMonitorConnectionFactory = (BinaryConnectionFactory)result._connectionFactory();
            var serverMonitorConnectionSettings = serverMonitorConnectionFactory._settings();
            serverMonitorConnectionSettings.Authenticators.Should().HaveCount(0);

            var serverMonitorStreamFactory = (TcpStreamFactory)serverMonitorConnectionFactory._streamFactory();
            var serverMonitorTcpStreamSettings = serverMonitorStreamFactory._settings();
            serverMonitorTcpStreamSettings.ConnectTimeout.Should().Be(expectedServerMonitorConnectTimeout);
            serverMonitorTcpStreamSettings.ReadTimeout.Should().Be(heartbeatTimeout);
            serverMonitorTcpStreamSettings.WriteTimeout.Should().Be(heartbeatTimeout);

            var eventSuscriber = result._eventSubscriber();

            var serverSettings = result._settings();
        }
    }

    public static class ClusterBuilderReflector
    {
        internal static IServerMonitorFactory CreateServerMonitorFactory(this ClusterBuilder obj) => (IServerMonitorFactory)Reflector.Invoke(obj, nameof(CreateServerMonitorFactory));
    }
}
