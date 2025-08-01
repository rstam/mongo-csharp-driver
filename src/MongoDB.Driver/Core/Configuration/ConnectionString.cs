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
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Bson.IO;
using MongoDB.Driver.Core.Compression;
using MongoDB.Driver.Core.Misc;
using MongoDB.Driver.Core.Servers;

namespace MongoDB.Driver.Core.Configuration
{
    /// <summary>
    ///  Represents the scheme used to construct the connection string.
    /// </summary>
    public enum ConnectionStringScheme
    {
        /// <summary>
        /// Mongodb scheme (mongodb://)
        /// </summary>
        MongoDB,
        /// <summary>
        /// SRV scheme (mongodb+srv://)
        /// </summary>
        MongoDBPlusSrv
    }

    /// <summary>
    /// Represents a connection string.
    /// </summary>
    public sealed class ConnectionString
    {
        // constants
        private const int defaultMongoDBPort = 27017;
        private const int defaultSrvPort = 53;

        // private fields
        private readonly string _originalConnectionString;
        private readonly NameValueCollection _allOptions;
        private readonly NameValueCollection _unknownOptions;
        private readonly Dictionary<string, string> _authMechanismProperties;
        private readonly CompressorsOptions _compressorsOptions;
        private readonly IDnsResolver _dnsResolver;
        private readonly string _srvPrefix;

        // these are all readonly, but since they are not assigned
        // from the ctor, they cannot be marked as such.
        private string _applicationName;
        private string _authMechanism;
        private string _authSource;
        private TimeSpan? _connectTimeout;
        private string _databaseName;
        private bool _directConnection;
        private bool? _fsync;
        private TimeSpan? _heartbeatInterval;
        private TimeSpan? _heartbeatTimeout;
        private IReadOnlyList<EndPoint> _hosts;
        private bool? _ipv6;
        private readonly bool _isInternalRepresentation;
        private readonly bool _isResolved;
        private bool? _journal;
        private bool _loadBalanced;
        private TimeSpan? _localThreshold;
        private int? _maxConnecting;
        private TimeSpan? _maxIdleTime;
        private TimeSpan? _maxLifeTime;
        private int? _maxPoolSize;
        private TimeSpan? _maxStaleness;
        private int? _minPoolSize;
        private string _password;
        private ReadConcernLevel? _readConcernLevel;
        private ReadPreferenceMode? _readPreference;
        private IReadOnlyList<TagSet> _readPreferenceTags;
        private string _replicaSet;
        private bool? _retryReads;
        private bool? _retryWrites;
        private ConnectionStringScheme _scheme;
        private ServerMonitoringMode? _serverMonitoringMode;
        private TimeSpan? _serverSelectionTimeout;
        private TimeSpan? _socketTimeout;
        private int? _srvMaxHosts;
        private string _srvServiceName;
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
        private TimeSpan? _timeout;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
        private bool? _tls;
        private bool? _tlsDisableCertificateRevocationCheck;
        private bool? _tlsInsecure;
        private string _username;
        private double? _waitQueueMultiple;
        private int? _waitQueueSize;
        private TimeSpan? _waitQueueTimeout;
        private WriteConcern.WValue _w;
        private TimeSpan? _wTimeout;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionString" /> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        public ConnectionString(string connectionString) : this(connectionString, false, DnsClientWrapper.Instance)
        {
        }

