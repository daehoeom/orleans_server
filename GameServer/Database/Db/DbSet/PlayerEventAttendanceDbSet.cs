using Database.Db.Row;

namespace Database.Db.DbSet;

public class PlayerEventAttendanceDbSet(DbConnector conn)
{
    public Task<PlayerEventAttendanceRow?> GetAsync(long player_id, int event_id)
    {
        var query = @"SELECT * FROM `player_event_attendance`
                WHERE `player_id` = @player_id AND `event_id` = @event_id";

        var param = new
        {
            player_id,
            event_id,
        };

        return conn.QueryFirstOrDefaultAsync<PlayerEventAttendanceRow>(query, param);
    }

    public Task<IEnumerable<PlayerEventAttendanceRow>> GetsAsync(long player_id)
    {
        var query = @"SELECT * FROM `player_event_attendance`
                WHERE `player_id` = @player_id";

        var param = new
        {
            player_id,
        };

        return conn.QueryAsync<PlayerEventAttendanceRow>(query, param);
    }

    public Task<int> InsertAsync(PlayerEventAttendanceRow row)
    {
        row.last_updated_at = DateTime.UtcNow;
        row.created_at = DateTime.UtcNow;

        return conn.InsertAsync(row);
    }

    public Task<int> UpdateAsync(long player_id, int event_id, int day, DateTime last_updated_at)
    {
        var query = @"UPDATE `player_event_attendance` SET
                `day` = @day,
                `last_updated_at` = @last_updated_at
                WHERE `player_id` = @player_id AND `event_id` = @event_id";

        var param = new
        {
            player_id,
            event_id,
            day,
            last_updated_at,
        };

        return conn.ExecuteAsync(query, param);
    }
}
