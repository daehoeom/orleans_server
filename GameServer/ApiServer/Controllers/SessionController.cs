using Database.Db;
using Database.Db.Row;
using Database.Redis;
using Database.Redis.RedisSet;
using Microsoft.AspNetCore.Mvc;
using SharedLibrary;
using SharedLibrary.Packet.Http;

namespace ApiServer.Controllers;

[ApiController]
[Route("api/v2/{controller}")]
public class SessionController(ILogger<SessionController> logger, DatabaseService databaseService, RedisService redisService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<IActionResult> LoginAsync([FromBody] Login req)
    {
        if (string.IsNullOrEmpty(req.GuidKey))
        {
            return Ok(new LoginRes { ResultCode = ResultCode.InvalidParameter });
        }

        var account = await databaseService.Account.Account.GetAsync(req.GuidKey);
        if (account is null)
        {
            var insertedRow = await databaseService.Account.Account.InsertAsync(new AccountRow
            {
                guid = req.GuidKey,
            });
            if (insertedRow <= 0)
            {
                return Ok(new LoginRes { ResultCode = ResultCode.DbInsertError });
            }

            account = await databaseService.Account.Account.GetAsync(req.GuidKey);
            if (account is null)
            {
                return Ok(new LoginRes { ResultCode = ResultCode.InternalServeError });
            }
        }

        var playerId = account.account_id;

        var player = await databaseService.Game.Player.GetAsync(playerId);
        if (player is null)
        {
            var insertedRow = await databaseService.Game.Player.InsertAsync(new PlayerRow
            {
                player_id = playerId,
                player_name = $"Player{playerId}",
                player_thumbnail = 0,
            });
            if (insertedRow <= 0)
            {
                return Ok(new LoginRes { ResultCode = ResultCode.DbInsertError });
            }
        }

        var accessToken = Guid.NewGuid().ToString("N");
        var tokenSet = new PlayerAccessTokenSet(redisService.GetDatabase(0));
        await tokenSet.StringSet(accessToken, playerId);

        logger.LogInformation($"[Session] 로그인: GuidKey={req.GuidKey}, PlayerId={playerId}");

        return Ok(new LoginRes
        {
            ResultCode = ResultCode.Success,
            AccountId = playerId,
            AccessToken = accessToken,
        });
    }
}
