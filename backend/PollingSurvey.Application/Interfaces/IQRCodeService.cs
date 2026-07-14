namespace PollingSurvey.Application.Interfaces;

public interface IQRCodeService
{
    byte[] GeneratePollQRCode(string code);
}
