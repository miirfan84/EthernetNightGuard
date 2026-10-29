# Ethernet NightGuard (prototype)

A local-only WPF prototype for the HP ProBook 4 G1i / Realtek PCIe GbE Family Controller.

## Important limitation

This is currently a **UI and adapter-status prototype**, not a working Ethernet LED controller. The Windows driver did not expose an LED-related advanced property in the checks supplied so far. The Day/Night controls are deliberately disabled until a supported, safe hardware-control mechanism is confirmed. This app does not change network settings.

## Build without installing anything on your laptop (GitHub Actions)

You can use GitHub's Windows build runner to compile the app remotely.

1. Create a new, empty GitHub repository (for example `EthernetNightGuard`).
2. Upload the contents of this folder to the repository root, including the hidden `.github` folder.
3. Open the repository's **Actions** tab and enable Actions if prompted.
4. Select **Build portable Windows app** and click **Run workflow**. Alternatively, push to the `main` branch.
5. When the workflow completes, open the run and download the `EthernetNightGuard-win-x64` artifact.
6. Extract the ZIP and double-click `EthernetNightGuard.exe`.

The EXE is published as a self-contained, single-file, 64-bit Windows app, so the target laptop does not need the .NET runtime installed. The GitHub account/repository is subject to GitHub's own terms and artifact retention.

## Build locally (optional)

Install the .NET 8 SDK on a development machine, open PowerShell in this folder, and run:

```powershell
dotnet publish .\EthernetNightGuard.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o .\publish
```

Output: `.\publish\EthernetNightGuard.exe`

## Adapter alias

The prototype currently looks for `Ethernet 18`, based on the supplied diagnostics. If Windows renames the adapter, edit `AdapterName`/the PowerShell query in `MainWindow.xaml.cs`.

## Next engineering step

Identify and validate a supported Windows control interface for the exact Realtek PCI ID before implementing physical LED control. Do not write arbitrary PCI/PHY registers or disable the adapter as a substitute.
