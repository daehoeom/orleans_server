using Database.Db.Row;

namespace Database.Db.DbSet;

public class PlayerEventAttendanceRewardDbSet(DbConnector conn)
{
    public Task<PlayerEventAttendanceRewardRow?> GetAsync(long player_id, int reward_id)
    {
        var query = @"SELECT * FROM `player_event_attendance_rewards`
                WHERE `player_id` = @player_id AND `reward_id` = @reward_id";

        var param = new
        {
            player_id,
            reward_id,
        };

        return conn.QueryFirstOrDefaultAsync<PlayerEventAttendanceRewardRow>(query, param);
    }

    public Task<IEnumerable<PlayerEventAttendanceRewardRow>> GetsAsync(long player_id)
    {
        var query = @"SELECT * FROM `player_event_attendance_rewards`
                WHERE `player_id` = @player_id";

        var param = new
        {
            player_id,
        };

        return conn.QueryAsync<PlayerEventAttendanceRewardRow>(query, param);
    }

    public Task<int> InsertAsync(PlayerEventAttendanceRewardRow row)
    {
        row.updated_at = DateTime.UtcNow;
        row.created_at = DateTime.UtcNow;

        return conn.InsertAsync(row);
    }
}
