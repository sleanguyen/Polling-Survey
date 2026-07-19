using PollingSurvey.Application.Interfaces;
using QRCoder;

namespace PollingSurvey.Application.Services;

public class QRCodeService : IQRCodeService
{
    private readonly string _frontendBaseUrl;

    public QRCodeService(string frontendBaseUrl)
    {
        _frontendBaseUrl = frontendBaseUrl;
    }

    public byte[] GeneratePollQRCode(string code)
    {
        var pollUrl = $"{_frontendBaseUrl}/{code}";

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(pollUrl, QRCodeGenerator.ECCLevel.Q);
        using var pngQrCode = new PngByteQRCode(qrCodeData);

        return pngQrCode.GetGraphic(20);
    }
}