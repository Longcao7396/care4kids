# GiveAID Self-Improving Auditor

Lightweight runtime + static analysis for the GiveAID project.
Runs a sequence of independent checks against the live system, fixes
simple problems itself, and reports everything it could not auto-fix.

## What it does (in order)

1. **Runtime health** — backend ports, frontend port, DB connectivity, login API
2. **IIS Express integrity** — process alive? Windows Auth disabled? Bindings correct?
3. **Backend code quality** — finds compile errors, banned patterns, custom crypto
4. **Frontend code quality** — finds flash-and-redirect bugs, missing error handling
5. **Security smoke** — verifies JWT secret length, password hashing format
6. **Apply safe fixes** — only changes that are reversible and isolated
7. **Report** — writes to `scripts/audit-report.md`

## Usage

```powershell
cd 'C:\Users\admin\Desktop\project NGO'
powershell -ExecutionPolicy Bypass -File scripts/auditor.ps1
```

## Philosophy

- Read-only by default. Only writes to DB when explicitly safe (e.g. rehash
  demo accounts).
- Each fix must have a comment explaining why it is safe.
- If a fix would touch more than one component, stop and report instead.