        internal ConnectionString(string connectionString, bool isInternalRepresentation, IDnsResolver dnsResolver)
        {
            _originalConnectionString = Ensure.IsNotNull(connectionString, nameof(connectionString));
            _isInternalRepresentation = isInternalRepresentation;

            _allOptions = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
            _unknownOptions = new NameValueCollection(StringComparer.OrdinalIgnoreCase);
            _authMechanismProperties = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            _compressorsOptions = new CompressorsOptions(_unknownOptions);
            _dnsResolver = Ensure.IsNotNull(dnsResolver, nameof(dnsResolver));
            Parse();

            _srvPrefix = $"_{_srvServiceName ?? MongoInternalDefaults.MongoClientSettings.SrvServiceName}._tcp.";

            _isResolved = _scheme != ConnectionStringScheme.MongoDBPlusSrv;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionString" /> class.
        /// </summary>
        /// <param name="connectionString">The connection string.</param>
        /// <param name="isResolved">Whether the connection string is resolved.</param>
        internal ConnectionString(string connectionString, bool isResolved)
            : this(connectionString, true, DnsClientWrapper.Instance)
        {
            if (!isResolved && _scheme != ConnectionStringScheme.MongoDBPlusSrv)
            {
                throw new ArgumentException("Only connection strings with scheme MongoDBPlusSrv can be unresolved.", nameof(isResolved));
            }

            _isResolved = isResolved;
        }

        // public properties
        /// <summary>
        /// Gets all the option names.
        /// </summary>
        public IEnumerable<string> AllOptionNames
        {
            get { return _allOptions.AllKeys; }
        }

        /// <summary>
        /// Gets all the unknown option names.
        /// </summary>
        public IEnumerable<string> AllUnknownOptionNames
        {
            get { return _unknownOptions.AllKeys; }
        }

        /// <summary>
        /// Gets the application name.
        /// </summary>
        public string ApplicationName
        {
            get { return _applicationName; }
        }

        /// <summary>
        /// Gets the auth mechanism.
        /// </summary>
        public string AuthMechanism
        {
            get { return _authMechanism; }
        }

        /// <summary>
        /// Gets the auth mechanism properties.
        /// </summary>
        public IReadOnlyDictionary<string, string> AuthMechanismProperties
        {
            get { return _authMechanismProperties; }
        }

        /// <summary>
        /// Gets the auth source.
        /// </summary>
        public string AuthSource
        {
            get { return _authSource; }
        }

        /// <summary>
        /// Gets the requested compressors.
        /// </summary>
        public IReadOnlyList<CompressorConfiguration> Compressors
        {
            get { return _compressorsOptions.Compressors; }
        }

        /// <summary>
        /// Gets the connect timeout.
        /// </summary>
        public TimeSpan? ConnectTimeout
        {
            get { return _connectTimeout; }
        }

        /// <summary>
        /// Gets the name of the database.
        /// </summary>
        public string DatabaseName
        {
            get { return _databaseName; }
        }

        /// <summary>
        /// Gets the directConnection.
        /// </summary>
        public bool DirectConnection
        {
            get { return _directConnection; }
        }

        /// <summary>
        /// Gets the fsync value of the write concern.
        /// </summary>
        public bool? FSync
        {
            get { return _fsync; }
        }

        /// <summary>
        /// Gets the heartbeat interval.
        /// </summary>
        public TimeSpan? HeartbeatInterval
        {
            get { return _heartbeatInterval; }
        }

        /// <summary>
        /// Gets the heartbeat timeout.
        /// </summary>
        public TimeSpan? HeartbeatTimeout
        {
            get { return _heartbeatTimeout; }
        }

        /// <summary>
        /// Gets the hosts.
        /// </summary>
        public IReadOnlyList<EndPoint> Hosts
        {
            get { return _srvMaxHosts > 0 ? _hosts.Take(_srvMaxHosts.Value).ToList() : _hosts; }
        }

        /// <summary>
        /// Gets whether to use IPv6.
        /// </summary>
        public bool? Ipv6
        {
            get { return _ipv6; }
        }

        /// <summary>
        /// Gets whether the connection string has been resolved. Always true when scheme is MongoDB.
        /// </summary>
        public bool IsResolved
        {
            get { return _isResolved; }
        }

        /// <summary>
        /// Gets the journal value of the write concern.
        /// </summary>
        public bool? Journal
        {
            get { return _journal; }
        }

        /// <summary>
        /// Gets a value indicating whether load balanced mode is used.
        /// </summary>
        public bool LoadBalanced => _loadBalanced;

        /// <summary>
        /// Gets the local threshold.
        /// </summary>
        public TimeSpan? LocalThreshold
        {
            get { return _localThreshold; }
        }

        /// <summary>
        /// Gets the maximum number of connections a pool may be establishing concurrently. Defaults to 2.
        /// </summary>
        public int? MaxConnecting
        {
            get { return _maxConnecting; }
        }

        /// <summary>
        /// Gets the max idle time.
        /// </summary>
        public TimeSpan? MaxIdleTime
        {
            get { return _maxIdleTime; }
        }

        /// <summary>
        /// Gets the max life time.
        /// </summary>
        public TimeSpan? MaxLifeTime
        {
            get { return _maxLifeTime; }
        }

        /// <summary>
        /// Gets the max size of the connection pool.
        /// </summary>
        public int? MaxPoolSize
        {
            get { return _maxPoolSize; }
        }

        /// <summary>
        /// Gets the max staleness.
        /// </summary>
        public TimeSpan? MaxStaleness
        {
            get { return _maxStaleness; }
        }

        /// <summary>
        /// Gets the min size of the connection pool.
        /// </summary>
        public int? MinPoolSize
        {
            get { return _minPoolSize; }
        }

        /// <summary>
        /// Gets the password.
        /// </summary>
        public string Password
        {
            get { return _password; }
        }

        /// <summary>
        /// Gets the read concern level.
        /// </summary>
        /// <value>
        /// The read concern level.
        /// </value>
        public ReadConcernLevel? ReadConcernLevel
        {
            get { return _readConcernLevel; }
        }

        /// <summary>
        /// Gets the read preference.
        /// </summary>
        public ReadPreferenceMode? ReadPreference
        {
            get { return _readPreference; }
        }

        /// <summary>
        /// Gets the replica set name.
        /// </summary>
        public string ReplicaSet
        {
            get { return _replicaSet; }
        }

        /// <summary>
        /// Gets the read preference tags.
        /// </summary>
        public IReadOnlyList<TagSet> ReadPreferenceTags
        {
            get { return _readPreferenceTags; }
        }

        /// <summary>
        /// Gets a value indicating whether or not to retry reads.
        /// </summary>
        public bool? RetryReads
        {
            get { return _retryReads; }
        }

        /// <summary>
        /// Gets a value indicating whether or not to retry writes.
        /// </summary>
        public bool? RetryWrites
        {
            get { return _retryWrites; }
        }

        /// <summary>
        /// Gets the connection string scheme.
        /// </summary>
        public ConnectionStringScheme Scheme
        {
            get { return _scheme; }
        }

        /// <summary>
        /// Gets the server monitoring mode.
        /// </summary>
        public ServerMonitoringMode? ServerMonitoringMode
        {
            get { return _serverMonitoringMode; }
        }

        /// <summary>
        /// Gets the server selection timeout.
        /// </summary>
        public TimeSpan? ServerSelectionTimeout
        {
            get { return _serverSelectionTimeout; }
        }

        /// <summary>
        /// Gets the socket timeout.
        /// </summary>
        public TimeSpan? SocketTimeout
        {
            get { return _socketTimeout; }
        }

        /// <summary>
        /// Limits the number of SRV records used to populate the seedlist
        /// during initial discovery, as well as the number of additional hosts
        /// that may be added during SRV polling.
        /// </summary>
        public int? SrvMaxHosts => _srvMaxHosts;

        /// <summary>
        /// Gets the SRV service name which modifies the srv URI to look like:
        /// <code>_{srvServiceName}._tcp.{hostname}.{domainname}</code>
        /// </summary>
        public string SrvServiceName => _srvServiceName;

        /// <summary>
        /// Gets whether to use SSL.
        /// </summary>
        [Obsolete("Use Tls instead.")]
        public bool? Ssl
        {
            get { return _tls; }
        }

        /// <summary>
        /// Gets whether to verify SSL certificates.
        /// </summary>
        [Obsolete("Use TlsInsecure instead.")]
        public bool? SslVerifyCertificate => !_tlsInsecure;

        /// <summary>
        /// Gets the per-operation timeout.
        /// </summary>
        // TODO: CSOT: Make it public when CSOT will be ready for GA
        internal TimeSpan? Timeout => _timeout;

        /// <summary>
        /// Gets whether to use TLS.
        /// </summary>
        public bool? Tls => _tls;

        /// <summary>
        ///  Get whether or not certificate revocation checking is disabled during the TLS handshake.
        /// </summary>
        public bool? TlsDisableCertificateRevocationCheck => _tlsDisableCertificateRevocationCheck;

        /// <summary>
        /// Gets whether to relax TLS constraints as much as possible.
        /// </summary>
        public bool? TlsInsecure => _tlsInsecure;

        /// <summary>
        /// Gets the username.
        /// </summary>
        public string Username
        {
            get { return _username; }
        }

        /// <summary>
        /// Gets the wait queue multiple.
        /// </summary>
        [Obsolete("This property will be removed in a later release.")]
        public double? WaitQueueMultiple
        {
            get { return _waitQueueMultiple; }
        }

        /// <summary>
        /// Gets the wait queue size.
        /// </summary>
        [Obsolete("This property will be removed in a later release.")]
        public int? WaitQueueSize
        {
            get { return _waitQueueSize; }
        }

        /// <summary>
        /// Gets the wait queue timeout.
        /// </summary>
        public TimeSpan? WaitQueueTimeout
        {
            get { return _waitQueueTimeout; }
        }

        /// <summary>
        /// Gets the w value of the write concern.
        /// </summary>
        public WriteConcern.WValue W
        {
            get { return _w; }
        }

        /// <summary>
        /// Gets the wtimeout value of the write concern.
        /// </summary>
        public TimeSpan? WTimeout
        {
            get { return _wTimeout; }
        }

        // public methods
        /// <summary>
        /// Gets the option.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>The option with the specified name.</returns>
        public string GetOption(string name)
        {
            return _allOptions[name];
        }

        /// <summary>
        /// Resolves a connection string. If the connection string indicates more information is available
        /// in the DNS system, it will acquire that information as well.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A resolved ConnectionString.</returns>
        public ConnectionString Resolve(CancellationToken cancellationToken = default(CancellationToken))
        {
            return Resolve(resolveHosts: true, cancellationToken);
        }

        /// <summary>
        /// Resolves a connection string. If the connection string indicates more information is available
        /// in the DNS system, it will acquire that information as well.
        /// </summary>
        /// <param name="resolveHosts">Whether to resolve hosts.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A resolved ConnectionString.</returns>
        public ConnectionString Resolve(bool resolveHosts, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_isResolved)
            {
                return this;
            }

            var host = GetHostNameForDns();

            ConnectionStringScheme resolvedScheme;
            List<string> hosts;
            if (resolveHosts)
            {
                resolvedScheme = ConnectionStringScheme.MongoDB;
                var srvRecords = _dnsResolver.ResolveSrvRecords(_srvPrefix + host, cancellationToken);
                hosts = GetHostsFromSrvRecords(srvRecords);
                ValidateResolvedHosts(host, hosts);
            }
            else
            {
                resolvedScheme = ConnectionStringScheme.MongoDBPlusSrv;
                hosts = new List<string> { host };
            }

            var txtRecords = _dnsResolver.ResolveTxtRecords(host, cancellationToken);
            var resolvedOptions = GetResolvedOptionsFromTxtRecords(txtRecords);

            return BuildResolvedConnectionString(resolvedScheme, hosts, resolvedOptions);
        }

