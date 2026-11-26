# FTF Privacy Policy

**Last updated: 26 November 2026**
**Applies to: FTF for Windows, version 1.0.6 and later**
**Publisher: Moonto Studio**

## The short version

FTF converts files on your own computer. It has **no accounts, no telemetry, no analytics, no advertising, and no
crash reporting**. Nothing about what you convert, or that you converted it, is ever sent anywhere.

The only times FTF contacts the internet are listed in "Network connections" below, and each one is either something
you asked for or a download you started.

## What FTF stores on your computer

Everything FTF remembers stays in `%APPDATA%\FTF` on your PC. It never leaves your computer unless you copy it there
yourself.

| What | Where | Why |
|---|---|---|
| Your settings (theme, language, output folder, shortcuts) | `settings.json` | So FTF opens the way you left it. |
| Your presets and donut-toolbar profiles | `presets.json`, `profiles.json` | They are yours; FTF keeps them. |
| A list of finished conversions (file names, formats, times, and the preset each job ran) | `history.json` | The Home page's "Recent conversions" list. Clear it from the app, or turn the history off in Settings → General. |
| A diagnostic log (last 7 days) | `logs\ftf-*.log` | Only to troubleshoot when something goes wrong. It stays on your PC; FTF never uploads it. |
| Downloaded conversion engines | `engines\` | The optional engines you chose to install (see below). |

All of these live in `%APPDATA%\FTF` on your PC, and never leave your computer unless you copy them there yourself.

## Network connections

FTF contacts the internet in exactly these cases:

**1. Downloading an optional conversion engine — only when you ask.**
Some formats need a program that FTF does not bundle (LibreOffice, Pandoc, FFmpeg, Ghostscript, calibre, Tesseract
OCR, the Eclipse Temurin Java runtime, H2Orestart). When you click Install on the Engines page, or accept the offer
during first-run setup, FTF downloads that program from its official release page and verifies it against a pinned
SHA-256 hash. **FTF sends nothing about you or your files** — the request carries only a `FTF/<version>` user agent
string, which download servers require.

The download servers are the projects' own sites:

| Engine | Downloaded from |
|---|---|
| LibreOffice | `download.documentfoundation.org` |
| Pandoc | `github.com/jgm/pandoc` |
| FFmpeg | `github.com/GyanD/codexffmpeg` |
| Ghostscript | `conda.anaconda.org` (conda-forge) |
| calibre | `download.calibre-ebook.com` |
| Tesseract OCR and language data | `github.com/UB-Mannheim/tesseract`, `github.com/tesseract-ocr/tessdata_fast` |
| Eclipse Temurin JRE | `github.com/adoptium` |
| H2Orestart | `github.com/ebandal/H2Orestart` |
| rhwp and 7-Zip (bundled, not downloaded) | — |

Each of these servers sees your IP address and the user agent string, as any web request does. Their handling of that
information is governed by their own privacy policies, not this one.

**2. Downloading a file you dragged from a web browser.**
If you drag a link (rather than a file) onto FTF, FTF fetches that address so you can convert what it points to. The
address is the one you dragged; nothing else is sent.

**FTF does not check for updates.** There is no update ping, no version check, and no phone-home on startup.

## What FTF never does

- No usage statistics, analytics, or telemetry of any kind.
- No crash reporting.
- No advertising, and no advertising identifiers.
- No account, sign-in, or licence check.
- No access to your files beyond the ones you hand it.
- No reading of your browsing history, contacts, calendar, location, camera, or microphone.
- Nothing is sold, rented, or shared with third parties. There is nothing to share.

## Windows permissions FTF uses, and why

| Permission | Why |
|---|---|
| Read and write files you choose | That is what a converter does: read the source, write the result. |
| Start with Windows (optional, on by default) | So the drag gesture works without opening the app first. Turn it off in Settings → General. |
| Explorer context menu and Send To entry (optional) | "Convert with FTF" in the right-click menu. Turn it off in Settings → General. |
| A low-level keyboard and mouse hook | FTF's whole interaction is "hold a key and drag a file". That needs a system-wide hook to see which modifier keys are held and when a drag starts. |
| Read the selected files in Explorer | To know what you dragged. Only file paths, only while a gesture is active. |

The keyboard hook is the one permission worth explaining plainly. It is the mechanism the app is built on, and it
works like this:

- The hook receives every key press and release, because that is how a system-wide hook works.
- FTF **acts on only two things**: whether a key that one of your shortcuts uses is currently held down, and the
  donut's flip key. Every other key is inspected, found irrelevant, and discarded immediately.
- **Nothing is logged, stored, or transmitted.** There is no keystroke history, no clipboard capture, and no screen
  capture.

If you would rather not have a system-wide hook at all, turn the trigger gestures off in Settings → Triggers. FTF then
works from its own window only, with no hook installed.

## Children

FTF is a general-purpose file utility. It is not directed at children and collects no information from anyone,
including children.

## Your control over your data

Because everything stays on your PC, you are already in control:

- **Delete what FTF stored:** remove `%APPDATA%\FTF`. The app starts fresh.
- **Stop the history:** Settings → General → "Keep history" off.
- **Remove the logs:** delete `%APPDATA%\FTF\logs`.
- **Uninstall:** Settings → Apps → FTF → Uninstall. The uninstaller removes the program; the data folder is left for
  you to delete, in case you reinstall.

## Changes to this policy

If a future version of FTF starts doing something this policy does not describe — for example, adding an update check
— this page will be updated before that version is published, and the "Last updated" date above will change.

## Contact

Questions about this policy or about FTF's handling of data:

**Moonto Studio** — by opening an issue at <https://github.com/WDHUS4G/Filee/issues>.

---

*FTF is a derivative work of [Filee](https://github.com/KnifeLemon/Filee) (MIT). This policy covers the FTF build
published by Moonto Studio.*
