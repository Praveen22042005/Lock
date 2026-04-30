using System.Text.Json;
using Lock.Client.Forms;
using Lock.Client.Models;
using Lock.Client.Services;

namespace Lock.Client;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        var tokenService = new TokenService();
        Form startupForm;

        try
        {
            var storedToken = tokenService.LoadToken();

            if (storedToken == null)
            {
                startupForm = new RegistrationForm();
            }
            else
            {
                if (tokenService.VerifySignature(storedToken.Value.token, storedToken.Value.signature))
                {
                    var tokenModel = JsonSerializer.Deserialize<ActivationToken>(storedToken.Value.token, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (tokenModel != null)
                    {
                        startupForm = new MainForm(tokenModel);
                    }
                    else
                    {
                        tokenService.DeleteToken();
                        startupForm = new RegistrationForm();
                    }
                }
                else
                {
                    tokenService.DeleteToken();
                    startupForm = new RegistrationForm();
                }
            }
        }
        catch
        {
            tokenService.DeleteToken();
            startupForm = new RegistrationForm();
        }

        Application.Run(startupForm);
    }
}