        /// <summary>
        /// Resolves a connection string. If the connection string indicates more information is available
        /// in the DNS system, it will acquire that information as well.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A resolved ConnectionString.</returns>
        public Task<ConnectionString> ResolveAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return ResolveAsync(resolveHosts: true, cancellationToken);
        }

        /// <summary>
        /// Resolves a connection string. If the connection string indicates more information is available
        /// in the DNS system, it will acquire that information as well.
        /// </summary>
        /// <param name="resolveHosts">Whether to resolve hosts.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A resolved ConnectionString.</returns>
        public async Task<ConnectionString> ResolveAsync(bool resolveHosts, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (_isResolved)
            {
                return this;
            }

            var host = GetHostNameForDns();

            ConnectionStringScheme resolvedScheme;
            List<string> hosts;
            if (resolveHosts)
            {
                resolvedScheme = ConnectionStringScheme.MongoDB;
                var srvRecords = await _dnsResolver.ResolveSrvRecordsAsync(_srvPrefix + host, cancellationToken).ConfigureAwait(false);
                hosts = GetHostsFromSrvRecords(srvRecords);
                ValidateResolvedHosts(host, hosts);
            }
            else
            {
                resolvedScheme = ConnectionStringScheme.MongoDBPlusSrv;
                hosts = new List<string> { host };
            }

            var txtRecords = await _dnsResolver.ResolveTxtRecordsAsync(host, cancellationToken).ConfigureAwait(false);
            var resolvedOptions = GetResolvedOptionsFromTxtRecords(txtRecords);

            return BuildResolvedConnectionString(resolvedScheme, hosts, resolvedOptions);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return _originalConnectionString;
        }

