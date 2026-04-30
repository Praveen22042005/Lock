using System.Security.Cryptography;
using System.Text;

namespace Lock.API.Services;

public class SigningService
{
    private readonly RSA rsa;

    public SigningService(string keySource)
    {
        string keyContent;

        if (keySource.StartsWith("-----BEGIN"))
        {
            keyContent = keySource;
        }
        else
        {
            if (!File.Exists(keySource))
            {
                throw new FileNotFoundException($"The RSA private key file was not found at: {keySource}");
            }
            keyContent = File.ReadAllText(keySource);
        }

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
