# PS4 fpkg CUSA Locker for SharpProspero

This project is a SharpProspero C# payload-style application. It replaces the old native C/Linux ELF build.

The app locks PS4 fpkg `CUSA*` folders so Rebuild Database GC cannot delete them. It covers these content
folders on configured internal and external drive roots:

- `/user/app`
- `/user/addcont`
- `/user/patch`
- `/user/playgo`

Only folders whose names start with `CUSA` are changed. Save data folders are not touched.

## Build

Install the .NET 10 SDK and set `SHARPPROSPERO_ROOT` to the SharpProspero SDK checkout, then run:

```powershell
pwsh ./build.ps1
```

The build script delegates to the SharpProspero payload pipeline when the SDK target file is available.

## Use

Run the generated payload ELF on the target and open:

```text
http://<console-ip>:8080
```

Use **Lock CUSA Folders** before Rebuild Database. Use **Unlock CUSA Folders** before installing,
updating, or modifying fpkg content.

## Arabic installation guide

For step-by-step Arabic installation and build instructions, see [INSTALL_AR.md](INSTALL_AR.md).