        // private methods
        private ConnectionString BuildResolvedConnectionString(ConnectionStringScheme resolvedScheme, List<string> resolvedHosts, NameValueCollection resolvedOptions)
        {
            var connectionString = resolvedScheme == ConnectionStringScheme.MongoDBPlusSrv ? "mongodb+srv://" : "mongodb://";
            if (_username != null)
            {
                connectionString += Uri.EscapeDataString(_username);
                if (_password != null)
                {
                    connectionString += ":" + Uri.EscapeDataString(_password);
                }

                connectionString += "@";
            }

            connectionString += string.Join(",", resolvedHosts) + "/";
            if (_databaseName != null)
            {
                connectionString += Uri.EscapeDataString(_databaseName);
            }

            // remove any option from the resolved options that was specified locally
            foreach (var key in _allOptions.AllKeys)
            {
                if (resolvedOptions.Get(key) != null)
                {
                    resolvedOptions.Remove(key);
                }
            }

            resolvedOptions.Add(_allOptions);

            var mergedOptions = new List<string>();
            mergedOptions.AddRange(
                resolvedOptions
                .AllKeys
                .SelectMany(key => resolvedOptions
                    .GetValues(key)
                    .Select(value => $"{key}={EscapeOptionValue(key, value)}")));

            if (mergedOptions.Count > 0)
            {
                connectionString += "?" + string.Join("&", mergedOptions);
            }

            return new ConnectionString(connectionString, isResolved: true);

            string EscapeOptionValue(string key, string value)
                => string.Equals(key, "authmechanismproperties", StringComparison.OrdinalIgnoreCase) ? value : Uri.EscapeDataString(value);

        }

        private void ExtractScheme(Match match)
        {
            var schemeGroup = match.Groups["scheme"];
            if (schemeGroup.Success && schemeGroup.Value == "mongodb+srv")
            {
                _scheme = ConnectionStringScheme.MongoDBPlusSrv;
                if (!_tls.HasValue)
                {
                    _tls = true;
                    _allOptions.Add("tls", "true");
                }
            }
        }

        private void ExtractDatabaseName(Match match)
        {
            var databaseGroup = match.Groups["database"];
            if (databaseGroup.Success)
            {
                _databaseName = Uri.UnescapeDataString(databaseGroup.Value);
            }
        }

        private void ExtractHosts(Match match)
        {
            int defaultPort = defaultMongoDBPort;
            if (_scheme == ConnectionStringScheme.MongoDBPlusSrv)
            {
                defaultPort = defaultSrvPort;
            }
            List<EndPoint> endPoints = new List<EndPoint>();
            foreach (Capture host in match.Groups["host"].Captures)
            {
                if (_scheme == ConnectionStringScheme.MongoDBPlusSrv && Regex.IsMatch(host.Value, @".*:\d+"))
                {
                    throw new MongoConfigurationException("Host for mongodb+srv scheme cannot specify a port.");
                }

                EndPoint endPoint;
                if (EndPointHelper.TryParse(host.Value, defaultPort, out endPoint))
                {
                    endPoints.Add(endPoint);
                }
                else
                {
                    throw new MongoConfigurationException(string.Format("Host '{0}' is not valid.", host.Value));
                }
            }

            if (_scheme == ConnectionStringScheme.MongoDBPlusSrv && endPoints.Count > 1)
            {
                throw new MongoConfigurationException("Only 1 host is allowed when using the mongodb+srv scheme.");
            }

            _hosts = endPoints;
        }

        private void ExtractOptions(Match match)
        {
            foreach (Capture option in match.Groups["option"].Captures)
            {
                var parts = option.Value.Split('=');
                var name = parts[0].Trim();
                var value = parts[1].Trim();
                // Should not decode authmechanismproperties before splitting by separator.
                if (!string.Equals(name, "authmechanismproperties", StringComparison.OrdinalIgnoreCase))
                {
                    value = Uri.UnescapeDataString(value);
                }

                _allOptions.Add(name, value);
                ParseOption(name, value);
            }
        }

        private void ExtractUsernameAndPassword(Match match)
        {
            var usernameGroup = match.Groups["username"];
            if (usernameGroup.Success)
            {
                _username = Uri.UnescapeDataString(usernameGroup.Value);
            }

            var passwordGroup = match.Groups["password"];
            if (passwordGroup.Success)
            {
                _password = Uri.UnescapeDataString(passwordGroup.Value);
            }
        }

        private string GetHostNameForDns()
        {
            string host;
            if (_hosts[0] is DnsEndPoint)
            {
                host = ((DnsEndPoint)_hosts[0]).Host;
            }
            else if (_hosts[0] is IPEndPoint)
            {
                host = ((IPEndPoint)_hosts[0]).Address.ToString();
            }
            else
            {
                throw new MongoConfigurationException($"Host {_hosts[0]} is invalid");
            }

            return host;
        }

