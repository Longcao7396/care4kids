# =============================================================
# copy-test-deps.ps1
# Copies transitive NuGet dependencies into GiveAID.Tests\bin\Debug
# so that `dotnet test` (the vstest adapter) can resolve them at runtime.
#
# Why this exists:
#   GiveAID.Tests is a legacy MSBuild (non-SDK) project that targets
#   .NET Framework 4.7.2.  When run via `dotnet test`, the test host
#   does not always honour HintPath references for transitive deps,
#   so runtime calls like `BCrypt.HashPassword` and
#   `JwtSecurityTokenHandler.WriteToken` fail with FileNotFoundException
#   for System.Memory 4.0.1.1 and Microsoft.IdentityModel.Abstractions 6.21.0.
#
# This script copies those DLLs (from the local `packages\` cache) into
# bin\Debug so they are loaded side-by-side with GiveAID.Tests.dll.
#
# After running once, the files are present for subsequent `dotnet test`
# invocations.  Re-run after `dotnet clean` or after pulling new package
# versions.
#
# Usage:
#   powershell -NoProfile -ExecutionPolicy Bypass -File scripts\copy-test-deps.ps1
# =============================================================

$ErrorActionPreference = 'Stop'

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition
$repoRoot   = Resolve-Path (Join-Path $scriptRoot '..\..\')
$testDir    = Join-Path $repoRoot 'GiveAID.Tests'
$binDir     = Join-Path $testDir 'bin\Debug'
$packages   = Join-Path $repoRoot 'packages'

if (-not (Test-Path $binDir)) {
    Write-Host "[copy-test-deps] ERROR: $binDir does not exist. Run `dotnet build` first." -ForegroundColor Red
    exit 1
}

# Map: package-version → relative path inside that package's lib\ folder
# We always pick the net472 (or net462) build for net472 test runs.
# Note: some NuGet packages ship the same DLL under a different filename
# (e.g. Microsoft.AspNet.WebApi.Core ships System.Web.Http.dll), or
# place the assembly in a sibling package folder (xunit.core lives inside
# xunit.extensibility.core, xunit.execution.desktop lives inside
# xunit.extensibility.execution, etc.).  We map to the actual on-disk path.
$deps = @(
    @{ PackageDir = 'System.Memory.4.5.4';                                  Lib = 'net461';    FileName = 'System.Memory.dll'                                },
    @{ PackageDir = 'System.Buffers.4.5.1';                                 Lib = 'net461';    FileName = 'System.Buffers.dll'                               },
    @{ PackageDir = 'System.Numerics.Vectors.4.5.0';                        Lib = 'net46';     FileName = 'System.Numerics.Vectors.dll'                      },
    @{ PackageDir = 'System.Runtime.CompilerServices.Unsafe.4.5.3';         Lib = 'net461';    FileName = 'System.Runtime.CompilerServices.Unsafe.dll'       },
    @{ PackageDir = 'System.Diagnostics.DiagnosticSource.8.0.0';            Lib = 'net462';    FileName = 'System.Diagnostics.DiagnosticSource.dll'          },
    @{ PackageDir = 'Microsoft.IdentityModel.Abstractions.6.21.0';           Lib = 'net472';    FileName = 'Microsoft.IdentityModel.Abstractions.dll'         },
    @{ PackageDir = 'Microsoft.IdentityModel.Logging.6.21.0';                Lib = 'net472';    FileName = 'Microsoft.IdentityModel.Logging.dll'              },
    @{ PackageDir = 'Microsoft.IdentityModel.Tokens.6.21.0';                 Lib = 'net472';    FileName = 'Microsoft.IdentityModel.Tokens.dll'               },
    @{ PackageDir = 'System.IdentityModel.Tokens.Jwt.6.21.0';                Lib = 'net472';    FileName = 'System.IdentityModel.Tokens.Jwt.dll'              },
    @{ PackageDir = 'Microsoft.IdentityModel.JsonWebTokens.6.21.0';          Lib = 'net472';    FileName = 'Microsoft.IdentityModel.JsonWebTokens.dll'        },
    @{ PackageDir = 'BCrypt.Net-Next.4.0.3';                                Lib = 'net472';    FileName = 'BCrypt.Net-Next.dll'                              },
    @{ PackageDir = 'Newtonsoft.Json.13.0.3';                                Lib = 'net45';     FileName = 'Newtonsoft.Json.dll'                              },
    @{ PackageDir = 'EntityFramework.6.4.4';                                Lib = 'net45';     FileName = 'EntityFramework.dll'                              },
    @{ PackageDir = 'EntityFramework.6.4.4';                                Lib = 'net45';     FileName = 'EntityFramework.SqlServer.dll'                    },
    @{ PackageDir = 'xunit.assert.2.4.2';                                   Lib = 'netstandard1.1'; FileName = 'xunit.assert.dll'                          },
    @{ PackageDir = 'xunit.abstractions.2.0.3';                             Lib = 'netstandard2.0'; FileName = 'xunit.abstractions.dll'                      },
    @{ PackageDir = 'xunit.extensibility.core.2.4.2';                       Lib = 'net452';    FileName = 'xunit.core.dll'                                   },
    @{ PackageDir = 'xunit.extensibility.execution.2.4.2';                  Lib = 'net452';    FileName = 'xunit.execution.desktop.dll'                     },
    @{ PackageDir = 'xunit.runner.visualstudio.2.4.2';                      Lib = 'net462';    FileName = 'xunit.runner.visualstudio.testadapter.dll'        },
    @{ PackageDir = 'Moq.4.18.4';                                           Lib = 'net462';    FileName = 'Moq.dll'                                          },
    @{ PackageDir = 'Microsoft.AspNet.WebApi.Core.5.2.9';                   Lib = 'net45';     FileName = 'System.Web.Http.dll'                              }
)

$copied = 0
$missing = @()

foreach ($d in $deps) {
    $src = Join-Path $packages "$($d.PackageDir)\lib\$($d.Lib)\$($d.FileName)"
    $dst = Join-Path $binDir $d.FileName

    if (Test-Path $src) {
        Copy-Item -Path $src -Destination $dst -Force
        $copied++
    } else {
        $missing += $src
    }
}

Write-Host "[copy-test-deps] Copied $copied DLL(s) into $binDir" -ForegroundColor Green

if ($missing.Count -gt 0) {
    Write-Host "[copy-test-deps] WARNING: $($missing.Count) source DLL(s) not found:" -ForegroundColor Yellow
    foreach ($m in $missing) {
        Write-Host "  - $m" -ForegroundColor Yellow
    }
    Write-Host "[copy-test-deps] These are usually harmless if you don't run the tests that need them." -ForegroundColor Yellow
}

# =============================================================
# Patch app.config with assembly binding redirects.
#
# GiveAID.Web/Web.config has full binding redirects but GiveAID.Tests
# ships with an empty <runtime>.  Without them, the loader asks for
# System.Memory 4.0.1.1 even when 4.5.4 is present on disk.
# =============================================================
$appConfigPath = Join-Path $testDir 'app.config'
if (Test-Path $appConfigPath) {
    [xml]$cfg = Get-Content $appConfigPath -Raw
    $ns = 'urn:schemas-microsoft-com:asm.v1'

    # Build the runtime/assemblyBinding block
    $runtime = $cfg.configuration.runtime
    if ($null -eq $runtime) {
        $runtime = $cfg.CreateElement('runtime')
        $cfg.configuration.AppendChild($runtime) | Out-Null
    }
    $assemblyBinding = $runtime.assemblyBinding
    if ($null -eq $assemblyBinding) {
        $assemblyBinding = $cfg.CreateElement('assemblyBinding')
        $assemblyBinding.SetAttribute('xmlns', $ns) | Out-Null
        $runtime.AppendChild($assemblyBinding) | Out-Null
    }

    $redirects = @(
        # NOTE on System.Memory: BCrypt.Net-Next requests Version=4.0.0.0-4.0.1.2,
        # but the only build present in our local `packages\` cache is 4.5.4 (in
        # System.Memory.4.5.4/lib/net461).  We rewrite all 4.0.x requests to 4.5.4
        # so that the loader finds a matching assembly on disk.
        @{ Name = 'System.Memory';                                PublicKeyToken = 'cc7b13ffcd2ddd51'; Old = '0.0.0.0-4.0.1.99'; New = '4.5.4'   },
        @{ Name = 'System.Buffers';                               PublicKeyToken = 'cc7b13ffcd2ddd51'; Old = '0.0.0.0-4.0.3.0';  New = '4.0.3.0' },
        @{ Name = 'System.Runtime.CompilerServices.Unsafe';       PublicKeyToken = 'b03f5f7f11d50a3a'; Old = '0.0.0.0-6.0.0.0';  New = '6.0.0.0' },
        @{ Name = 'System.Numerics.Vectors';                      PublicKeyToken = 'b03f5f7f11d50a3a'; Old = '0.0.0.0-4.1.4.0';  New = '4.1.4.0' },
        @{ Name = 'System.Diagnostics.DiagnosticSource';          PublicKeyToken = 'cc7b13ffcd2ddd51'; Old = '0.0.0.0-8.0.0.0';  New = '8.0.0.0' },
        @{ Name = 'Microsoft.IdentityModel.Tokens';               PublicKeyToken = '31bf3856ad364e35'; Old = '0.0.0.0-6.21.0.0'; New = '6.21.0.0' },
        @{ Name = 'System.IdentityModel.Tokens.Jwt';              PublicKeyToken = '31bf3856ad364e35'; Old = '0.0.0.0-6.21.0.0'; New = '6.21.0.0' },
        @{ Name = 'Microsoft.IdentityModel.Abstractions';         PublicKeyToken = '31bf3856ad364e35'; Old = '0.0.0.0-6.21.0.0'; New = '6.21.0.0' },
        @{ Name = 'Newtonsoft.Json';                              PublicKeyToken = '30ad4fe6b2a6aeed'; Old = '0.0.0.0-13.0.0.0'; New = '13.0.0.0' },
        @{ Name = 'EntityFramework';                              PublicKeyToken = 'b77a5c561934e089'; Old = '0.0.0.0-6.0.0.0';  New = '6.0.0.0'  },
        @{ Name = 'EntityFramework.SqlServer';                    PublicKeyToken = 'b77a5c561934e089'; Old = '0.0.0.0-6.0.0.0';  New = '6.0.0.0'  }
    )

    $added = 0
    $updated = 0
    foreach ($r in $redirects) {
        $existing = $assemblyBinding.SelectSingleNode("dependentAssembly[assemblyIdentity/@name='$($r.Name)']")
        if ($null -eq $existing) {
            # New entry — append
            $dep = $cfg.CreateElement('dependentAssembly')
            $id  = $cfg.CreateElement('assemblyIdentity')
            $id.SetAttribute('name',          $r.Name)           | Out-Null
            $id.SetAttribute('publicKeyToken', $r.PublicKeyToken)| Out-Null
            $id.SetAttribute('culture',      'neutral')         | Out-Null
            $br  = $cfg.CreateElement('bindingRedirect')
            $br.SetAttribute('oldVersion', $r.Old) | Out-Null
            $br.SetAttribute('newVersion', $r.New) | Out-Null
            $dep.AppendChild($id)  | Out-Null
            $dep.AppendChild($br) | Out-Null
            $assemblyBinding.AppendChild($dep) | Out-Null
            $added++
        }
        else {
            # Existing entry — rewrite its oldVersion/newVersion to the current
            # values in case the script's mapping changed between runs.
            $brNode = $existing.SelectSingleNode('bindingRedirect')
            if ($brNode -ne $null) {
                $oldAttr = $brNode.GetAttribute('oldVersion')
                $newAttr = $brNode.GetAttribute('newVersion')
                if ($oldAttr -ne $r.Old -or $newAttr -ne $r.New) {
                    $brNode.SetAttribute('oldVersion', $r.Old) | Out-Null
                    $brNode.SetAttribute('newVersion', $r.New) | Out-Null
                    $updated++
                }
            }
        }
    }

    if ($added -gt 0 -or $updated -gt 0) {
        $cfg.Save($appConfigPath)
        Write-Host "[copy-test-deps] Added $added, updated $updated assembly binding redirect(s) in app.config" -ForegroundColor Green
    } else {
        Write-Host "[copy-test-deps] app.config binding redirects already match" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "Ready.  You can now run:" -ForegroundColor Cyan
Write-Host "  cd GiveAID.Tests" -ForegroundColor Cyan
Write-Host "  dotnet test --no-build" -ForegroundColor Cyan
