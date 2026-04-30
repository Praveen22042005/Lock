using System.Management;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

namespace Lock.Client.Services;

public class HardwareIdService
{
    public string GetHardwareId()
    {
        var motherboardSerial = GetMotherboardSerial();
        var systemUuid = GetSystemUuid();
        var machineGuid = GetMachineGuid();

        if (string.IsNullOrEmpty(motherboardSerial) || 
            motherboardSerial.Equals("Default String", StringComparison.OrdinalIgnoreCase) || 
            motherboardSerial.Equals("To Be Filled By O.E.M.", StringComparison.OrdinalIgnoreCase))
        {
            motherboardSerial = string.Empty;
        }

        if (string.IsNullOrEmpty(systemUuid) || 
            systemUuid.Equals("00000000-0000-0000-0000-000000000000", StringComparison.OrdinalIgnoreCase) || 
            systemUuid.Equals("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF", StringComparison.OrdinalIgnoreCase))
        {
            systemUuid = string.Empty;
        }

        var combined = motherboardSerial + systemUuid + machineGuid;
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(combined));
        return Convert.ToHexString(hashBytes).ToLower();
    }

    private string GetMotherboardSerial()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT SerialNumber FROM Win32_BaseBoard");
            foreach (var item in searcher.Get())
            {
                return item["SerialNumber"]?.ToString() ?? string.Empty;
            }
        }
        catch { }
        return string.Empty;
    }

    private string GetSystemUuid()
    {
        try
        {
            using var searcher = new ManagementObjectSearcher("SELECT UUID FROM Win32_ComputerSystemProduct");
            foreach (var item in searcher.Get())
            {
                return item["UUID"]?.ToString() ?? string.Empty;
            }
        }
        catch { }
        return string.Empty;
    }

    private string GetMachineGuid()
    {
        try
        {
            using var key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Cryptography");
            return key?.GetValue("MachineGuid")?.ToString() ?? string.Empty;
        }
        catch { }
        return string.Empty;
    }
}
