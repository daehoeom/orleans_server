---
name: database-new-table
description: Use when given a MySQL CREATE TABLE schema and asked to generate the corresponding Row.cs/DbSet.cs for this GameServer repo, when adding a new query/mutation method to an existing DbSet, or when creating a new Row+DbSet pair for a new table under Database/Db. Covers the Dapper raw-SQL and row-mapping conventions this project follows and known bugs to avoid repeating.
---

# Database/Db: Row + DbSet conventions

This project has no ORM/migrations layer. Given a table's schema, two files are always produced together: a `Row` (`Database/Db/Row/XRow.cs`) that mirrors the columns 1:1, and a `DbSet` (`Database/Db/DbSet/XDbSet.cs`) that hand-writes SQL against it via Dapper. Four pairs exist today: `Player`/`Account`/`PlayerWallet`/`PlayerPurchaseLimit`. Follow their shape exactly — don't introduce a query builder, LINQ provider, code-gen step, or new abstraction.

## Row.cs conventions

```csharp
using Database.Db.Attribute;

namespace Database.Db.Row;

[Table("table_name")]
public record XRow
{
    public long x_id { get; set; }          // see PK convention below
    public string some_column { get; set; }
    public int another_column { get; set; }
    public DateTime updated_at { get; set; }
    public DateTime created_at { get; set; }
}
```

- **`[Table("table_name")]` is mandatory** (`Database/Db/Attribute/TableAttribute.cs`) — `DbConnector.InsertAsync`/`BulkInsertAsync` read it reflectively to build the INSERT statement. A row type without it silently produces broken SQL (see the `PlayerPurchaseLimitRow` bug below). Table name = the literal SQL table name, snake_case.
- **Property names = column names, verbatim, snake_case** (`player_id`, `monetary_type`, `purchase_count`, ...). There is no separate column-mapping attribute in this codebase — the property name *is* the bind name Dapper uses, so it must match the schema exactly.
- **`record` is the default choice**; 3 of the 4 existing rows are records, only `PlayerWalletRow` is a plain `class` (an inconsistency in the existing code, not a deliberate pattern to follow) — use `record` for new row types unless you have a specific reason not to. Records still support the `row.updated_at = ...` mutation used by `InsertAsync` callers because these use `{ get; set; }`, not `{ get; init; }`.
- **Every row type so far has both `updated_at` and `created_at DateTime` columns**, stamped by the DbSet's `InsertAsync` (not defaulted at the DB layer) — include both on every new table and don't rely on the schema's own `DEFAULT`/`ON UPDATE` clauses to populate them, since the app always overwrites them on insert.
- **Primary key naming convention** — two shapes appear in existing tables, pick based on cardinality:
  - *One row per entity* (`accounts`, `players`): the entity's own id is the PK column directly (`account_id`, `player_id`), no separate surrogate `id` column.
  - *Multiple rows per entity* (`player_wallets` keyed by player+currency, `player_purchase_limit` keyed by player+product): a surrogate `id BIGINT` PK, plus a foreign business-key column (`player_id`) and a discriminator column (`monetary_type`, `product_id`) that together form the natural/lookup key used by `GetAsync`.
- **SQL type → C# type mapping** (inferred from existing rows; no nullable/boolean/decimal columns exist yet in this codebase, so treat these as sane C# defaults rather than an established convention):
  - `BIGINT` → `long`
  - `INT` → `int`
  - `VARCHAR`/`CHAR`/`TEXT` → `string`
  - `DATETIME`/`TIMESTAMP` → `DateTime`
  - `TINYINT(1)`/`BOOLEAN` → `bool`
  - `DECIMAL` → `decimal`
  - Nullable SQL column → nullable C# type (`long?`, `string?`, `DateTime?`, ...)

## Shape of a DbSet class

```csharp
public class XDbSet(DbConnector conn)
{
    public Task<XRow?> GetAsync(<key columns>)
    {
        var sql = @"SELECT * FROM `table_name` WHERE `col` = @col";
        var param = new { col };
        return conn.QueryFirstOrDefaultAsync<XRow>(sql, param);
    }

    public Task<int> InsertAsync(XRow row)
    {
        row.updated_at = DateTime.UtcNow;
        row.created_at = DateTime.UtcNow;
        return conn.InsertAsync(row);   // reflection-based, needs [Table("table_name")] on XRow
    }
}
```

Rules to follow:

- **Primary-constructor style**: `public class XDbSet(DbConnector conn)` — no field declarations, no other dependencies. If a method needs a second table's data, add a second `DbSet` to the caller instead of reaching across.
- **Raw SQL, backtick-quoted identifiers**: table and column names are wrapped in backticks (MySQL style), written as verbatim string literals (`@"..."`). No string concatenation of user input into SQL — always bind through Dapper's anonymous-object params.
- **Param object property names must match the SQL `@param` names exactly**, and those in turn should match real column names (snake_case: `player_id`, `monetary_type`, `product_id`, ...). Dapper binds by name, so a mismatch fails silently at the SQL level (unknown parameter) or wrong-column level.
- **`InsertAsync(row)` always stamps `updated_at`/`created_at` to `DateTime.UtcNow` on the row before calling `conn.InsertAsync(row)`.** `conn.InsertAsync` builds the column list reflectively from the row's public properties plus the row's `[Table(...)]` attribute (`Database/Db/Attribute/TableAttribute.cs`) — if the row type is missing `[Table(...)]`, the generated SQL becomes invalid (empty table name). Always confirm the row type has the attribute before wiring a new `InsertAsync` call.
- **Delta mutations are atomic SQL arithmetic, not read-modify-write.** See `PlayerWalletDbSet.AddAsync`/`SpendAsync`: `SET amount = amount + @addValue WHERE ...` executed directly against the DB, never "read row into C#, add, write back" — that pattern avoids lost updates under concurrent access and must be preserved for any new balance/counter-style field.
- **Enum convenience overloads forward to the primitive overload**, they don't duplicate SQL. See `PlayerWalletDbSet.GetAsync(long, CurrencyType)` → `GetAsync(long, (int)monetary_type)`. Follow this if a new method needs both an `int` and an enum entry point.
- **Multi-row reads use `GetsAsync` (plural) returning `Task<IEnumerable<XRow>>`** via `conn.QueryAsync<XRow>`; single-row reads use `GetAsync` returning `Task<XRow?>` via `conn.QueryFirstOrDefaultAsync<XRow>`.

