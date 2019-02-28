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
using MongoDB.Driver.Core.Events;
using MongoDB.Driver.Core.Misc;

namespace MongoDB.Driver.Core.Clusters
{
    internal enum DnsMonitorState
    {
        Created,
        Running,
        Failed,
        Stopped
    }

    internal class DnsMonitor
    {
        #region static
        private static string EnsureLookupDomainNameIsValid(string lookupDomainName)
        {
            Ensure.IsNotNull(lookupDomainName, nameof(lookupDomainName));
            Ensure.That(lookupDomainName.Count(c => c == '.') >= 2, "LookupDomainName must have at least three components.", nameof(lookupDomainName));
            return lookupDomainName;
        }

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
        private bool _processDnsResultHasEverBeenCalled;
        private readonly string _query;
        private DnsMonitorState _state;
        private Exception _unhandledException;

        private readonly Action<SdamInformationEvent> _sdamInformationEventHandler;

        // constructors
        public DnsMonitor(IDnsMonitoringCluster cluster, string lookupDomainName, IEventSubscriber eventSubscriber, CancellationToken cancellationToken)
        {
            _cluster = Ensure.IsNotNull(cluster, nameof(cluster));
            _lookupDomainName = EnsureLookupDomainNameIsValid(lookupDomainName);
            _cancellationToken = cancellationToken;
            _lookupClient = new LookupClient();
            _parentDomainName = GetParentDomainName(lookupDomainName);
            _query = "_mongodb._tcp." + _lookupDomainName;
            _state = DnsMonitorState.Created;

            eventSubscriber?.TryGetEventHandler(out _sdamInformationEventHandler);
        }

        // public properties
        public DnsMonitorState State => _state;

        public Exception UnhandledException => _unhandledException;

        // public methods
        public void Start()
        {
            _state = DnsMonitorState.Running;
            try
            {
                Monitor();
            }
            catch (OperationCanceledException)
            {
                // ignore OperationCanceledException
            }
            catch (Exception exception)
            {
                _state = DnsMonitorState.Failed;
                _unhandledException = exception;

                if (_sdamInformationEventHandler != null)
                {
                    var message = $"Unexpected exception in DnsMonitor: {exception}.";
                    var sdamInformationEvent = new SdamInformationEvent(() => message);
                    _sdamInformationEventHandler(sdamInformationEvent);
                }

                throw;
            }
            _state = DnsMonitorState.Stopped;
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
                    var port = srvRecord.Port;
                    var endPoint = new DnsEndPoint(host, port);
                    endPoints.Add(endPoint);
                }
                else
                {
                    if (_sdamInformationEventHandler != null)
                    {
                        var message = $"Invalid host returned by DNS SRV lookup: {host}.";
                        var sdamInformationEvent = new SdamInformationEvent(() => message);
                        _sdamInformationEventHandler(sdamInformationEvent);
                    }

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
                _cluster.ProcessDnsResults(dnsEndPoints);
                _processDnsResultHasEverBeenCalled = true;

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

            try
            {
                var response = _lookupClient.Query(_query, QueryType.SRV, QueryClass.IN);
                return response.Answers.SrvRecords().ToList();
            }
            catch (Exception exception)
            {
                if (!_processDnsResultHasEverBeenCalled)
                {
                    _cluster.ProcessDnsException(exception);
                }
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
