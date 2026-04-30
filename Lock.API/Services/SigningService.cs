using System.Security.Cryptography;
using System.Text;

namespace Lock.API.Services;

public class SigningService
{
    private readonly RSA rsa;

    public SigningService(string keyPath)
    {
        if (!File.Exists(keyPath))
        {
            throw new FileNotFoundException($"The RSA private key file was not found at: {keyPath}");
        }

        var keyContent = File.ReadAllText(keyPath);
        rsa = RSA.Create();
        rsa.ImportFromPem(keyContent);
    }

    public string Sign(string payload)
    {
        var data = Encoding.UTF8.GetBytes(payload);
        var signature = rsa.SignData(data, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        return Convert.ToBase64String(signature);
    }
}
