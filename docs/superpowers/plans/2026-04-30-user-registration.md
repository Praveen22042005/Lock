# User Registration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Implement user registration during the activation process.

**Architecture:** Add a new `UpsertUserAsync` method to `LicenseService` to handle user creation/update in the database. Update the activation endpoint to call this method and store the user ID in the activation record. Extend DTOs and models to support passing the full name from the client.

**Tech Stack:** .NET 10.0, Npgsql (PostgreSQL).

---

### Task 1: Update Models and DTOs

**Files:**
- Modify: `Lock.API/Models/ActivationRequest.cs`
- Modify: `Lock.Shared/Dtos/ActivationRequestDto.cs`

- [ ] **Step 1: Add FullName to ActivationRequest model**
- [ ] **Step 2: Add FullName to ActivationRequestDto**
- [ ] **Step 3: Run dotnet build to verify**

### Task 2: Implement UpsertUserAsync in LicenseService

**Files:**
- Modify: `Lock.API/Services/LicenseService.cs`

- [ ] **Step 1: Add UpsertUserAsync method**

```csharp
public async Task<Guid> UpsertUserAsync(string email, string fullName)
{
    using var connection = new NpgsqlConnection(connectionString);
    await connection.OpenAsync();

    var sql = @"
        INSERT INTO users (id, email, full_name, created_at)
        VALUES (gen_random_uuid(), @email, @fullName, @now)
        ON CONFLICT (email) 
        DO UPDATE SET 
            full_name = EXCLUDED.full_name
        RETURNING id";

    using var command = new NpgsqlCommand(sql, connection);
    command.Parameters.AddWithValue("email", email);
    command.Parameters.AddWithValue("fullName", fullName);
    command.Parameters.AddWithValue("now", DateTime.UtcNow);

    return (Guid)await command.ExecuteScalarAsync();
}
```

- [ ] **Step 2: Run dotnet build to verify**

### Task 3: Update ActivateEndpoint

**Files:**
- Modify: `Lock.API/Endpoints/ActivateEndpoint.cs`

- [ ] **Step 1: Call UpsertUserAsync and pass returned userId to UpsertActivationAsync**
- [ ] **Step 2: Run dotnet build to verify**

### Task 4: Update ActivationService in Client

**Files:**
- Modify: `Lock.Client/Services/ActivationService.cs`

- [ ] **Step 1: Map fullName to FullName in the request body in ActivateAsync**
- [ ] **Step 2: Run dotnet build on the full solution**

### Task 5: Final Verification and Push

- [ ] **Step 1: Confirm zero errors on full solution build**
- [ ] **Step 2: Commit and push changes**
- [ ] **Step 3: Wait for deployment and verify in Neon DB**
