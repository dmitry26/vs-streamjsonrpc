StreamJsonRpc
===============

[![NuGet package](https://img.shields.io/nuget/v/StreamJsonRpc.svg)](https://nuget.org/packages/StreamJsonRpc)
[![Build status](https://ci.appveyor.com/api/projects/status/3qckpo5perk9r83j/branch/master?svg=true)](https://ci.appveyor.com/project/AArnott/vs-streamjsonrpc/branch/master)
[![codecov](https://codecov.io/gh/Microsoft/vs-streamjsonrpc/branch/master/graph/badge.svg)](https://codecov.io/gh/Microsoft/vs-streamjsonrpc)
[![Join the chat at https://gitter.im/vs-streamjsonrpc/Lobby](https://badges.gitter.im/vs-streamjsonrpc/Lobby.svg)](https://gitter.im/vs-streamjsonrpc/Lobby?utm_source=badge&utm_medium=badge&utm_campaign=pr-badge&utm_content=badge)

StreamJsonRpc is a cross-platform, .NET portable library that implements the
[JSON-RPC][JSONRPC] wire protocol.

It works over [Stream](https://docs.microsoft.com/en-us/dotnet/api/system.io.stream), [WebSocket](https://docs.microsoft.com/en-us/dotnet/api/system.net.websockets.websocket), or System.IO.Pipelines pipes, independent of the underlying transport.

Bonus features beyond the JSON-RPC spec include:

1. Request cancellation
1. .NET Events as notifications
1. Dynamic client proxy generation

## Supported platforms

* .NET 4.6
* .NET Standard 1.6
* .NET Standard 2.0

## Compatibility

This library has been tested with and is compatible with the following other
JSON-RPC libraries:

* [json-rpc-peer][json-rpc-peer] (npm)

## Documentation
[Documentation](doc/index.md)

## Testability/mockability

Testing this library or users of this library can be done without any transport
by using the [Nerdbank.FullDuplexStream][FullDuplexStream] library in your tests
to produce the Stream object.

[JSONRPC]: http://jsonrpc.org/
[json-rpc-peer]: https://www.npmjs.com/package/json-rpc-peer
[FullDuplexStream]: https://www.nuget.org/packages/nerdbank.fullduplexstream
