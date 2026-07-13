# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Scope

`GrainLibrary` is a class library, not a runnable process — it has no `Program.cs`. It provides the transport pipeline, packet dispatch machinery, session management, and Orleans grains that `FrontServer` hosts. It depends on `Database` and `SharedLibrary`; build it with `dotnet build GrainLibrary/GrainLibrary.csproj`, but there's nothing to run standalone — exercise it through `dotnet run --project ../FrontServer`.

## Transport pipeline (`Server/`)

`ServerRunner` is a `BackgroundService` that binds a DotNetty TCP listener on `127.0.0.1:35000` in `StartAsync` (after checking DB/Redis connectivity via the injected `DatabaseService`/`RedisService`), and builds this channel pipeline per connection:

`LengthFieldBasedFrameDecoder` (4-byte length prefix, stripped) → `PacketDecoder` → `PacketEncoder` → `GameServerHandler`.

The two directions are asymmetric — worth knowing before touching the wire format:
- **Inbound** (`PacketDecoder` in `Services/PacketTranslator.cs`): reads a 4-byte `PacketHeaderType` *outside* the MessagePack body, then treats the rest of the frame as the raw MessagePack-encoded payload (`StreamPacket { HeaderType, Body }`). The body isn't deserialized here — `PacketHandler` deserializes it later once it knows the target request type.
- **Outbound** (`PacketEncoder`): MessagePack-serializes the *entire* envelope object (e.g. `BaseResponsePacket<T>`, which carries its own `HeaderType` as a `[Key]` field), then manually writes a 4-byte length prefix + that body. There's no separate outer header-type field on the wire for outgoing packets — the header type only exists inside the serialized body. There's also no `LengthFieldPrepender` in the pipeline; `PacketEncoder` does the length-prefixing itself.

`GameServerHandler` ties the pipeline to session lifecycle: `ChannelActive` registers a `PlayerSession` via `SessionService.AddContext` and starts a 10s auth timeout (`PlayerSession.StartAuthTimeout` closes the channel if it fires); `ChannelRead0` fire-and-forgets each `StreamPacket` into `PacketHandler.DispatchAsync`, logging (not propagating) faults; `ChannelInactive`/`ExceptionCaught` remove the session.

## Packet dispatch (`Services/PacketHandler.cs`, `Router.cs`)

`PacketHandler` is constructed with `IEnumerable<PlayerBaseController>` — every packet controller registered in the host's DI container (this happens in `FrontServer/Program.cs`, not here; `GrainLibrary` only defines the extensibility point). At construction it reflects over each controller for methods tagged `[PacketHandler(PacketHeaderType.X)]`, validates the signature is exactly `(PlayerSession, TRequest)`, and stores `{Type, Target, MethodInfo}` in a `Dictionary<PacketHeaderType, Router>`. Registering two handlers for the same `PacketHeaderType` throws at startup. `DispatchAsync` looks up the session, looks up the router by `HeaderType`, MessagePack-deserializes the body into `Router.Type`, and invokes the method via reflection.

`PlayerBaseController` (`Services/PlayerBaseController.cs`) is the abstract base every packet controller extends (concrete subclasses live in `FrontServer/Controllers`, not here). It carries the injected `IClusterClient` for calling into grains, and exposes `SendAsync<T>`/`NotifyAsync<T>` helpers that wrap a payload in `BaseResponsePacket<T>`/`BaseNtfPacket<T>` and resolve the header type from the payload's `[Response]`/`[Notify]` attribute via `ResponseHeaderCache<T>`/`NotifyHeaderCache<T>`.

## Session management (`Services/SessionService.cs`, `Models/PlayerSession.cs`)

`SessionService` tracks sessions two ways: by DotNetty `IChannelHandlerContext` (set on connect, before auth) and by numeric session id (set once authenticated via `AddSession`). `Broadcast<T>` pushes a notify packet to every currently-tracked session — used e.g. by `CommunityController.SendChatAsync` for channel-wide chat.

**Known bug**: `AddSession` logs `logger.LogError("Failed to add session...")` unconditionally on the success path (after `_sessions.TryAdd` succeeds), not just on failure — the log statement and the `return null` failure paths above it got separated incorrectly. Don't treat that log line as a real failure signal, and fix the misplaced log if you're in this method for another reason.

`PlayerSession` holds the channel reference and session id, and provides its own `SendAsync`/`Notify` instance methods (used when you already have a session reference rather than going through a controller, e.g. whispering to a specific session in `CommunityController`).

## Orleans grains (`Grains/`)

Only one grain exists: `PlayerGrain : Grain, IPlayerGrain` (`IGrainWithIntegerKey`). It's a stub — `FrontServer.PlayerController` activates it with `clusterClient.GetGrain<IPlayerGrain>(player.SessionId)`, i.e. **keyed by the transport session id, not a persistent player id**, and `_playerId` is never actually set, so `GetPlayerData()` always returns `PlayerId = 0`. Treat this as the pattern to extend (grain-per-player, called from controllers via `IClusterClient`) rather than a finished feature — expect to change the keying scheme to a real player id once persistence is wired up. The silo itself is hosted in-process by `FrontServer` via `UseLocalhostClustering()` (single-node dev clustering).

## Resource loading (`Resource/`)

`IDataSet<T>` (`Resource/Model/IDataSet.cs`) defines a `Load`/`PostProcess` contract for static game-data tables, and `ResourceLoader` is an empty stub — this is scaffolding for a future game-data (e.g. exported table) loading system, not yet implemented. `ServerConstants.cs`'s `RedisDbType` enum (currently empty) looks like a companion stub for routing to different Redis logical databases.

## Utility (`Utility/`)

`AtomicInt`/`AtomicLong` are thin `Interlocked`-based wrappers (get/set/increment/add/compare-and-set). `TimeUtil.UtcNow` is a one-line indirection over `DateTime.UtcNow` — use it instead of calling `DateTime.UtcNow` directly so time can be centralized/mocked later. `RandomUtil` wraps `System.Random` and adds weighted-random selection (`GetRandomWeight`) over `IntRandomWeight<T>`/`LongRandomWeight<T>` lists, e.g. for loot-table-style rolls.