        private void Parse()
        {
            const string serverPattern = @"(?<host>((\[[^]]+?\]|[^:@,/?#]+)(:\d+)?))";
            const string serversPattern = serverPattern + @"(," + serverPattern + ")*";
            const string databasePattern = @"(?<database>[^/?]+)";
            const string optionPattern = @"(?<option>[^&;]+=[^&;]+)";
            const string optionsPattern = @"(\?" + optionPattern + @"((&|;)" + optionPattern + ")*)?";
            const string pattern =
                @"^(?<scheme>mongodb|mongodb\+srv)://" +
                @"((?<username>[^:@/]+)(:(?<password>[^:@/]*))?@)?" +
                serversPattern + @"(/" + databasePattern + ")?/?" + optionsPattern + "$";

            if (_originalConnectionString.Contains("%"))
            {
                var invalidPercentPattern = @"%$|%.$|%[^0-9a-fA-F]|%[0-9a-fA-F][^0-9a-fA-F]";
                if (Regex.IsMatch(_originalConnectionString, invalidPercentPattern))
                {
                    var protectedConnectionString = ProtectConnectionString(_originalConnectionString);
                    var message = $"The connection string '{protectedConnectionString}' contains an invalid '%' escape sequence.";
                    throw new MongoConfigurationException(message);
                }
            }

            var match = Regex.Match(_originalConnectionString, pattern);
            if (!match.Success)
            {
                var protectedConnectionString = ProtectConnectionString(_originalConnectionString);
                var message = $"The connection string '{protectedConnectionString}' is not valid.";
                throw new MongoConfigurationException(message);
            }

            ExtractUsernameAndPassword(match);
            ExtractDatabaseName(match);
            ExtractOptions(match);
            ExtractScheme(match);
            ExtractHosts(match);

            if (_journal.HasValue && _journal.Value && _w != null && _w.Equals(0))
            {
                throw new MongoConfigurationException("This is an invalid w and journal pair.");
            }

            if (_tlsInsecure.HasValue && _tlsDisableCertificateRevocationCheck.HasValue)
            {
                throw new MongoConfigurationException(
                    "Specifying both tlsInsecure and tlsDisableCertificateRevocationCheck is invalid.");
            }

            if (_scheme == ConnectionStringScheme.MongoDBPlusSrv && _directConnection)
            {
                throw new MongoConfigurationException("Direct connect cannot be used with SRV.");
            }

            if (_hosts.Count > 1 && _directConnection)
            {
                throw new MongoConfigurationException("Direct connect cannot be used with multiple host names.");
            }

            if (!_isInternalRepresentation && _srvMaxHosts > 0 && _scheme != ConnectionStringScheme.MongoDBPlusSrv)
            {
                throw new MongoConfigurationException("srvMaxHosts can only be used with the mongodb+srv scheme.");
            }

            if (_replicaSet != null && _srvMaxHosts > 0)
            {
                throw new MongoConfigurationException("Specifying srvMaxHosts when connecting to a replica set is invalid.");
            }

            if (!_isInternalRepresentation && _srvServiceName != null && _scheme != ConnectionStringScheme.MongoDBPlusSrv)
            {
                throw new MongoConfigurationException("Specifying srvServiceName is only allowed with the mongodb+srv scheme.");
            }

            if (_loadBalanced)
            {
                if (_hosts.Count > 1)
                {
                    throw new MongoConfigurationException("Load balanced mode cannot be used with multiple host names.");
                }

                if (_replicaSet != null)
                {
                    throw new MongoConfigurationException("ReplicaSetName cannot be used with load balanced mode.");
                }

                if (_srvMaxHosts > 0)
                {
                    throw new MongoConfigurationException("srvMaxHosts cannot be used with load balanced mode.");
                }

                if (_directConnection)
                {
                    throw new MongoConfigurationException("Load balanced mode cannot be used with direct connection.");
                }
            }

            string ProtectConnectionString(string connectionString)
            {
                var protectedString = Regex.Replace(connectionString, @"(?<=://)[^/]*(?=@)", "<hidden>");
                return protectedString;
            }
        }

