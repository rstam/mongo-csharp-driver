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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using DnsClient;
using DnsClient.Protocol;
using FluentAssertions;
using MongoDB.Bson.TestHelpers;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;
using Moq;
using Xunit;

namespace MongoDB.Driver.Core.Clusters
{
    public class DnsMonitorTests
    {
        [Theory]
        [InlineData("a.com", "com")]
        [InlineData("a.b.com", "b.com")]
        [InlineData("a.b.c.com", "b.c.com")]
        public void GetParentDomainName_should_return_expected_result(string domainName, string expectedResult)
        {
            var result = DnsMonitorReflector.GetParentDomainName(domainName);

            result.Should().Be(expectedResult);
        }

        [Fact]
        public void constructor_should_initialize_instance()
        {
            var cluster = Mock.Of<IDnsMonitoringCluster>();
            var lookupDomainName = "a.b.c.com";
            var cancellationToken = new CancellationTokenSource().Token;

            var subject = new DnsMonitor(cluster, lookupDomainName, null, cancellationToken);

            subject.State.Should().Be(DnsMonitorState.Created);
            subject._cancellationToken().Should().Be(cancellationToken);
            subject._cluster().Should().BeSameAs(cluster);
            subject._lookupClient().Should().NotBeNull();
            subject._lookupDomainName().Should().Be("a.b.c.com");
            subject._parentDomainName().Should().Be("b.c.com");
            subject._query().Should().Be("_mongodb._tcp.a.b.c.com");
        }

        [Fact]
        public void constructor_should_throw_when_cluster_is_null()
        {
            var lookupDomainName = "a.b.c.com";
            var cancellationToken = new CancellationTokenSource().Token;

            var exception = Record.Exception(() => new DnsMonitor(null, lookupDomainName, null, cancellationToken));

            var e = exception.Should().BeOfType<ArgumentNullException>().Subject;
            e.ParamName.Should().Be("cluster");
        }

        [Fact]
        public void constructor_should_throw_when_lookupDomainName_is_null()
        {
            var cluster = Mock.Of<IDnsMonitoringCluster>();
            var cancellationToken = new CancellationTokenSource().Token;

            var exception = Record.Exception(() => new DnsMonitor(cluster, null, null, cancellationToken));

            var e = exception.Should().BeOfType<ArgumentNullException>().Subject;
            e.ParamName.Should().Be("lookupDomainName");
        }

        [Theory]
        [InlineData("test5.test.build.10gen.cc", new[] { "localhost.test.build.10gen.cc:27017" })]
        public void DnsMonitor_should_call_ProcessDnsResults_with_expected_endPoints(string lookupDomainName, string[] expectedEndPointStrings)
        {
            List<DnsEndPoint> actualEndPoints = null;
            var cts = new CancellationTokenSource();
            var mockCluster = new Mock<IDnsMonitoringCluster>();
            mockCluster.Setup(x => x.Description).Returns(CreateClusterDescription(ClusterType.Unknown));
            mockCluster
                .Setup(x => x.ProcessDnsResults(It.IsAny<List<DnsEndPoint>>()))
                .Callback((List<DnsEndPoint> endPoints) =>
                {
                    actualEndPoints = endPoints;
                    cts.Cancel();
                });
            var subject = CreateSubject(cluster: mockCluster.Object, lookupDomainName: lookupDomainName, cancellationToken: cts.Token);

            var thread = new Thread(subject.Start);
            thread.Start();
            thread.Join();

            var expectedEndPoints = expectedEndPointStrings.Select(e => (DnsEndPoint)EndPointHelper.Parse(e)).ToList();
            actualEndPoints.Should().Equal(expectedEndPoints);
            subject.State.Should().Be(DnsMonitorState.Stopped);
        }

        [Fact]
        public void Start_should_return_when_cancellation_is_requested()
        {
            var cts = new CancellationTokenSource();
            var mockCluster = new Mock<IDnsMonitoringCluster>();
            mockCluster.Setup(x => x.Description).Returns(CreateClusterDescription(ClusterType.Unknown));
            mockCluster
                .Setup(x => x.ProcessDnsResults(It.IsAny<List<DnsEndPoint>>()))
                .Callback(() =>
                {
                    cts.Cancel();
                });
            var lookupDomainName = "test5.test.build.10gen.cc";
            var subject = CreateSubject(cluster: mockCluster.Object, lookupDomainName: lookupDomainName, cancellationToken: cts.Token);

            var thread = new Thread(subject.Start);
            thread.Start();
            thread.Join();

            subject.State.Should().Be(DnsMonitorState.Stopped);
        }

