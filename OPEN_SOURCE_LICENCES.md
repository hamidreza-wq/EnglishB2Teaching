# Open-source licence register

Reviewed 2026-11-06. Package licences checked against the exact restored NuGet
package metadata, not inferred from free availability.

| Component | Version | Licence | Role |
|---|---|---|---|
| .NET runtime / WPF | 10.0.14 | MIT + upstream third-party notices | Self-contained execution/UI |
| .NET SDK | 10.0.403 | MIT + upstream notices | Build tooling |
| Microsoft.Data.Sqlite | 10.0.14 | MIT | SQLite ADO.NET interface |
| Microsoft.Data.Sqlite.Core | 10.0.14 | MIT | Database provider |
| SQLitePCLRaw.bundle_e_sqlite3 | 2.1.13 | Apache-2.0 | Native-provider setup |
| SQLitePCLRaw.core | 2.1.13 | Apache-2.0 | Native SQLite bridge |
| SQLitePCLRaw.provider.e_sqlite3 | 2.1.13 | Apache-2.0 | Native-provider bindings |
| SQLitePCLRaw.lib.e_sqlite3 | 2.1.13 | Apache-2.0 package; SQLite engine public-domain dedication | Native database |
| System.Speech | 10.0.14 | MIT | Installed Windows speech voices |
| WiX Toolset | 3.14.1 | MS-RL | MSI compiler; not installed with app |

Sources:
- https://github.com/dotnet/runtime/blob/main/LICENSE.TXT
- https://github.com/dotnet/wpf/blob/main/LICENSE.TXT
- https://github.com/dotnet/efcore/blob/main/LICENSE.txt
- https://github.com/ericsink/SQLitePCL.raw/blob/master/LICENSE.TXT
- https://sqlite.org/copyright.html
- https://github.com/wixtoolset/wix3/blob/develop/LICENSE.TXT

Preserved licences: [Apache 2.0](docs/licences/SQLITEPCLRAW-APACHE-2.0.txt),
[MS-RL](docs/licences/WIX-MS-RL.txt), [MIT notices](THIRD_PARTY_NOTICES.md).
The MSI includes upstream .NET notices and the Apache/MS-RL licence texts.
The installer source's standard MSI error-dialog protocol is adapted from WiX
and is available here under MS-RL with its licence; custom application code is
not thereby assigned that licence.

Windows, its installed fonts, voices and platform APIs are prerequisites under
their own Windows terms; no font files or voice models are redistributed.
Earlier Windows App SDK / WinUI dependencies are removed, not licensed for
distribution by this register. No paid services are required.
