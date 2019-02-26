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
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Clusters
{
    internal class DnsMonitor
    {
        #region static
        private static string GetParentDomainName(string domainName)
        {
            var index = domainName.IndexOf('.');
            return domainName.Substring(index + 1);
        }
        #endregion

        // private fields
        private readonly CancellationToken _cancellationToken;
        private readonly IDnsMonitoringCluster _cluster;
        private readonly LookupClient _lookupClient;
        private readonly string _lookupDomainName;
        private readonly string _parentDomainName;
        private readonly string _query;
        private bool _startReturnedNormally; // used for testing

        // constructors
        public DnsMonitor(IDnsMonitoringCluster cluster, string lookupDomainName, CancellationToken cancellationToken)
        {
            _cluster = Ensure.IsNotNull(cluster, nameof(cluster));
            _lookupDomainName = Ensure.IsNotNull(lookupDomainName, nameof(lookupDomainName));
            _cancellationToken = cancellationToken;
            _lookupClient = new LookupClient();
            _parentDomainName = GetParentDomainName(lookupDomainName);
            _query = "_mongodb._tcp." + _lookupDomainName;
        }

        // public properties
        public bool StartReturnedNormally => _startReturnedNormally;

        // public methods
        public void Start()
        {
            try
            {
                Monitor();
            }
            catch (OperationCanceledException)
            {
                // ignore OperationCanceledException
            }
            catch (Exception)
            {
                // TODO: log unexpected exceptions
                throw;
            }
            _startReturnedNormally = true;
        }

        // private methods
        private TimeSpan ComputeRescanDelay(List<SrvRecord> srvRecords)
        {
            var delay = TimeSpan.FromSeconds(60);

            if (srvRecords.Count > 0)
            {
                var minTimeToLive = TimeSpan.FromSeconds(srvRecords.Select(s => s.InitialTimeToLive).Min());
                if (minTimeToLive > delay)
                {
                    delay = minTimeToLive;
                }
            }

            return delay;
        }

        private List<DnsEndPoint> GetDnsEndPoints(List<SrvRecord> srvRecords)
        {
            var endPoints = new List<DnsEndPoint>();

            foreach (var srvRecord in srvRecords)
            {
                var host = srvRecord.Target.ToString();
                if (host.EndsWith("."))
                {
                    host = host.Substring(0, host.Length - 1);
                }

                if (IsValidHost(host))
                {
                    // for testing
                    if (host == "localhost.mongodb_plus_srv_test.com")
                    {
                        host = "localhost";
                    }
                    var port = srvRecord.Port;
                    var endPoint = new DnsEndPoint(host, port);
                    endPoints.Add(endPoint);
                }
            }

            return endPoints;
        }

        private bool IsValidHost(string host)
        {
            return host.EndsWith(_parentDomainName);
        }

        private void Monitor()
        {
            while (true)
            {
                if (ShouldStopMonitoring())
                {
                    return;
                }

                var srvRecords = QuerySrvRecords();
                var dnsEndPoints = GetDnsEndPoints(srvRecords);
                _cluster.ProcessDnsResults(dnsEndPoints, _cancellationToken);

                if (ShouldStopMonitoring())
                {
                    return;
                }

                _cancellationToken.ThrowIfCancellationRequested();
                var delay = ComputeRescanDelay(srvRecords);
                Thread.Sleep(delay);
            }
        }

        private List<SrvRecord> QuerySrvRecords()
        {
            _cancellationToken.ThrowIfCancellationRequested();

            // for testing
            if (_lookupDomainName == "dnsmonitor_test.mongodb_plus_srv_test.com")
            {
                var host = DnsString.Parse("localhost.mongodb_plus_srv_test.com");
                var info = new ResourceRecordInfo(host, ResourceRecordType.SRV, QueryClass.IN, 600, 0);
                var srvRecord = new SrvRecord(info, 0, 0, 27017, host);
                return new List<SrvRecord> { srvRecord };
            }

            try
            {
                var response = _lookupClient.Query(_query, QueryType.SRV, QueryClass.IN);
                return response.Answers.SrvRecords().ToList();
            }
            catch (Exception)
            {
                // TODO: log exception
                return new List<SrvRecord>();
            }
        }

        private bool ShouldStopMonitoring()
        {
            var clusterType = _cluster.Description.Type;
            return clusterType != ClusterType.Unknown && clusterType != ClusterType.Sharded;
        }
    }
}