        [Theory]
        [InlineData(ClusterType.Standalone)]
        [InlineData(ClusterType.ReplicaSet)]
        public void Start_should_return_when_cluster_type_does_not_require_monitoring(ClusterType clusterType)
        {
            var mockCluster = new Mock<IDnsMonitoringCluster>();
            mockCluster.Setup(x => x.Description).Returns(CreateClusterDescription(ClusterType.Unknown));
            mockCluster
                .Setup(x => x.ProcessDnsResults(It.IsAny<List<DnsEndPoint>>()))
                .Callback(() =>
                {
                    mockCluster.Setup(x => x.Description).Returns(CreateClusterDescription(clusterType));
                });
            var lookupDomainName = "test5.test.build.10gen.cc";
            var subject = CreateSubject(cluster: mockCluster.Object, lookupDomainName: lookupDomainName);

            var thread = new Thread(subject.Start);
            thread.Start();
            thread.Join();

            subject.State.Should().Be(DnsMonitorState.Stopped);
        }

        [Theory]
        [InlineData(new int[0], 60)]
        [InlineData(new[] { 30 }, 60)]
        [InlineData(new[] { 60 }, 60)]
        [InlineData(new[] { 61 }, 61)]
        [InlineData(new[] { 15, 30 }, 60)]
        [InlineData(new[] { 30, 60 }, 60)]
        [InlineData(new[] { 61, 90 }, 61)]
        public void ComputeRescanDelay_should_return_expected_result(int[] ttls, int expectedResult)
        {
            var subject = CreateSubject();
            var srvRecords = CreateSrvRecords(ttls);

            var result = subject.ComputeRescanDelay(srvRecords);

            result.Should().Be(TimeSpan.FromSeconds(expectedResult));
        }

        [Theory]
        [InlineData(new string[0], new string[0])]
        [InlineData(new[] { "x.b.c.com:27017" }, new[] { "x.b.c.com:27017" })]
        [InlineData(new[] { "x.b.c.com.:27017" }, new[] { "x.b.c.com:27017" })]
        [InlineData(new[] { "x.b.c.com:27017", "x.com:27017" }, new[] { "x.b.c.com:27017" })]
        [InlineData(new[] { "x.b.c.com:27017", "x.b.com:27017" }, new[] { "x.b.c.com:27017" })]
        [InlineData(new[] { "x.b.c.com:27017", "x.c.com:27017" }, new[] { "x.b.c.com:27017" })]
        [InlineData(new[] { "x.b.c.com:27017", "y.b.c.com:27017" }, new[] { "x.b.c.com:27017", "y.b.c.com:27017" })]
        public void GetDnsEndPoints_should_return_expected_results(string[] srvEndPoints, string[]expectedEndPoints)
        {
            var lookupDomainName = "a.b.c.com";
            var subject = CreateSubject(lookupDomainName: lookupDomainName);
            var srvRecords = CreateSrvRecords(srvEndPoints);

            var result = subject.GetDnsEndPoints(srvRecords);

            var expectedResult = expectedEndPoints.Select(x => (DnsEndPoint)EndPointHelper.Parse(x)).ToList();
            result.Should().Equal(expectedResult);
        }

        [Theory]
        [InlineData("a.b.com", "x.b.com", true)]
        [InlineData("a.b.com", "x.com", false)]
        [InlineData("a.b.com", "x.c.com", false)]
        [InlineData("a.b.c.com", "x.b.c.com", true)]
        [InlineData("a.b.c.com", "x.com", false)]
        [InlineData("a.b.c.com", "x.b.com", false)]
        [InlineData("a.b.c.com", "x.c.com", false)]
        [InlineData("a.b.c.com", "x.d.com", false)]
        [InlineData("a.b.c.com", "x.b.d.com", false)]
        [InlineData("a.b.c.com", "x.d.c.com", false)]
        public void IsValidHost_should_return_expected_result(string lookupDomainName, string host, bool expectedResult)
        {
            var subject = CreateSubject(lookupDomainName: lookupDomainName);

            var result = subject.IsValidHost(host);

            result.Should().Be(expectedResult);
        }

        [Theory]
        [InlineData("test5.test.build.10gen.cc", new[] { "localhost.test.build.10gen.cc:27017" })]
        public void QuerySrvRecords_should_return_expected_result(string lookupDomainName, string[] expectedEndPointStrings)
        {
            var subject = CreateSubject(lookupDomainName: lookupDomainName);

            var result = subject.QuerySrvRecords();

            var actualEndPoints = subject.GetDnsEndPoints(result.SrvRecords().ToList());
            var expectedEndPoints = expectedEndPointStrings.Select(x => (DnsEndPoint)EndPointHelper.Parse(x)).ToList();
            actualEndPoints.Should().Equal(expectedEndPoints);
        }

        [Fact]
        public void QuerySrvRecords_should_throw_if_cancellation_is_requested()
        {
            var cts = new CancellationTokenSource();
            var subject = CreateSubject(cancellationToken: cts.Token);
            cts.Cancel();

            var exception = Record.Exception(() => subject.QuerySrvRecords());

            exception.Should().BeOfType<OperationCanceledException>();
        }

