using Lock.Client.Models;
using Lock.Client.Services;

namespace Lock.Client.Forms;

public partial class MainForm : Form
{
    private readonly ActivationToken token;
    private HardwareIdService hardwareIdService = null!;
    private TokenService tokenService = null!;
    private ActivationService activationService = null!;

    private Label statusLabel = null!;
    private Button deactivateButton = null!;

    public MainForm(ActivationToken token)
    {
        this.token = token;

        Text = "Lock - Activated";
        Size = new Size(400, 300);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        StartPosition = FormStartPosition.CenterScreen;

        InitializeControls();

        Load += OnFormLoad;
    }

    private void InitializeControls()
    {
        var titleLabel = new Label
        {
            Text = "Lock — Activated",
            Font = new Font(Font.FontFamily, 14, FontStyle.Bold),
            AutoSize = true,
            Location = new Point(20, 20)
        };

        var hwidDisplay = token.Hwid.Length > 16 ? token.Hwid[..16] : token.Hwid;
        var hwidLabel = new Label { Text = $"Machine ID: {hwidDisplay}", Location = new Point(20, 60), AutoSize = true };
        var licenseLabel = new Label { Text = $"License ID: {token.LicenseKeyId}", Location = new Point(20, 90), AutoSize = true };
        var expiresLabel = new Label { Text = $"Expires: {token.ExpiresAt:yyyy-MM-dd}", Location = new Point(20, 120), AutoSize = true };

        statusLabel = new Label
        {
            Text = "Checking status...",
            Location = new Point(20, 160),
            Width = 340,
            TextAlign = ContentAlignment.TopCenter
        };

        deactivateButton = new Button
        {
            Text = "Deactivate This Machine",
            Location = new Point(20, 200),
            Width = 340,
            Height = 40
        };
        deactivateButton.Click += OnDeactivateClicked;

        Controls.AddRange(new Control[] { titleLabel, hwidLabel, licenseLabel, expiresLabel, statusLabel, deactivateButton });
    }

    private void OnFormLoad(object? sender, EventArgs e)
    {
        hardwareIdService = new HardwareIdService();
        tokenService = new TokenService();
        activationService = new ActivationService(tokenService, hardwareIdService);

        var currentHwid = hardwareIdService.GetHardwareId();

        if (currentHwid != token.Hwid)
        {
            statusLabel.Text = "HWID mismatch — please reactivate";
            statusLabel.ForeColor = Color.Red;
            foreach (Control control in Controls)
            {
                if (control != statusLabel) control.Enabled = false;
            }
            return;
        }

        statusLabel.Text = "Active";
        statusLabel.ForeColor = Color.Green;

        if (token.ExpiresAt < DateTime.UtcNow.AddDays(30))
        {
            Task.Run(async () =>
            {
                var success = await activationService.RevalidateAsync(token.LicenseKeyId);
                if (!success)
                {
                    Invoke(() =>
                    {
                        statusLabel.Text = "Revalidation failed — please check your connection";
                        statusLabel.ForeColor = Color.Red;
                    });
                }
            });
        }
    }

    private async void OnDeactivateClicked(object? sender, EventArgs e)
    {
        var result = MessageBox.Show(
            "Are you sure you want to deactivate this machine?",
            "Confirm Deactivation",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);

        if (result == DialogResult.Yes)
        {
            deactivateButton.Enabled = false;
            statusLabel.Text = "Deactivating...";

            await activationService.DeactivateAsync(token.LicenseKeyId);
            tokenService.DeleteToken();

            var registrationForm = new RegistrationForm();
            registrationForm.Show();

            this.Hide();
            registrationForm.FormClosed += (s, args) => this.Close();
        }
    }
}