        private void ParseOption(string name, string value)
        {
            switch (name.ToLowerInvariant())
            {
                case "appname":
                    string invalidApplicationNameMessage;
                    if (!ApplicationNameHelper.IsApplicationNameValid(value, out invalidApplicationNameMessage))
                    {
                        throw new MongoConfigurationException(invalidApplicationNameMessage);
                    }
                    _applicationName = value;
                    break;
                case "authmechanism":
                    _authMechanism = value;
                    break;
                case "authmechanismproperties":
                    foreach (var property in GetAuthMechanismProperties(name, value))
                    {
                        _authMechanismProperties.Add(property.Key, property.Value);
                    }
                    break;
                case "authsource":
                    _authSource = value;
                    break;
                case "compressors":
                    _compressorsOptions.SaveCompressors(name, value.Split(','));
                    break;
                case "connecttimeout":
                case "connecttimeoutms":
                    _connectTimeout = ParseTimeSpan(name, value);
                    break;
                case "directconnection":
                    _directConnection = ParseBoolean(name, value);
                    break;
                case "fsync":
                    _fsync = ParseBoolean(name, value);
                    break;
                case "gssapiservicename":
                    _authMechanismProperties.Add("SERVICE_NAME", value);
                    break;
                case "heartbeatfrequency":
                case "heartbeatfrequencyms":
                case "heartbeatinterval":
                case "heartbeatintervalms":
                    _heartbeatInterval = ParseTimeSpan(name, value);
                    break;
                case "heartbeattimeout":
                case "heartbeattimeoutms":
                    _heartbeatTimeout = ParseTimeSpan(name, value);
                    break;
                case "ipv6":
                    _ipv6 = ParseBoolean(name, value);
                    break;
                case "j":
                case "journal":
                    _journal = ParseBoolean(name, value);
                    break;
                case "loadbalanced":
                    _loadBalanced = ParseBoolean(name, value);
                    break;
                case "maxconnecting":
                    _maxConnecting = ParseInt32(name, value);
                    break;
                case "maxidletime":
                case "maxidletimems":
                    _maxIdleTime = ParseTimeSpan(name, value);
                    break;
                case "maxlifetime":
                case "maxlifetimems":
                    _maxLifeTime = ParseTimeSpan(name, value);
                    break;
                case "maxpoolsize":
                    _maxPoolSize = ParseInt32(name, value);
                    break;
                case "maxstaleness":
                case "maxstalenessseconds":
                    _maxStaleness = ParseTimeSpan(name, value);
                    if (_maxStaleness.Value == TimeSpan.FromSeconds(-1))
                    {
                        _maxStaleness = null;
                    }
                    break;
                case "minpoolsize":
                    _minPoolSize = ParseInt32(name, value);
                    break;
                case "readconcernlevel":
                    _readConcernLevel = ParseEnum<ReadConcernLevel>(name, value);
                    break;
                case "readpreference":
                    _readPreference = ParseEnum<ReadPreferenceMode>(name, value);
                    break;
                case "readpreferencetags":
                    var tagSet = ParseReadPreferenceTagSets(name, value);
                    if (_readPreferenceTags == null)
                    {
                        _readPreferenceTags = new List<TagSet> { tagSet }.AsReadOnly();
                    }
                    else
                    {
                        _readPreferenceTags = _readPreferenceTags.Concat(new[] { tagSet }).ToList();
                    }
                    break;
                case "replicaset":
                    _replicaSet = value;
                    break;
                case "retryreads":
                    _retryReads = ParseBoolean(name, value);
                    break;
                case "retrywrites":
                    _retryWrites = ParseBoolean(name, value);
                    break;
                case "safe":
                    var safe = ParseBoolean(name, value);
                    if (_w == null)
                    {
                        _w = safe ? 1 : 0;
                    }
                    else
                    {
                        if (safe)
                        {
                            // don't overwrite existing W value unless it's 0
                            var wCount = _w as WriteConcern.WCount;
                            if (wCount != null && wCount.Value == 0)
                            {
                                _w = 1;
                            }
                        }
                        else
                        {
                            _w = 0;
                        }
                    }
                    break;
                case "localthreshold":
                case "localthresholdms":
                case "secondaryacceptablelatency":
                case "secondaryacceptablelatencyms":
                    _localThreshold = ParseTimeSpan(name, value);
                    break;
                case "serverselectiontimeout":
                case "serverselectiontimeoutms":
                    _serverSelectionTimeout = ParseTimeSpan(name, value);
                    break;
                case "servermonitoringmode":
                    _serverMonitoringMode = ParseEnum<ServerMonitoringMode>(name, value);
                    break;
                case "sockettimeout":
                case "sockettimeoutms":
                    _socketTimeout = ParseTimeSpan(name, value);
                    break;
                case "srvmaxhosts":
                    var srvMaxHostsValue = ParseInt32(name, value);
                    if (srvMaxHostsValue < 0)
                    {
                        throw new MongoConfigurationException("srvMaxHosts must be greater than or equal to 0.");
                    }
                    _srvMaxHosts = srvMaxHostsValue;
                    break;
                case "srvservicename":
                    _srvServiceName = value;
                    if (_srvServiceName.Length == 0)
                    {
                        throw new MongoConfigurationException("SrvServiceName cannot be empty.");
                    }
                    break;
                case "ssl": // Obsolete
                case "tls":
                    var tlsValue = ParseBoolean(name, value);
                    if (_tls.HasValue && _tls.Value != tlsValue)
                    {
                        throw new MongoConfigurationException("tls has already been configured with a different value.");
                    }
                    _tls = tlsValue;
                    break;
                case "sslverifycertificate": // Obsolete
                    var sslVerifyCertificateValue = ParseBoolean(name, value);
                    _tlsInsecure = EnsureTlsInsecureIsValid(!sslVerifyCertificateValue);
                    break;
#if DEBUG // TODO: CSOT: Make it public when CSOT will be ready for GA
                case "timeout":
                case "timeoutms":
                    _timeout = value == "0" ? System.Threading.Timeout.InfiniteTimeSpan : ParseTimeSpan(name, value);
                    break;
#endif
                case "tlsdisablecertificaterevocationcheck":
                    var tlsDisableCertificateRevocationCheckValue = ParseBoolean(name, value);
                    _tlsDisableCertificateRevocationCheck =
                        EnsureTlsDisableCertificateRevocationCheckIsValid(tlsDisableCertificateRevocationCheckValue);
                    break;
                case "tlsinsecure":
                    var tlsInsecureValue = ParseBoolean(name, value);
                    _tlsInsecure = EnsureTlsInsecureIsValid(tlsInsecureValue);
                    break;
                case "w":
                    _w = WriteConcern.WValue.Parse(value);
                    break;
                case "wtimeout":
                case "wtimeoutms":
                    _wTimeout = ParseTimeSpan(name, value);
                    if (_wTimeout < TimeSpan.Zero)
                    {
                        throw new MongoConfigurationException($"{name} must be greater than or equal to 0.");
                    }
                    break;
                case "waitqueuemultiple":
                    _waitQueueMultiple = ParseDouble(name, value);
                    break;
                case "waitqueuesize":
                    _waitQueueSize = ParseInt32(name, value);
                    break;
                case "waitqueuetimeout":
                case "waitqueuetimeoutms":
                    _waitQueueTimeout = ParseTimeSpan(name, value);
                    break;
                case "zlibcompressionlevel":
                    _compressorsOptions.SaveCompressionOption("Level", ParseInt32(name, value), CompressorType.Zlib);
                    break;
                default:
                    _unknownOptions.Add(name, value);
                    break;
            }
        }

