using Microsoft.Extensions.Configuration;
using PollingSurvey.Application.Interfaces;
using QRCoder;

namespace PollingSurvey.Application.Services;

public class QRCodeService : IQRCodeService
{
    private readonly string _frontendBaseUrl;

    public QRCodeService(IConfiguration configuration)
    {
        _frontendBaseUrl = configuration["FrontendBaseUrl"]
            ?? "http://localhost:5173/poll";
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