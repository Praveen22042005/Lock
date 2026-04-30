<div align="center">

# 🔒 Lock

### A machine-bound software licensing system for .NET desktop applications.

![.NET 8](https://img.shields.io/badge/.NET-8-blue)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-purple)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-blue)
![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey)
![License](https://img.shields.io/badge/License-MIT-green)

<br />

<a href="https://github.com/Praveen22042005/Lock/raw/main/Lock.Client/publish/win-x64/Lock.Client.exe" style="background-color:#238636; color:white; padding:12px 24px; border-radius:6px; font-size:16px; font-weight:bold; text-decoration:none; display: inline-block;">Download for Windows</a>

<br />
<br />

*Windows 10 / 11 x64 | No installation needed*

</div>

---

## About

Lock is a robust licensing system designed to protect .NET desktop applications by binding software activations to specific physical hardware. It combines hardware fingerprinting, cloud-based validation, and cryptographically signed tokens to ensure secure and authorized usage across your user base.

## Screenshots

*Coming soon...*

## System Architecture

| Name | Type | Responsibility |
| :--- | :--- | :--- |
| **Lock.Client** | WinForms | Handles HWID fingerprinting, activation requests, and local token management. |
| **Lock.API** | ASP.NET Core | Validates license keys, manages activation limits, and signs activation tokens. |
| **Lock.Shared** | Library | Contains shared DTOs used by both Client and API for communication. |

## Tech Stack

| Component | Technology |
| :--- | :--- |
| **UI** | WinForms (.NET 10) |
| **Backend** | ASP.NET Core Minimal API |
| **Database** | Neon PostgreSQL |
| **DB Access** | Npgsql (Raw SQL) |
| **Cryptography** | RSA-2048 signing, SHA-256 fingerprinting |
| **Storage** | Windows DPAPI |

## Getting Started

### Prerequisites

- Visual Studio 2022
- .NET 10 SDK
- Neon PostgreSQL account

### Setup Steps

1. **Clone the repository** to your local machine.
2. **Run `schema.sql`** in your Neon PostgreSQL database to set up the required tables.
3. **Configure the connection string** in `Lock.API/appsettings.json` (or via environment variables).
4. **Generate RSA keys** (private key in `Lock.API/Keys/`, public key in `Lock.Client/Resources/`).
5. **Run `Lock.API`** using `dotnet run`.
6. **Generate a license key** using the `Lock.KeyGen` script.
7. **Run `Lock.Client`** from Visual Studio or the published executable.

## License Key Format

The system uses a `XXXXX-XXXXX-XXXXX-XXXXX-XXXXX` format. Each group consists of random uppercase letters or digits, with the final group being a checksum of the first four groups (sum of ASCII values modulo 100,000).

## Security Design

**HWID Fingerprinting**: Creates a composite fingerprint from motherboard serial, system UUID, and Windows MachineGuid. This ensures that a license is bound to a specific physical PC.

**DPAPI Storage**: Locally stores activation tokens encrypted with Windows Data Protection API (DPAPI). This ensures they can only be read by the current user on the same machine, preventing token portability.

**RSA Signing**: The API signs tokens using RSA-2048. The client verifies this signature using an embedded public key, ensuring that the token was issued by the legitimate server and has not been tampered with.
