# Development software bill of materials

Version 0.2.1, Windows x64, 2026-11-06.

| Component | Version |
|---|---|
| English B2 Academy / Core | 0.2.1 / local original source |
| .NET SDK (build only) | 10.0.403 |
| .NET runtime / Windows Desktop (bundled) | 10.0.14 |
| Microsoft.Data.Sqlite | 10.0.14 |
| Microsoft.Data.Sqlite.Core | 10.0.14 |
| SQLitePCLRaw.bundle_e_sqlite3 | 2.1.13 |
| SQLitePCLRaw.core | 2.1.13 |
| SQLitePCLRaw.lib.e_sqlite3 | 2.1.13 |
| SQLitePCLRaw.provider.e_sqlite3 | 2.1.13 |
| System.Speech | 10.0.14 |
| WiX (MSI build only) | 3.14.1 |

Native Windows Installer, Windows audio APIs and installed Windows fonts/voices
are platform prerequisites. No browser, WebView, external educational dataset,
AI model, cloud SDK or downloaded voice model is used.

This is a human-readable development inventory, not a file-level SPDX/CycloneDX
certification. The exact runtime version is in the published runtimeconfig file;
resolved packages are in project.assets.json. Dependency advisory checking on
2026-11-06 reported no known vulnerable packages from the configured NuGet source.
That is not a security guarantee or substitute for a full audit.

No signing certificate or signed-update infrastructure is included.
