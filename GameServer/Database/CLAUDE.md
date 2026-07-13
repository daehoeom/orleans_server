# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Scope

This is the `Database` project — the data-access layer shared by `FrontServer`, `ApiServer`, and `GrainLibrary`. It has no dependencies other than `SharedLibrary`. There is no EF Core anywhere in this project: everything is raw SQL via Dapper against MySQL, plus a thin StackExchange.Redis wrapper.

## Connection & concurrency model

`DbConnector` (`Db/DbConnector.cs`) is the single point of access to one MySQL database. Two independent instances exist, both created by `DatabaseService` (`Db/DatabaseService.cs`):

- `AccountDbContext.Conn` → the `account` database
- `GameDbContext.Conn` → the `game` database

Each `DbConnector` owns an unbounded, single-reader/single-writer `Channel<IDbPipelineWork>`. Every query/execute call (`QueryAsync`, `ExecuteAsync`, `InsertAsync`, `WithTransactionAsync`, etc.) is wrapped as a `DbPipelineContext<T>` work item, pushed onto the channel, and awaited via a `TaskCompletionSource`. A single background loop (`ProcessQueueAsync`) drains the channel and runs work items **one at a time**, opening a fresh `MySqlConnection` per item. This means all DB traffic against one database is effectively serialized through one worker, regardless of the `Max Pool Size` value baked into the connection string — that setting is currently unused headroom, not real parallelism. Keep this in mind before assuming query load parallelizes; if you need concurrent throughput, you'd need multiple `DbConnector`s or a redesign of the queue, not just pool tuning.

Connection settings are hardcoded defaults in `Db/DbSetting.cs` (host/port/db name/user/password/pool size for both `Account*` and `Game*`) and `Redis/RedisSetting.cs` — not read from `appsettings.json`. Edit these records directly to point at a different environment; there is currently no config-binding or environment-variable override wired in.

## Adding a new table

Follow the existing three-piece pattern (see `PlayerWalletDbSet`/`PlayerWalletRow` as the most complete example):

1. **Row** (`Db/Row/XRow.cs`) — a POCO/record whose public property names match the MySQL column names exactly (snake_case, e.g. `player_id`, `updated_at`), tagged with `[Table("table_name")]` from `Db/Attribute/TableAttribute.cs`.
2. **DbSet** (`Db/DbSet/XDbSet.cs`) — takes a `DbConnector` in its constructor, hand-writes SQL per method (`GetAsync`, `InsertAsync`, `UpdateXAsync`, etc.) using Dapper parameter objects. `InsertAsync`/`BulkInsertAsync` on `DbConnector` build the INSERT list from the row type's public properties reflectively, so property order/names must line up with the table schema — no attribute-level column renaming is supported.
3. **Wire into a context** — add the new `XDbSet` as a property on `AccountDbContext` or `GameDbContext` (`Db/Context/`), constructed with that context's shared `conn`.

Known gotcha: `PlayerPurchaseLimitRow` is missing the `[Table(...)]` attribute that every other row type has. `DbConnector.GetTableName<T>()` falls back to an empty string when the attribute is absent, so `PlayerPurchaseLimitDbSet.InsertAsync` would currently generate an invalid `INSERT INTO  (...)` statement. Add the attribute before relying on that insert path.

## Transactions

`DbConnector.WithTransactionAsync<T>(Func<DbTransactionScope, Task<T>>)` is the preferred transaction entry point. `DbTransactionScope` (`Db/DbTransactionScope.cs`) wraps an `IDbTransaction` and requires an explicit `Complete()` call inside the callback — if you don't call it, the transaction rolls back automatically on `DisposeAsync`. It also supports `OnCommitted`/`OnRolledBack` callback hooks for side effects that must only fire after the outcome is known (e.g. cache updates, retry-queue registration). `ExecuteTransactionAsync` is an older/lower-level alternative that commits/rolls back a raw `IDbTransaction` directly — prefer `WithTransactionAsync` for new code.

## Redis layer

`RedisService` (`Redis/RedisService.cs`) wraps a single `ConnectionMultiplexer` and exposes `TryAcquireLockAsync`/`ReleaseLockAsync` (compare-and-delete via a Lua script, for distributed locking) plus raw `GetDatabase(int)` access. Typed helpers live under `Redis/RedisSet/` (e.g. `PlayerAccessTokenSet`), following a key-prefix-plus-get/set-wrapper pattern distinct from the SQL `DbSet` naming — don't confuse the two; Redis "sets" here are just namespaced key helpers, not Redis SET data structures.