## Adding a method to an existing DbSet

1. Confirm the exact column names by reading the corresponding `Row` type in `Database/Db/Row/` — don't assume a PK is called `id`.
2. Write the SQL as a verbatim string with backtick-quoted identifiers, matching the style of neighboring methods in the same file.
3. Build the param object with property names identical to the `@param` placeholders.
4. Route through `conn.QueryAsync`/`QueryFirstOrDefaultAsync`/`ExecuteAsync`/`InsertAsync` on `DbConnector` — never open a raw `IDbConnection` yourself; everything must go through the connector's serialized work queue (see `Database/CLAUDE.md` for why).
5. If the mutation needs multi-statement atomicity, use `conn.WithTransactionAsync` instead of a bare `ExecuteAsync` (see `Database/CLAUDE.md`, "Transactions").

## Adding a brand-new table: CREATE TABLE → Row.cs → DbSet.cs → Context

Given a `CREATE TABLE` statement, produce the three pieces in this order:

1. **Row** (`Database/Db/Row/XRow.cs`): one property per column using the type mapping above, `[Table("<table name>")]`, `record`.
2. **DbSet** (`Database/Db/DbSet/XDbSet.cs`): at minimum a `GetAsync` keyed on the natural/lookup key and an `InsertAsync` that stamps `updated_at`/`created_at`; add `GetsAsync`, `UpdateXAsync`, or delta methods (`AddAsync`/`SpendAsync`-style) only for what the caller actually needs — don't speculatively generate a full CRUD surface.
3. **Register on a context**: add the new `DbSet` as a property on `AccountDbContext` (account DB) or `GameDbContext` (game DB) in `Database/Db/Context/`, constructed with that context's shared `conn`. Pick the context matching which physical database the table lives in.

Example — given:

```sql
CREATE TABLE `player_items` (
  `id` BIGINT NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `player_id` BIGINT NOT NULL,
  `item_id` INT NOT NULL,
  `count` INT NOT NULL,
  `updated_at` DATETIME NOT NULL,
  `created_at` DATETIME NOT NULL
);
```

produce:

```csharp
// Database/Db/Row/PlayerItemRow.cs
using Database.Db.Attribute;

namespace Database.Db.Row;

[Table("player_items")]
public record PlayerItemRow
{
    public long id { get; set; }
    public long player_id { get; set; }
    public int item_id { get; set; }
    public int count { get; set; }
    public DateTime updated_at { get; set; }
    public DateTime created_at { get; set; }
}
```

```csharp
// Database/Db/DbSet/PlayerItemDbSet.cs
using Database.Db.Row;

namespace Database.Db.DbSet;

public class PlayerItemDbSet(DbConnector conn)
{
    public Task<PlayerItemRow?> GetAsync(long player_id, int item_id)
    {
        var sql = @"SELECT * FROM `player_items`
                    WHERE `player_id` = @player_id AND `item_id` = @item_id";
        var param = new { player_id, item_id };
        return conn.QueryFirstOrDefaultAsync<PlayerItemRow>(sql, param);
    }

    public Task<int> InsertAsync(PlayerItemRow row)
    {
        row.updated_at = DateTime.UtcNow;
        row.created_at = DateTime.UtcNow;
        return conn.InsertAsync(row);
    }
}
```

```csharp
// Database/Db/Context/GameDbContext.cs — add one line
public PlayerItemDbSet Items { get; private set; } = new(conn);
```

(`player_items` is per-player game data, so it goes on `GameDbContext`, not `AccountDbContext`.)

## Known bugs in existing files — don't copy these mistakes

- `PlayerDbSet.UpdateNickNameAsync` has `WHERE id = @player_id`, but `PlayerRow` has no `id` column — the actual PK column is `player_id` (as used correctly in `GetAsync`/`DeleteAsync` in the same file). This query would fail against the real schema (unknown column `id`). If you touch this method, fix the WHERE clause to `` `player_id` = @player_id ``.
- `PlayerPurchaseLimitDbSet.AddAsync(player_id, addValue)` updates `purchase_count` filtered only by `player_id`, with no `product_id` in the `WHERE` clause — even though `GetAsync`/`GetsAsync` on the same class treat `(player_id, product_id)` as the row's identity. As written, calling `AddAsync` bumps every purchase-limit row for that player, not just the target product. Add a `product_id` parameter and filter before relying on this method.
- `PlayerPurchaseLimitRow` is missing `[Table("player_purchase_limit")]` (every other row type has a matching `[Table(...)]`). `PlayerPurchaseLimitDbSet.InsertAsync` currently generates invalid SQL because of this — add the attribute before using that insert path.
