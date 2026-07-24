using ScenarioBot.Session;
using SharedLibrary;
using SharedLibrary.Packet.Tcp.Mail;

namespace ScenarioBot.Scenario;

public class MailScenario : IScenario
{
    public string Name => "Mail";

    public async Task RunAsync(BotClientSession session)
    {
        await session.SendAsync(PacketHeaderType.LoadMail, new LoadMailReq());

        var loadRes = await session.WaitForResponseAsync<LoadMailRes>();
        if (loadRes.ResultCode != ResultCode.Success)
        {
            throw new Exception($"[{Name}] LoadMail 실패: {loadRes.ResultCode}");
        }

        Console.WriteLine($"[{Name}] 우편 {loadRes.Stream.Mails.Count}건 조회");

        if (loadRes.Stream.Mails.Count == 0)
        {
            Console.WriteLine($"[{Name}] 수령할 우편이 없어 Read/Claim/Delete는 생략합니다.");
            return;
        }

        var mail = loadRes.Stream.Mails[0];

        await session.SendAsync(PacketHeaderType.ReadMail, new ReadMailReq { Id = mail.Id });

        var readRes = await session.WaitForResponseAsync<ReadMailRes>();
        if (readRes.ResultCode != ResultCode.Success)
        {
            throw new Exception($"[{Name}] ReadMail 실패: {readRes.ResultCode}");
        }

        Console.WriteLine($"[{Name}] 읽음 처리: Id={readRes.Stream.MailModel.Id}");

        if (mail.Rewards.Count > 0)
        {
            Console.WriteLine($"[{Name}] 보상 수령 완료 (읽음과 동시 처리): Id={readRes.Stream.MailModel.Id}");
        }

        await session.SendAsync(PacketHeaderType.DeleteMail, new DeleteMailReq { Id = mail.Id });

        var deleteRes = await session.WaitForResponseAsync<DeleteMailRes>();
        if (deleteRes.ResultCode != ResultCode.Success)
        {
            throw new Exception($"[{Name}] DeleteMail 실패: {deleteRes.ResultCode}");
        }

        Console.WriteLine($"[{Name}] 삭제 완료: Id={deleteRes.Stream.Id}");
    }
}
