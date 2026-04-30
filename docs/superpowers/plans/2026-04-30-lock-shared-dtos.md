# Lock.Shared DTOs Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Clean up Lock.Shared project and add ActivationRequestDto and ActivationResponseDto for licensing communication.

**Architecture:**
- Remove the boilerplate `Class1.cs`.
- Organize DTOs into a dedicated `Dtos` namespace and directory.
- Ensure the project compiles after changes.

**Tech Stack:** .NET 8.0, C#

---

### Task 1: Cleanup and Preparation

**Files:**
- Delete: `Lock.Shared/Class1.cs`
- Create: `Lock.Shared/Dtos/`

- [ ] **Step 1: Delete Class1.cs**

Run: `rm Lock.Shared/Class1.cs`

- [ ] **Step 2: Create Dtos directory**

Run: `mkdir Lock.Shared/Dtos`

### Task 2: Implement ActivationRequestDto

**Files:**
- Create: `Lock.Shared/Dtos/ActivationRequestDto.cs`

- [ ] **Step 1: Create ActivationRequestDto.cs**

```csharp
namespace Lock.Shared.Dtos;

public class ActivationRequestDto
{
    public string Hwid { get; set; } = string.Empty;
    public string LicenseKey { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string MachineName { get; set; } = string.Empty;
    public string LicenseKeyId { get; set; } = string.Empty;
}
```

### Task 3: Implement ActivationResponseDto

**Files:**
- Create: `Lock.Shared/Dtos/ActivationResponseDto.cs`

- [ ] **Step 1: Create ActivationResponseDto.cs**

```csharp
using System;

namespace Lock.Shared.Dtos;

public class ActivationResponseDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime IssuedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public string Signature { get; set; } = string.Empty;
}
```

### Task 4: Verification

- [ ] **Step 1: Run dotnet build**

Run: `dotnet build Lock.Shared`
Expected: "Build succeeded. 0 Warning(s) 0 Error(s)"

- [ ] **Step 2: Verify file existence**

Run: `ls Lock.Shared/Dtos/`
Expected: `ActivationRequestDto.cs`, `ActivationResponseDto.cs`
