# Third-party notices

Reviewed 2026-11-06 for the dashboard foundation.

## .NET runtime and Windows Presentation Foundation

Sources:
- https://github.com/dotnet/runtime/blob/main/LICENSE.TXT
- https://github.com/dotnet/wpf/blob/main/LICENSE.TXT

Both use the following MIT licence. Preserve upstream third-party notices when
publishing runtime files; this summary is not a substitute for their complete
distribution notices.

The MIT License (MIT)

Copyright (c) .NET Foundation and Contributors

All rights reserved.

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

## Windows platform

Segoe UI and Segoe MDL2 Assets are referenced from Windows. No font files,
third-party icon packs, photographs, audio recordings or speech models are bundled.
Windows and system-font use are governed by the user's Windows licence.

## Removed dependency

An initial WinUI build was replaced with WPF at the owner's request. Its
Windows App SDK packages used separate Microsoft binary terms. They are not
dependencies of the current application. Do not distribute obsolete WinUI build
outputs or assume source MIT licensing applies to those binaries.

## Version 0.2 additions

Microsoft.Data.Sqlite / Microsoft.Data.Sqlite.Core / System.Speech:
Copyright (c) Microsoft Corporation. Licensed under MIT; the permission and
disclaimer text above applies to these components as well.

SQLitePCLRaw:
Copyright 2014-2024 SourceGear, LLC. Licensed under Apache License 2.0.
The complete licence is included in `docs/licences/SQLITEPCLRAW-APACHE-2.0.txt`
in source and `licences/SQLITEPCLRAW-APACHE-2.0.txt` in the installed application.
Source: https://github.com/ericsink/SQLitePCL.raw
SQLite engine dedication: https://sqlite.org/copyright.html

WiX Toolset 3.14.1 (build tooling):
Copyright (c) .NET Foundation and contributors. Microsoft Reciprocal License.
The standard MSI error-dialog protocol in the installer source is adapted from
WiX; the source is supplied with `docs/licences/WIX-MS-RL.txt`. The installed
package includes the licence notice. No WiX executable compiler is bundled.
Source: https://github.com/wixtoolset/wix3/tree/wix3141rtm

Preserve the complete upstream .NET third-party notices included with the
self-contained distribution.
