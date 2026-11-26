# Licensing notes for redistribution

This file explains how FTF may be redistributed — for example through the Microsoft Store — and why each part of
the package is allowed to travel with it. It is a companion to [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md),
which lists every component and its license.

## What FTF itself is

FTF is a derivative work of [Filee](https://github.com/KnifeLemon/Filee), which is MIT-licensed. The MIT license
permits modification and redistribution, provided the original copyright notice is kept. It is: see
[LICENSE](LICENSE), which carries both the original notice (Myeonggu Kim / Filee contributors) and the notice of the
derivative's publisher (Moonto Studio).

The name, icon and theme differ from upstream Filee. That is allowed by MIT. FTF does not claim to *be* Filee, and
the About page and this repository both state the derivation.

## What ships inside the installer

| Component | License | Redistribution basis |
|---|---|---|
| FTF's own code (`FTF.exe`, `FTF.dll`, `ftf-cli.exe`, `Filee*.dll`) | MIT | The publisher's own work, plus Filee's MIT grant. |
| .NET runtime | MIT | Redistributable as part of a self-contained app. |
| Avalonia, SkiaSharp, HarfBuzzSharp, CommunityToolkit.Mvvm, Markdig, SharpCompress, and the rest of the NuGet set | MIT / BSD / Apache-2.0 | All permissive; the notices are listed in THIRD-PARTY-NOTICES.md. |
| **Microsoft Visual C++ runtime** (`msvcp140.dll`, `vcruntime140.dll`, …) | Microsoft Visual C++ Redistributable license | Taken from Microsoft's own `vc_redist.x64.exe`. Microsoft grants redistribution of these files as "distributable code" of Visual Studio; the app-local copies are exactly the files that installer ships. |
| rhwp | MIT | Bundled from its official release. |
| 7-Zip (`7z.exe`, `7z.dll`) | LGPL-2.1-or-later; `7z.dll` also carries the unRAR restriction | Bundled unmodified as a separate program. FTF only *reads* RAR archives; it does not implement a RAR-compatible archiver. |
| Noto Sans fonts | SIL Open Font License 1.1 | Bundled unmodified; the OFL permits embedding and redistribution. |

## What is downloaded later, by the user

These are **not** in the installer. They are fetched from their official release pages when the user chooses to
install them (Settings → Engines, or the first-run setup), and each runs as a **separate process**:

| Component | License | Why this is not a derivative work |
|---|---|---|
| FFmpeg (GPL-3.0 build) | GPL-3.0-or-later | Separate executable, invoked as a child process. FTF does not link against it. |
| Pandoc | GPL-2.0-or-later | Separate executable. |
| calibre | GPL-3.0 | Separate executable. |
| H2Orestart (LibreOffice extension) | GPL-3.0 | Separate extension, loaded by LibreOffice, not by FTF. |
| Ghostscript | AGPL-3.0 | Separate executable. FTF does not expose it as a network service, so the AGPL's network clause does not attach. |
| LibreOffice | MPL-2.0 | Separate application. |
| Eclipse Temurin JRE | GPL-2.0 with Classpath Exception | Separate runtime used by H2Orestart. |
| Tesseract OCR | Apache-2.0 | Separate executable. |

The GPL family's obligations (offering source) are met by the upstream projects: each download records the exact
version and URL in `src/Filee.Engines/Infrastructure/engines.json`, and the source is available from the linked
project. FTF distributes no modified GPL code.

Because these components are downloaded rather than shipped, and run as independent programs, they are "mere
aggregation" under the GPL: their licenses do not extend to FTF's own code.

## The user's own copies

FTF also accepts a copy the user already has (Settings → Engines): a path to `ffmpeg.exe`, a LibreOffice install, and
so on. Nothing is redistributed in that case.

## If you redistribute FTF yourself

Keep `LICENSE`, `THIRD-PARTY-NOTICES.md` and `NOTICES.txt` next to `FTF.exe` (the installer does this), and do not
add or remove components without updating the notices.
