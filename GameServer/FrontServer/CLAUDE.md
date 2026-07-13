# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Scope

`FrontServer` (root namespace `GameServer`) is the actual game server process, despite its name and despite being an `Microsoft.NET.Sdk.Web` project. `Program.cs` builds a plain generic `Host` (`Host.CreateDefaultBuilder(args)`), not a `WebApplication` — it calls `.UseOrleans(siloBuilder => siloBuilder.UseLocalhostClustering())` to co-host a single-node Orleans silo, registers the DI graph for the packet pipeline, and adds `GrainLibrary.Server.ServerRunner` as the hosted `BackgroundService` that actually binds the game's DotNetty TCP listener (`127.0.0.1:35000`, hardcoded in `ServerRunner`, not read from this project's config). Run it with `dotnet run --project FrontServer`; requires local MySQL + Redis reachable first (see `Database/CLAUDE.md`) or startup throws.

**No HTTP endpoint is ever bound.** `wwwroot/` (bootstrap/jquery static assets), `Properties/launchSettings.json` (`http`/`https` profiles with `applicationUrl`), and `appsettings.json`/`appsettings.Development.json` (bare default `Logging`/`AllowedHosts`) are all unused leftovers from the original `Sdk.Web` project template — there's no Kestrel pipeline, no MVC/Razor middleware, and no `Views/` folder even though the `.csproj` still has stale `_ContentIncludedByDefault Remove` entries referencing `Views/Shared/*.cshtml` files that don't exist in the tree. Don't try to "fix" or wire up the web host here; if HTTP is needed, that's `ApiServer`'s job.

## `Controllers/` — not ASP.NET controllers

Despite the folder name and despite this being a `Sdk.Web` project, `PlayerController`, `ShopController`, and `CommunityController` are **not** MVC/API controllers (contrast with `ApiServer/Controllers/SessionController.cs`, which is a real `[ApiController]`). They're packet handlers: each extends `PlayerBaseController` (defined in `GrainLibrary/Services/PlayerBaseController.cs`) and holds methods tagged `[PacketHandler(PacketHeaderType.X)]` with signature `(PlayerSession, TRequest)`. `GrainLibrary`'s `PacketHandler` discovers these via reflection at startup from whatever gets injected as `IEnumerable<PlayerBaseController>` — see `GrainLibrary/CLAUDE.md` for the full dispatch mechanics.

The DI wiring that makes this work is in `Program.cs`:

```csharp
services.AddSingleton<PlayerBaseController, PlayerController>();
services.AddSingleton<PlayerBaseController, ShopController>();
services.AddSingleton<PlayerBaseController, CommunityController>();
```

Registering multiple implementations against the same `PlayerBaseController` service type is what populates `PacketHandler`'s `IEnumerable<PlayerBaseController>` constructor parameter. **To add a new controller, add another `AddSingleton<PlayerBaseController, YourController>()` line here** — a controller class that isn't registered here will never receive dispatched packets, even if its methods are correctly tagged.

Current controller contents:
- **`PlayerController`**: `LoadPlayer` (fetches `IPlayerGrain` via `IClusterClient`, keyed by `player.SessionId` — see the grain-keying caveat in `GrainLibrary/CLAUDE.md`) and `KeepAlive` (echoes server UTC ticks).
- **`ShopController`**: empty — no handlers registered yet, a placeholder for future shop packets (`LoadShop` exists in `PacketHeaderType` but has no handler).
- **`CommunityController`**: `SendChat`, branching on `ChatType.Channel` (broadcasts via `SessionService.Broadcast`) vs `ChatType.Whisper` (looks up the target session by id and notifies it directly, or replies `ResultCode.PlayerNotFound`). Has a `// 대화내용 필터 추가` comment marking an unimplemented chat filter.

## Adding a new packet handler

1. Add the `PacketHeaderType` and request/response/notify DTOs in `SharedLibrary` (see `SharedLibrary`'s conventions — response/notify DTOs need `[Response(...)]`/`[Notify(...)]`).
2. Add a `[PacketHandler(PacketHeaderType.X)]`-tagged method with signature `(PlayerSession, TRequest)` to one of the three controllers here (or a new one).
3. If it's a new controller class, register it in `Program.cs` as shown above.
4. Reply via the inherited `SendAsync`/`NotifyAsync` (from `PlayerBaseController`) or `PlayerSession.SendAsync`/`Notify` if you already hold the target session directly (as `CommunityController` does for whispers).
