# Ethernet NightGuard — Project Handout

**Project:** Ethernet NightGuard  
**Repository:** https://github.com/miirfan84/EthernetNightGuard  
**Current working branch:** `feature/v0.2-diagnostics`  
**Application type:** Windows desktop utility (WPF / .NET 8)  
**Target system:** HP ProBook 4 G1i 14-inch AI PC with Realtek PCIe GbE Family Controller  
**Current release state:** v0.2 diagnostics prototype — not yet an LED controller

## 1. Purpose

Ethernet NightGuard is an exploratory utility intended to eventually turn off the physical Ethernet-port LEDs while keeping the Ethernet connection active. The goal is to reduce distracting lights at night without disabling the network adapter or interrupting connectivity.

The current version focuses on safe adapter discovery and status reporting while the team investigates whether a supported LED-control interface exists for the exact laptop and Windows driver.

## 2. Current verified environment

Diagnostics supplied during development identify:

- Adapter alias: `Ethernet 18`
- Adapter: Realtek PCIe GbE Family Controller
- PCI identifier: `PCI\VEN_10EC&DEV_8168&SUBSYS_8DEA103C&REV_15`
- Driver version: `1168.27.919.2025`
- Driver date: 2025-09-19
- Driver file: `rt68cx21x64.sys`
- Reported link speed during app testing: 1 Gbps
- Windows adapter status during app testing: Connected / Up

The application successfully launched in the user's Windows environment and displayed the adapter as connected at 1 Gbps. This validates basic application launch and adapter-status display; it does **not** validate physical LED control.

## 3. What v0.2 does

- Discovers a physical network adapter using broad name/description matching for Ethernet, Realtek, or GbE rather than relying only on the alias `Ethernet 18`.
- Displays the selected adapter's Windows-reported status and link speed.
- Runs the PowerShell query off the UI thread and includes a timeout to reduce the risk of a frozen window.
- Provides a simple dark WPF interface with a refresh control.
- Clearly marks LED control as unavailable/unverified.
- Keeps Day Mode and Night Mode actions disabled.

## 4. What v0.2 does not do

- It does not switch Ethernet LEDs off or on.
- It does not guarantee that every physical LED on the laptop can be controlled.
- It does not configure schedules or background automation.
- It does not modify network configuration, reset or disable the adapter, edit the registry, or intentionally write hardware registers.
- It does not use a cloud backend or paid API.

## 5. Why the EXE is about 63 MB

The current GitHub Actions workflow publishes a self-contained, single-file Windows executable. This bundles the .NET runtime and application dependencies so the target PC does not need the .NET Desktop Runtime installed. That convenience makes the file much larger than the app's own source code.

A smaller alternative is a framework-dependent build, which relies on a compatible .NET Desktop Runtime already being installed on the target PC. Other publish optimizations may be tested, but any trimmed build must be tested carefully because WPF/runtime features can be sensitive to trimming. No smaller size is promised until a new build has been produced and measured.

Microsoft documentation: https://learn.microsoft.com/en-us/dotnet/core/deploying/single-file/overview

## 6. LED-control research so far

The Windows checks supplied during development did not find an LED-related Advanced property in the installed Realtek driver. This means that a simple documented driver switch has not been found yet; it does not prove that the hardware is incapable of LED control.

Public Linux kernel discussion documents LED support in the `r8169` driver for many RTL8168/RTL8101 variants, with the patch author reporting testing on RTL8168H. The same discussion cautions that the driver cannot know which LED devices are physically wired on a particular board.

Research source: https://lists.openwall.net/netdev/2023/12/16/141

This is a promising hardware-validation route, but it does not establish that a supported Windows API or control interface exists for this laptop.

Detailed research notes: https://github.com/miirfan84/EthernetNightGuard/blob/feature/v0.2-diagnostics/docs/LED-CONTROL-RESEARCH.md

## 7. Safety principles

The project follows these rules:

1. **Read-only first.** Gather driver and interface evidence before attempting any hardware action.
2. **Keep the network connected.** Do not disable or reset the adapter as a substitute for LED control.
3. **No speculative registry changes.** Unknown Realtek registry values must not be treated as LED controls without evidence.
4. **No arbitrary PCI/MMIO/PHY register writes.** Incorrect low-level writes can destabilize the device or system.
5. **No EEPROM/eFuse programming.** Such changes can be persistent and are not appropriate for an initial software utility.
6. **No fake success.** The UI must never claim the LEDs are off unless the physical behavior has been verified.
7. **Keep the main branch stable.** Development is taking place on `feature/v0.2-diagnostics`; do not merge to `main` until reviewed and tested.

## 8. Repository and build

Repository: https://github.com/miirfan84/EthernetNightGuard  
Working branch: https://github.com/miirfan84/EthernetNightGuard/tree/feature/v0.2-diagnostics  
GitHub Actions: https://github.com/miirfan84/EthernetNightGuard/actions

The workflow builds the app on GitHub's Windows runner and uploads an artifact named `EthernetNightGuard-win-x64`. Open the Actions run for the feature branch, wait for it to finish successfully, and download the artifact ZIP. Extract it and run `EthernetNightGuard.exe` on Windows.

The current build uses .NET 8 WPF and targets Windows x64. See `EthernetNightGuard.csproj` and `.github/workflows/build.yml` for the exact build configuration.

## 9. Recommended next steps

### Step A — Reduce package size
- Produce a framework-dependent build as a separate test artifact.
- Check whether a compatible .NET Desktop Runtime is installed on the target PC before relying on that build.
- Compare file size and startup behavior against the self-contained build.
- Test any trimming or other optimization before adopting it.

### Step B — Continue Windows-side investigation (no restart)
- Review the installed driver INF, advanced adapter properties, driver package documentation, and any documented vendor utility/API.
- Keep checks read-only; do not modify undocumented registry values.
- Record evidence and exact driver versions in the research notes.

### Step C — Optional Linux Live USB validation
A restart is only needed for this separate test because Linux must boot and load its own network driver to test the Linux LED interface. It is not required for ongoing Windows research. A Live USB can be used without installing Linux, but physical LED behavior and exposed interfaces still need to be confirmed on the exact laptop.

### Step D — Only implement a proven control method
If a documented, safe Windows control interface is found, implement it behind explicit checks, with clear error handling and a way to restore the prior state. If no supported Windows interface can be established, keep LED actions disabled and document that limitation.

## 10. Current project conclusion

Ethernet NightGuard v0.2 is a working **read-only adapter diagnostics prototype**. It correctly displayed the user's Realtek Ethernet adapter as connected at 1 Gbps. Linux driver evidence suggests RTL8168H LED control may be possible at the hardware/driver level, but the exact laptop's physical LED wiring and a safe Windows control method remain unverified.

The next milestones are to test a smaller distribution, continue Windows-side research without restarting the laptop, and only then decide whether a Linux Live USB test is necessary.
