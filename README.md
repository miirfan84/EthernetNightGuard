# Ethernet NightGuard

A local-only WPF utility for investigating Ethernet LED control on the HP ProBook 4 G1i / Realtek PCIe GbE Family Controller.

## Current status: v0.2 diagnostics

This version discovers a physical adapter whose name or description matches Ethernet, Realtek, or GbE, then displays its Windows-reported status and link speed. The PowerShell query runs off the UI thread, drains output safely, and has a five-second timeout.

The query is **read-only**. It does not change network configuration, edit the registry, reset/disable the adapter, or write hardware registers.

## LED research finding

The Linux kernel's `r8169` driver has public LED-control support for RTL8168H, and the upstream change reports testing on RTL8168H. That establishes a promising route to validate the hardware, but it does **not** establish a supported Windows control interface. The physical LEDs actually wired on this HP model also need testing. See [the research notes and safe validation plan](docs/LED-CONTROL-RESEARCH.md).

This is **not yet a working Ethernet LED controller**. Day/Night controls remain disabled intentionally until a supported, safe Windows control mechanism is verified. Do not use arbitrary PCI/PHY register writes, EEPROM/eFuse programming, or adapter disablement as a substitute for LED control.

## Build with GitHub Actions

1. Open the repository's **Actions** tab.
2. Select **Build portable Windows app** and run the workflow, or push a commit to a branch covered by the workflow.
3. When the run completes, open it and download the `EthernetNightGuard-win-x64` artifact.
4. Extract the ZIP and run `EthernetNightGuard.exe` on Windows.

The workflow publishes a self-contained, single-file, 64-bit Windows app. The target laptop does not need the .NET runtime installed. GitHub artifact retention applies.

## Build locally (optional)

Install the .NET 8 SDK on a development machine, open PowerShell in this folder, and run:

```powershell
dotnet publish .\EthernetNightGuard.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o .\publish
```

Output: `publish\EthernetNightGuard.exe`

## Next engineering step

Validate physical LED exposure using a Linux live USB when a reboot is convenient, then continue searching for a documented Windows LED-control interface. Keep Windows diagnostics read-only until a supported method is confirmed.
