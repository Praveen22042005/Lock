using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Lock.Client.Services;

public class TokenService
{
    private readonly string tokenFilePath;

    public TokenService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var lockFolder = Path.Combine(appData, "Lock");
        tokenFilePath = Path.Combine(lockFolder, "activation.dat");
    }

    public void SaveToken(string token, string signature)
    {
        var directory = Path.GetDirectoryName(tokenFilePath);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var data = new { token, signature };
        var json = JsonSerializer.Serialize(data);
        var encryptedBytes = ProtectedData.Protect(Encoding.UTF8.GetBytes(json), null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(tokenFilePath, encryptedBytes);
    }

    public (string token, string signature)? LoadToken()
    {
        if (!File.Exists(tokenFilePath)) return null;

        try
        {
            var encryptedBytes = File.ReadAllBytes(tokenFilePath);
            var decryptedBytes = ProtectedData.Unprotect(encryptedBytes, null, DataProtectionScope.CurrentUser);
            var json = Encoding.UTF8.GetString(decryptedBytes);
            var data = JsonSerializer.Deserialize<TokenData>(json);
            
            if (data == null || string.IsNullOrEmpty(data.token) || string.IsNullOrEmpty(data.signature)) return null;
            
            return (data.token, data.signature);
        }
        catch
        {
            return null;
        }
    }

    public bool VerifySignature(string token, string signature)
    {
        try
        {
            var publicKey = GetPublicKey();
            using var rsa = RSA.Create();
            rsa.ImportFromPem(publicKey);
            
            var tokenBytes = Encoding.UTF8.GetBytes(token);
            var signatureBytes = Convert.FromBase64String(signature);
            
            return rsa.VerifyData(tokenBytes, signatureBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        }
        catch
        {
            return false;
        }
    }

    public void DeleteToken()
    {
        if (File.Exists(tokenFilePath))
        {
            File.Delete(tokenFilePath);
        }
    }

    private string GetPublicKey()
    {
        var assembly = Assembly.GetExecutingAssembly();
        var resourceName = "Lock.Client.Resources.publickey.pem";
        
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream == null) throw new FileNotFoundException("Public key resource not found.");
        
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private class TokenData
    {
        public string token { get; set; } = string.Empty;
        public string signature { get; set; } = string.Empty;
    }
}
