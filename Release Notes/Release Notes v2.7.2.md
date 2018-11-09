# .NET Driver Version 2.7.2 Release Notes

This is a patch release that fixes one bug reported since 2.7.1 was released.

An online version of these release notes is available at:

https://github.com/mongodb/mongo-csharp-driver/blob/v2.7.x/Release%20Notes/Release%20Notes%20v2.7.2.md

The full list of JIRA issues resolved in this release is available at:

https://jira.mongodb.org/issues/?jql=project%20%3D%20CSHARP%20AND%20fixVersion%20%3D%202.7.2%20ORDER%20BY%20key%20ASC

Documentation on the .NET driver can be found at:

http://mongodb.github.io/mongo-csharp-driver/

Upgrading

There are no known backwards breaking changes in this release.

This is a mandatory upgrade for anyone using the .NET Driver on Linux or OS X. In 2.7.1
we added some code to set and configure TCP KeepAlive. Although the methods we used to
set and configure TCP KeepAlive were documented as being supported in .NET Standard 1.5,
it turned out that they were not in fact supported on Linux and OS X. We are working
around that by attempting to set and configure TCP KeepAlive but ignoring any
PlatformNotSupportedExceptions that are thrown when doing so. While it is advantageous
for TCP KeepAlive to be turned on, the driver will work fine without it and earlier
versions of the driver did not attempt to turn on TCP KeepAlive, so ignoring the
PlatformNotSupportedException is acceptable.
