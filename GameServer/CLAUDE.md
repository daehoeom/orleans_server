# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

Build/run everything from the repo root (`GameServer.sln`), targeting `net9.0`.

```
dotnet build GameServer.sln              # build all 6 projects
dotnet build <Project>/<Project>.csproj  # build a single project

dotnet run --project FrontServer         # main game server: Orleans silo + DotNetty TCP on :35000
dotnet run --project ApiServer           # HTTP API (Kestrel)
dotnet run --project ScenarioBot         # load-test bot client, connects to 127.0.0.1:35000
```

There is no test project in the solution (`dotnet test` finds nothing).

`FrontServer` requires a local MySQL (`account` and `game` databases) and local Redis reachable before it will start — `DatabaseService.CheckConnectionAsync()` / `RedisService.ConnectAsync()` run during `ServerRunner.StartAsync` and throw if unreachable. Connection settings (host/port/db/user/password) are hardcoded defaults in `Database/Db/DbSetting.cs` and `Database/Redis/RedisSetting.cs`, not read from `appsettings.json` — edit those records directly to point at a different environment.

## Solution layout

Six projects, dependency direction flows one way (`SharedLibrary` ← `Database`/`GrainLibrary` ← `FrontServer`/`ApiServer`):

- **SharedLibrary** — wire-protocol contracts only, referenced by every other project. No dependencies.
- **Database** — Dapper + MySqlConnector + StackExchange.Redis data access. No EF Core. Depends on SharedLibrary.
- **GrainLibrary** — Orleans grains, the DotNetty server/dispatch pipeline, and session management. Depends on Database.
- **FrontServer** — the actual game server process (confusingly named; hosts the Orleans silo + the DotNetty TCP listener). Depends on Database, GrainLibrary, SharedLibrary.
- **ApiServer** — separate HTTP API process, currently a thin scaffold (one controller, `SessionController`, stubbed). Depends on Database, GrainLibrary, SharedLibrary.
- **ScenarioBot** — standalone console load-test client with its own minimal packet pipeline (`BotClientHandler`/`BotPacketTranslator`), driven by pluggable `IScenario` implementations and `BotManager` for concurrent bots.

## Request/response pipeline (FrontServer)

This is the core flow to understand before touching packet handling; it spans SharedLibrary, GrainLibrary, and FrontServer:

1. `GrainLibrary/Server/ServerRunner.cs` binds a DotNetty TCP server (`127.0.0.1:35000`). Its channel pipeline: `LengthFieldBasedFrameDecoder` (4-byte length prefix) → `PacketDecoder` (reads a 4-byte `PacketHeaderType` + MessagePack body into a `StreamPacket`) → `PacketEncoder` (MessagePack-serializes outgoing objects, writes length + body) → `GameServerHandler`.
2. `GameServerHandler.ChannelActive` registers a `PlayerSession` in `SessionService` and starts an auth timeout; `ChannelRead0` forwards each `StreamPacket` to `PacketHandler.DispatchAsync`.
3. `PacketHandler` is built at startup by reflecting over every DI-registered `PlayerBaseController`-derived singleton, collecting methods tagged `[PacketHandler(PacketHeaderType.X)]` into a `Dictionary<PacketHeaderType, Router>`. On dispatch it deserializes the MessagePack body into the handler's request type and invokes the method via reflection with `(PlayerSession, TRequest)`.
4. Controller methods (`FrontServer/Controllers/*.cs`) hold game logic — typically calling into Orleans grains via the injected `IClusterClient` (e.g. `IPlayerGrain`) — then reply via `SendAsync` (request/response; header resolved from the response DTO's `[Response(...)]` attribute through `ResponseHeaderCache<T>`) or `NotifyAsync`/`PlayerSession.Notify` (server push, via `[Notify(...)]`/`NotifyHeaderCache<T>`).
5. The Orleans silo is co-hosted in-process with FrontServer via `UseLocalhostClustering()` — single-node dev clustering, not a real multi-silo cluster.

To add a new packet: add a `PacketHeaderType` entry (`SharedLibrary/PacketHeaderType.cs`), add request/response/notify DTOs under `SharedLibrary/Packet/Tcp` (or `Packet/Http` for ApiServer) tagging outgoing DTOs with `[Response(...)]`/`[Notify(...)]`, add a `[PacketHandler(...)]`-tagged method with signature `(PlayerSession, TRequest)` to a `PlayerBaseController` subclass, and register that controller as a singleton in `FrontServer/Program.cs` (`services.AddSingleton<PlayerBaseController, YourController>()`).

Session lookup: `SessionService` keeps sessions both by DotNetty channel context and by numeric session id (`ConcurrentDictionary`), and exposes `Broadcast<T>` for server-wide pushes (see `CommunityController.SendChatAsync` for channel-broadcast vs. whisper-to-session-id patterns).

## Database layer

No ORM — `DbConnector` (`Database/Db/DbConnector.cs`) wraps `MySqlConnection` and serializes all work for a given connector through a single-reader `Channel`-based queue (`ProcessQueueAsync`), so operations on one `DbConnector` execute one-at-a-time on a dedicated background loop rather than concurrently across the pool. Two independent `DbConnector`s exist, one per database, wired up in `DatabaseService`:

- `AccountDbContext` — account DB (`AccountDbSet`, etc.)
- `GameDbContext` — game DB (`PlayerDbSet`, `PlayerWalletDbSet`, `PlayerPurchaseLimitDbSet`)

`DbSet` classes hand-write raw SQL via Dapper against `Row` POCOs. Row types are tagged `[Table("name")]` (`Database/Db/Attribute/TableAttribute.cs`), which `DbConnector.InsertAsync`/`BulkInsertAsync` use via reflection to build INSERT statements from all public properties — column names in the row class must match the table schema exactly. Use `WithTransactionAsync`/`ExecuteTransactionAsync` for multi-statement transactions.

`RedisService` wraps StackExchange.Redis for distributed locks (`TryAcquireLockAsync`/`ReleaseLockAsync`, Lua-script compare-and-delete) and typed sets like `PlayerAccessTokenSet`.

## Packet serialization details

All wire types are MessagePack objects (`[MessagePackObject]`/`[Key(n)]`). `BasePacket.cs` defines the three envelope shapes (`BaseRequestPacket`, `BaseResponsePacket<T>`, `BaseNtfPacket<T>`); every concrete packet DTO under `SharedLibrary/Packet/Tcp` and `Packet/Http` is a plain payload type wrapped in one of these envelopes at the transport layer, not by extending them directly. `PacketHeaderType` (`SharedLibrary/PacketHeaderType.cs`) is the single source of truth mapping every request/response/notify to a numeric header id used in the 4-byte frame prefix.
