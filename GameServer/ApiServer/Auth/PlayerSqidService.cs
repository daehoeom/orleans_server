using Sqids;

namespace ApiServer.Auth;

public class PlayerSqidService
{
    private readonly SqidsEncoder<long> _encoder = new(new SqidsOptions
    {
        MinLength = 8, 
        Alphabet = "k3G7QAe51FCsPW92uEOyq4Bg6Sp8YzVTmnU0liwDdHXLajZrfxNhobJIRcMvKt",
    });

    public string Encode(long playerId) => _encoder.Encode(playerId);

    public long Decode(string sqid)
    {
        var numbers = _encoder.Decode(sqid);
        return numbers.Count > 0 ? numbers[0] : 0;
    }
}