        [Theory]
        [InlineData(ClusterType.Unknown, false)]
        [InlineData(ClusterType.Sharded, false)]
        [InlineData(ClusterType.Standalone, true)]
        [InlineData(ClusterType.ReplicaSet, true)]
        public void ShouldStopMonitoring_should_return_expected_result(ClusterType clusterType, bool expectedResult)
        {
            var mockCluster = new Mock<IDnsMonitoringCluster>();
            var clusterDescription = CreateClusterDescription(clusterType);
            mockCluster.SetupGet(x => x.Description).Returns(clusterDescription);
            var subject = CreateSubject(cluster: mockCluster.Object);

            var result = subject.ShouldStopMonitoring();

            result.Should().Be(expectedResult);
        }

        // private methods
        private ClusterDescription CreateClusterDescription(ClusterType type)
        {
            var clusterId = new ClusterId(1);
            var servers = new ServerDescription[0];
            return new ClusterDescription(clusterId, ClusterConnectionMode.Automatic, type, servers);
        }

        private List<SrvRecord> CreateSrvRecords(int[] ttls)
        {
            var srvRecords = new List<SrvRecord>();

            for (var i = 0; i < ttls.Length; i++)
            {
                var domainName = $"host{i + 1}.b.c.com";
                var info = new ResourceRecordInfo(domainName, ResourceRecordType.SRV, QueryClass.IN, ttls[i], 0);
                var target = DnsString.Parse(domainName);
                var srvRecord = new SrvRecord(info, 0, 0, 0, target);
                srvRecords.Add(srvRecord);
            }

            return srvRecords;
        }

        private List<SrvRecord> CreateSrvRecords(string[] endPoints)
        {
            var srvRecords = new List<SrvRecord>();

            for (var i = 0; i < endPoints.Length; i++)
            {
                var endPoint = (DnsEndPoint)EndPointHelper.Parse(endPoints[i]);
                var info = new ResourceRecordInfo(endPoint.Host, ResourceRecordType.SRV, QueryClass.IN, 0, 0);
                var target = DnsString.Parse(endPoint.Host);
                var srvRecord = new SrvRecord(info, 0, 0, (ushort)endPoint.Port, target);
                srvRecords.Add(srvRecord);
            }

            return srvRecords;
        }

        private DnsMonitor CreateSubject(
            IDnsMonitoringCluster cluster = null,
            string lookupDomainName = null,
            CancellationToken cancellationToken = default)
        {
            cluster = cluster ?? Mock.Of<IDnsMonitoringCluster>();
            lookupDomainName = lookupDomainName ?? "a.b.c.com";
            return new DnsMonitor(cluster, lookupDomainName, null, cancellationToken);
        }
    }

    internal static class DnsMonitorReflector
    {
        public static string GetParentDomainName(string domainName) => (string)Reflector.InvokeStatic(typeof(DnsMonitor), nameof(GetParentDomainName), domainName);

        public static CancellationToken _cancellationToken(this DnsMonitor obj) => (CancellationToken)Reflector.GetFieldValue(obj, nameof(_cancellationToken));
        public static IDnsMonitoringCluster _cluster(this DnsMonitor obj) => (IDnsMonitoringCluster)Reflector.GetFieldValue(obj, nameof(_cluster));
        public static LookupClient _lookupClient(this DnsMonitor obj) => (LookupClient)Reflector.GetFieldValue(obj, nameof(_lookupClient));
        public static string _lookupDomainName(this DnsMonitor obj) => (string)Reflector.GetFieldValue(obj, nameof(_lookupDomainName));
        public static string _parentDomainName(this DnsMonitor obj) => (string)Reflector.GetFieldValue(obj, nameof(_parentDomainName));
        public static string _query(this DnsMonitor obj) => (string)Reflector.GetFieldValue(obj, nameof(_query));

        public static TimeSpan ComputeRescanDelay(this DnsMonitor obj, List<SrvRecord> srvRecords) => (TimeSpan)Reflector.Invoke(obj, nameof(ComputeRescanDelay), srvRecords);
        public static List<DnsEndPoint> GetDnsEndPoints(this DnsMonitor obj, List<SrvRecord> srvRecords) => (List<DnsEndPoint>)Reflector.Invoke(obj, nameof(GetDnsEndPoints), srvRecords);
        public static bool IsValidHost(this DnsMonitor obj, string host) => (bool)Reflector.Invoke(obj, nameof(IsValidHost), host);
        public static List<SrvRecord> QuerySrvRecords(this DnsMonitor obj) => (List<SrvRecord>)Reflector.Invoke(obj, nameof(QuerySrvRecords));
        public static bool ShouldStopMonitoring(this DnsMonitor obj) => (bool)Reflector.Invoke(obj, nameof(ShouldStopMonitoring));
    }
}
