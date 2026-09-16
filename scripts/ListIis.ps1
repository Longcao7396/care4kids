Get-NetTCPConnection -State Listen -ErrorAction SilentlyContinue | Where-Object { $_.LocalPort -eq 61508 -or $_.LocalPort -eq 44300 } | Select-Object LocalAddress, LocalPort, OwningProcess | Format-Table -AutoSize
Get-Process -Name iisexpress -ErrorAction SilentlyContinue | Select-Object Id, ProcessName, StartTime | Format-Table -AutoSize
