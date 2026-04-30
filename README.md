# Lock

## Download

Download the latest Windows executable: [Download Lock.Client.exe](https://github.com/Praveen22042005/Lock/raw/main/Lock.Client/publish/win-x64/Lock.Client.exe)

*Windows 10 or 11 x64 required. No installation needed. Run the exe directly. If Windows SmartScreen appears, click "More info" then "Run anyway".*

## System Architecture

- **Lock.Client**: WinForms desktop application that handles hardware fingerprinting, activation requests, and local token management.
- **Lock.API**: ASP.NET Core Minimal API backend that validates license keys, manages activation limits, and signs activation tokens.
- **Lock.Shared**: Shared library containing DTOs used by both the Client and API for communication.

## Tech Stack

- WinForms .NET 10
- ASP.NET Core Minimal API
- Neon PostgreSQL
- Npgsql (Raw SQL)
- RSA-2048 token signing
- Windows DPAPI
- SHA-256 hardware fingerprinting

## Getting Started

### Prerequisites

- Visual Studio 2022
- VS Code
- .NET 10 SDK
- Neon account

### Setup Steps

1. Clone the repository.
2. Run the `schema.sql` in your Neon PostgreSQL database.
3. Configure the connection string in `Lock.API/appsettings.Development.json`.
4. Generate RSA keys using the `Lock.KeyGen` script or your own tool (ensure private key is in `Lock.API/Keys/` and public key is in `Lock.Client/Resources/`).
5. Run `Lock.API` using `dotnet run`.
6. Generate a license key using the `Lock.KeyGen` script in the `scripts/Lock.KeyGen` folder.
7. Run `Lock.Client` from Visual Studio.

## License Key Format

The system uses a `XXXXX-XXXXX-XXXXX-XXXXX-XXXXX` format. Each group consists of random uppercase letters or digits, with the final group being a checksum of the first four groups (sum of ASCII values modulo 100,000).

## Security Design

- **HWID Fingerprinting**: Creates a composite fingerprint from motherboard serial, system UUID, and Windows MachineGuid.
- **DPAPI Storage**: Locally stores tokens encrypted with Windows DPAPI, ensuring they can only be read by the current user on the same machine.
- **RSA Verification**: The client verifies the token's RSA signature using an embedded public key to ensure it was issued by the legitimate API.
