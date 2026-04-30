using Lock.Client.Models;
using Lock.Client.Services;

namespace Lock.Client.Forms;

public partial class RegistrationForm : Form
{
    private readonly HardwareIdService hardwareIdService;
    private readonly TokenService tokenService;
    private readonly ActivationService activationService;

    private readonly TextBox fullNameInput;
    private readonly TextBox emailInput;
    private readonly TextBox licenseKeyInput;
    private readonly Button activateButton;
    private readonly Label statusLabel;
    private string previousLicenseKey = string.Empty;

    public RegistrationForm()
    {
        hardwareIdService = new HardwareIdService();
        tokenService = new TokenService();
        activationService = new ActivationService(tokenService, hardwareIdService);

        Text = "Lock Activation";
        Size = new Size(400, 350);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        var titleLabel = new Label
        {
            Text = "Lock Activation",
            Font = new Font(Font.FontFamily, 16, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 20)
        };

        var fullNameLabel = new Label { Text = "Full Name", Location = new Point(20, 60), AutoSize = true };
        fullNameInput = new TextBox { Location = new Point(20, 80), Width = 340 };

        var emailLabel = new Label { Text = "Email", Location = new Point(20, 110), AutoSize = true };
        emailInput = new TextBox { Location = new Point(20, 130), Width = 340 };

        var licenseKeyLabel = new Label { Text = "License Key", Location = new Point(20, 160), AutoSize = true };
        licenseKeyInput = new TextBox { Location = new Point(20, 180), Width = 340, MaxLength = 29 };
        licenseKeyInput.TextChanged += OnLicenseKeyTextChanged;

        activateButton = new Button
        {
            Text = "Activate",
            Location = new Point(20, 220),
            Width = 340,
            Height = 40
        };
        activateButton.Click += OnActivateClicked;

        statusLabel = new Label
        {
            Text = "",
            ForeColor = Color.Red,
            Location = new Point(20, 270),
            Width = 340,
            Height = 40,
            TextAlign = ContentAlignment.TopCenter
        };

        Controls.AddRange(new Control[] { 
            titleLabel, fullNameLabel, fullNameInput, 
            emailLabel, emailInput, 
            licenseKeyLabel, licenseKeyInput, 
            activateButton, statusLabel 
        });
    }

    private void OnLicenseKeyTextChanged(object? sender, EventArgs e)
    {
        var input = (TextBox)sender!;
        var currentText = input.Text;

        if (currentText.Length > previousLicenseKey.Length)
        {
            var len = currentText.Length;
            if (len == 5 || len == 11 || len == 17 || len == 23)
            {
                input.Text = currentText + "-";
                input.SelectionStart = input.Text.Length;
            }
        }

        previousLicenseKey = input.Text;
    }

    private async void OnActivateClicked(object? sender, EventArgs e)
    {
        activateButton.Enabled = false;
        statusLabel.ForeColor = Color.Black;
        statusLabel.Text = "Activating...";

        try
        {
            var token = await activationService.ActivateAsync(
                fullNameInput.Text, 
                emailInput.Text, 
                licenseKeyInput.Text);

            var mainForm = new MainForm(token);
            mainForm.Show();
            
            this.Hide();
            mainForm.FormClosed += (s, args) => this.Close();
        }
        catch (Exception ex)
        {
            statusLabel.ForeColor = Color.Red;
            statusLabel.Text = ex.Message;
            activateButton.Enabled = true;
        }
    }
}