        // private static methods
        private static IEnumerable<KeyValuePair<string, string>> GetAuthMechanismProperties(string name, string value)
        {
            foreach (var property in value.Split(','))
            {
                var unescapedProperty = Uri.UnescapeDataString(property);
                var separatorPosition = unescapedProperty.IndexOf(':');
                if (separatorPosition == -1)
                {
                    throw new MongoConfigurationException($"{name} has an invalid value of {value}.");
                }

                var propertyKey = unescapedProperty.Substring(0, separatorPosition);
                var propertyValue = unescapedProperty.Substring(separatorPosition + 1);

                yield return new KeyValuePair<string, string>(propertyKey, propertyValue);
            }
        }

        private static bool ParseBoolean(string name, string value)
        {
            try
            {
                return JsonConvert.ToBoolean(value.ToLower());
            }
            catch (Exception ex)
            {
                throw new MongoConfigurationException(string.Format("{0} has an invalid boolean value of {1}.", name, value), ex);
            }
        }

        internal static double ParseDouble(string name, string value)
        {
            try
            {
                return JsonConvert.ToDouble(value);
            }
            catch (Exception ex)
            {
                throw new MongoConfigurationException(string.Format("{0} has an invalid double value of {1}.", name, value), ex);
            }
        }

        private static TEnum ParseEnum<TEnum>(string name, string value)
            where TEnum : struct
        {
            try
            {
                return (TEnum)Enum.Parse(typeof(TEnum), value, true);
            }
            catch (Exception ex)
            {
                throw new MongoConfigurationException(string.Format("{0} has an invalid {1} value of {2}.", name, typeof(TEnum), value), ex);
            }
        }

        private static int ParseInt32(string name, string value)
        {
            try
            {
                return JsonConvert.ToInt32(value);
            }
            catch (Exception ex)
            {
                throw new MongoConfigurationException(string.Format("{0} has an invalid int32 value of {1}.", name, value), ex);
            }
        }

        private static TagSet ParseReadPreferenceTagSets(string name, string value)
        {
            var tags = new List<Tag>();
            foreach (var tagString in value.Split(','))
            {
                var parts = tagString.Split(':');
                if (parts.Length != 2)
                {
                    throw new MongoConfigurationException(string.Format("{0} has an invalid value of {1}.", name, value));
                }
                var tag = new Tag(parts[0].Trim(), parts[1].Trim());
                tags.Add(tag);
            }
            return new TagSet(tags);
        }

        private static TimeSpan ParseTimeSpan(string name, string value)
        {
            // all timespan keys can be suffixed with 'MS'
            var lowerName = name.ToLower();
            var lowerValue = value.ToLower();
            var end = lowerValue.Length - 1;

            var multiplier = 1000; // default units are seconds
            if (lowerName.EndsWith("ms", StringComparison.Ordinal))
            {
                multiplier = 1;
            }
            else if (lowerName.EndsWith("seconds", StringComparison.Ordinal))
            {
                multiplier = 1000;
            }
            else if (lowerValue.EndsWith("ms", StringComparison.Ordinal))
            {
                lowerValue = lowerValue.Substring(0, lowerValue.Length - 2);
                multiplier = 1;
            }
            else if (lowerValue[end] == 's')
            {
                lowerValue = lowerValue.Substring(0, lowerValue.Length - 1);
                multiplier = 1000;
            }
            else if (lowerValue[end] == 'm')
            {
                lowerValue = lowerValue.Substring(0, lowerValue.Length - 1);
                multiplier = 60 * 1000;
            }
            else if (lowerValue[end] == 'h')
            {
                lowerValue = lowerValue.Substring(0, lowerValue.Length - 1);
                multiplier = 60 * 60 * 1000;
            }
            else if (lowerValue.IndexOf(':') != -1)
            {
                try
                {
                    return TimeSpan.Parse(lowerValue);
                }
                catch (Exception ex)
                {
                    throw new MongoConfigurationException(string.Format("{0} has an invalid TimeSpan value of {1}.", name, value), ex);
                }
            }

            try
            {
                return TimeSpan.FromMilliseconds(multiplier * JsonConvert.ToDouble(lowerValue));
            }
            catch (Exception ex)
            {
                throw new MongoConfigurationException(string.Format("{0} has an invalid TimeSpan value of {1}.", name, value), ex);
            }
        }

        private bool EnsureTlsDisableCertificateRevocationCheckIsValid(bool value)
        {
            if (_tlsDisableCertificateRevocationCheck.HasValue && _tlsDisableCertificateRevocationCheck.Value != value)
            {
                var name = nameof(_tlsDisableCertificateRevocationCheck).Substring(1);
                throw new MongoConfigurationException($"{name} has already been configured with a different value.");
            }

            return value;
        }

        private bool EnsureTlsInsecureIsValid(bool value)
        {
            if (_tlsInsecure.HasValue && _tlsInsecure.Value != value)
            {
                throw new MongoConfigurationException("tlsInsecure has already been configured with a different value.");
            }

            return value;
        }

