Zabbix Agent .Net
=================

[![Build status](https://ci.appveyor.com/api/projects/status/75pdxc9bq693ts02?svg=true)](https://ci.appveyor.com/project/vbfox/zabbixagentlib)

A .Net library implementing a [Zabbix](http://www.zabbix.com/) agent.

Only passive agents are supported for now (Where zabbix create one connection
per item he want to request)

Passive agent sample
--------------------

```csharp
var server = new PassiveCheckServer(new IPEndPoint(0, 10050));

server.AddItem("test", () => 42);
server.AddItem("echo", a => a);

server.Start();
```