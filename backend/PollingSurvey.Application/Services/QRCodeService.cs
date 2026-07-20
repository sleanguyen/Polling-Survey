using Microsoft.Extensions.Options;
using PollingSurvey.Application.Interfaces;
using QRCoder;

namespace PollingSurvey.Application.Services;

public class QRCodeService : IQRCodeService
{
    private readonly string _frontendBaseUrl;

    public QRCodeService(IOptions<QrCodeSettings> qrCodeSettings)
    {
        _frontendBaseUrl = qrCodeSettings.Value.BaseUrl.TrimEnd('/');
    }

    public byte[] GeneratePollQRCode(string code)
    {
        var pollUrl = $"{_frontendBaseUrl}/poll/{code}";

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(pollUrl, QRCodeGenerator.ECCLevel.Q);
        using var pngQrCode = new PngByteQRCode(qrCodeData);

        return pngQrCode.GetGraphic(20);
    }
}