        private List<string> GetHostsFromSrvRecords(IEnumerable<SrvRecord> srvRecords)
        {
            var hosts = new List<string>();
            foreach (var srvRecord in srvRecords)
            {
                var h = srvRecord.EndPoint.Host;
                if (h.EndsWith(".", StringComparison.Ordinal))
                {
                    h = h.Substring(0, h.Length - 1);
                }
                hosts.Add(h + ":" + srvRecord.EndPoint.Port);
            }

            if (_srvMaxHosts > 0)
            {
                FisherYatesShuffle.Shuffle(hosts);
            }

            return hosts;
        }

        private NameValueCollection GetResolvedOptionsFromTxtRecords(IReadOnlyCollection<TxtRecord> txtRecords)
        {
            if (txtRecords.Count > 1)
            {
                throw new MongoConfigurationException("Only 1 TXT record is allowed when using the SRV protocol.");
            }

            var txtRecord = txtRecords.FirstOrDefault();

            // Build a dummy connection string in order to parse the options
            var dummyConnectionString = "mongodb://localhost/";
            if (txtRecord != null)
            {
                dummyConnectionString += "?" + string.Join("", txtRecord.Strings);
            }
            var dnsConnectionString = new ConnectionString(dummyConnectionString);
            ValidateResolvedOptions(dnsConnectionString.AllOptionNames);
            return dnsConnectionString._allOptions;
        }

        private void ValidateResolvedHosts(string original, List<string> resolved)
        {
            if (resolved.Count == 0)
            {
                throw new MongoConfigurationException($"No hosts were found in the SRV record for {original}.");
            }

            // for each resolved host, make sure that it ends with domain of the parent.
            foreach (var resolvedHost in resolved)
            {
                EndPoint endPoint;
                if (!EndPointHelper.TryParse(resolvedHost, 0, out endPoint) || !(endPoint is DnsEndPoint))
                {
                    throw new MongoConfigurationException($"Unable to parse {resolvedHost} as a hostname.");
                }
                var dnsEndPoint = (DnsEndPoint)endPoint;

                var host = ((DnsEndPoint)endPoint).Host;
                if (!HasValidParentDomain(original, dnsEndPoint))
                {
                    throw new MongoConfigurationException($"Hosts in the SRV record must have the same parent domain as the seed host.");
                }
            }
        }

        internal static bool HasValidParentDomain(string original, DnsEndPoint resolvedEndPoint)
        {
            var host = resolvedEndPoint.Host;

            var hostDotCount = host.Count(c => c == '.');
            var originalDotCount = original.Count(c => c == '.');

            // If original has less than 3 dot separated parts,
            // the returned hostname must have at least one more domain level than original
            if (originalDotCount < 2)
            {
                return host.Length > original.Length &&
                       hostDotCount > originalDotCount &&
                       host.EndsWith(original, StringComparison.Ordinal) &&
                       host[host.Length - original.Length -1 ] == '.';
            }

            // We check that the returned hostname has the same domain name as original
            var originalFirstDotIndex = original.IndexOf('.');
            return hostDotCount >= originalDotCount && host.AsSpan().EndsWith(original.AsSpan(originalFirstDotIndex), StringComparison.Ordinal);
        }

        private void ValidateResolvedOptions(IEnumerable<string> optionNames)
        {
            var allowedOptionsInTxtRecords = new[] { "authSource", "replicaSet", "loadBalanced" };
            if (optionNames.Any(o => !allowedOptionsInTxtRecords.Contains(o, StringComparer.OrdinalIgnoreCase)))
            {
                throw new MongoConfigurationException($"Only {string.Join(", ", allowedOptionsInTxtRecords)} are allowed in a TXT record.");
            }
        }

        // nested types
        private class CompressorsOptions
        {
            private readonly List<CompressorConfiguration> _compressors;
            private readonly Dictionary<CompressorType, Dictionary<string, object>> _compressorsOptions;
            private bool _hasBeenBuilt;
            private readonly NameValueCollection _unknownOptions;

            public CompressorsOptions(NameValueCollection unknownOptions)
            {
                _compressorsOptions = new Dictionary<CompressorType, Dictionary<string, object>>();
                _compressors = new List<CompressorConfiguration>();
                _unknownOptions = unknownOptions;
            }

            public IReadOnlyList<CompressorConfiguration> Compressors
            {
                get
                {
                    BuildIfNotBuilt();
                    return _compressors;
                }
            }

            public void SaveCompressionOption<TOptionType>(string option, TOptionType value, CompressorType compressorType)
            {
                if (!_compressorsOptions.TryGetValue(compressorType, out var properties))
                {
                    properties = new Dictionary<string, object>();
                    _compressorsOptions.Add(compressorType, properties);
                }

                properties.Add(option, value);
            }

            public void SaveCompressors(string name, string[] compressorNames)
            {
                foreach (var compressor in compressorNames)
                {
                    // NOTE: the 'noop' is also expected by the server
                    if (!CompressorTypeMapper.TryFromServerName(compressor, out CompressorType compressorType) || !CompressorSource.IsCompressorSupported(compressorType))
                    {
                        _unknownOptions.Add(name, compressor);

                        // Keys that aren't supported by a driver MUST be ignored.
                        continue;
                    }
                    _compressors.Add(new CompressorConfiguration(compressorType));
                }
            }

            // private methods
            private void BuildIfNotBuilt()
            {
                if (!_hasBeenBuilt)
                {
                    MapCompressorsAndProperties();
                    _hasBeenBuilt = true;
                }
            }

            private void MapCompressorsAndProperties()
            {
                foreach (var compressor in _compressors)
                {
                    if (_compressorsOptions.TryGetValue(compressor.Type, out var options))
                    {
                        foreach (var option in options)
                        {
                            compressor.Properties.Add(option);
                        }
                    }
                }
            }
        }
    }
}
