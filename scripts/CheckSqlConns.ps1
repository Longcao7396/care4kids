# =============================================================================
# CheckSqlConns.ps1 — Show active TCP connections to SQL Server ports.
# Single source of truth: (localdb)\MSSQLLocalDB uses dynamic ports; this
# script also includes 1433 (SQL Server default) and 62580 (legacy fallback).
# =============================================================================

Get-NetTCPConnection -State Established -ErrorAction SilentlyContinue |
    Where-Object { $_.RemotePort -in @(1433, 62580) -or $_.LocalPort -in @(1433, 62580) } |
    Select-Object LocalAddress, LocalPort, RemoteAddress, RemotePort, OwningProcess |
    Format-Table -AutoSize
