using PollingSurvey.Application.Interfaces;
using QRCoder;

namespace PollingSurvey.Application.Services;

public class QRCodeService : IQRCodeService
{
    // ✅ Localhost only for now, as requested
    private const string FrontendBaseUrl = "http://localhost:5173/poll";

    public byte[] GeneratePollQRCode(string code)
    {
        var pollUrl = $"{FrontendBaseUrl}/{code}";

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(pollUrl, QRCodeGenerator.ECCLevel.Q);
        using var pngQrCode = new PngByteQRCode(qrCodeData);

        return pngQrCode.GetGraphic(20);
    }
}
