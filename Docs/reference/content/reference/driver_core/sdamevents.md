+++
date = "2019-04-17T21:21:00Z"
draft = false
title = "SDAM Events"
[menu.main]
  parent = "Eventing"
  identifier = "SdamEvents"
  weight = 90
  pre = "<i class='fa'></i>"
+++

## SDAM Events

Server Discovery and Monitoring (SDAM) is the process by which the driver discovers and monitors the set of servers that it is connected to. In
the case of a standalone configuration the driver will only monitor a single server. In the case of a replica set configuration the driver
will monitor each member of the replica set (primary, secondaries, etc...). In the case of a sharded configuration the driver will  
monitor the set of shard routers (mongos instances) that it is connected to.

As the driver monitors the health and state of each server in a configuration it raises a number of events that report what it is finding and
how it is reacting to that information. You can subscribe to any or all of these events if you want to observe what SDAM is doing.

See the general [Eventing]({{< relref "events.md" >}}) page for information on how to subscribe to individual events.

### Logging SDAM Events the Easy Way

Often all you want to do with SDAM events is log them. You can configure logging of SDAM events by setting the `SdamLogFilename` property of `MongoClientSettings`.

```csharp
var clientSettings = new MongoClientSettings();
clientSetting.SdamLogFilename = @"c:\sdam.log";
var client = new MongoClient(clientSettings);
```

The `SdamLogFilename` is only configurable in code, not in the connection string. The way to combine using a connection string with SDAM logging is:

```
var connectionString = "mongodb://localhost"; // presumably loaded via some config mechanism
var clientSettings = MongoClientSettings.FromConnectionString(connectionString);
clientSetting.SdamLogFilename = @"c:\sdam.log";
var client = new MongoClient(clientSettings);
```

{{% note %}}
Logging SDAM events results in the file being opened in exclusive mode, so it is possible for multiple instances of `MongoClient` to conflict
over the use of the file. Whether the multiple `MongoClient` instances actually conflict over the use of the file or not depens on how similar
the `MongoClientSettings` are. If they are different enough to result in the creation of separate underlying `Cluster` instances then they will conflict.

The safest approach when logging SDAM events is to use a single instance of `MongoClient` throughout 
your application. This differs from previous guidance which stated that it didn't matter how many instances of `MongoClient` you created.
{{% /note %}}

### SDAM Events That Are Logged

SDAM logging logs the following events which are raised in the course of monitoring the servers the driver is connected to.

#### ClusterAddedServerEvent

Raised after a server has been added to the cluster.

#### ClusterAddingServerEvent

Raised before a server is added to the cluster.

#### ClusterClosedEvent

Raised after a cluster has been closed.

#### ClusterClosingEvent

Raised before a cluster is closed.

#### ClusterDescriptionChangedEvent

Raised when the cluster description changes.

#### ClusterOpenedEvent

Raised after a cluster has been opened.

#### ClusterOpeningEvent

Raised before a cluster is opened.

#### ClusterRemovedServerEvent

Raised after a server has been removed from the cluster.

#### ClusterRemovingServerEvent

Raised before a server is removed from the cluster.

#### SdamInformationEvent

Raised when something interesting happened that is not covered by a custom event type.

#### ServerClosedEvent

Raised after a server has been closed.

#### ServerClosingEvent

Raised before a server is closed.

#### ServerDescriptionChangedEvent

Raised when the server description has changed.

#### ServerHeartbeatFailedEvent

Raised after a heartbeat has failed.

#### ServerHeartbeatStartedEvent

Raised after a heartbeat has started (but before the heartbeat is sent to the server).

#### ServerHeartbeatSucceededEvent

Raised after a heartbeat has succeeded.

#### ServerOpenedEvent

Raised after a server has been opened.

#### ServerOpeningEvent

Raised before a server is opened.
