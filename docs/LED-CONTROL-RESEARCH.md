# Ethernet LED-control research: RTL8168H

**Status:** Linux-side LED support is evidenced; a supported Windows control interface for the installed driver has not been identified.

## Hardware evidence

The supplied Windows diagnostics identify a Realtek PCIe GbE Family Controller with PCI ID `PCI\VEN_10EC&DEV_8168&SUBSYS_8DEA103C&REV_15`. The installed Windows driver did not expose an LED-related Advanced property in the checks performed for this project.

The Linux kernel's `r8169` driver has explicit LED support for RTL8168/RTL8101 variants. The upstream patch says the implementation was tested on RTL8168H and exposes control through the Linux network-device LED trigger. The driver source defines the RTL8168 LED control register at offset `0x18`. This is evidence that a supported driver can manage LEDs on at least the tested RTL8168H hardware; it is **not** a safe recipe for writing the register directly from Windows.

Sources:
- Linux kernel mailing-list patch: https://lists.openwall.net/netdev/2023/12/16/141
- Linux r8169 driver source: https://github.com/gregkh/linux/blob/master/drivers/net/ethernet/realtek/r8169_main.c
- Realtek's controller family download page, which lists RTL8168H among supported controller families: https://www.realtek.com/Download/List?cate_id=584NetworkInterfaceControllers

## What this proves—and what it does not

- **Supported by public evidence:** Linux `r8169` can expose LED controls for RTL8168H; the patch author reports testing on RTL8168H.
- **Not established:** that the exact HP system's physical LEDs are all wired to controllable outputs; the patch explicitly notes that the driver cannot know which LEDs are physically wired.
- **Not established:** that the installed Windows driver has a documented or supported equivalent API, registry setting, or command-line switch.
- **Not safe to infer:** that directly writing PCI/MMIO/PHY registers or using undocumented registry values is appropriate. NightGuard must not do that.

## Recommended next validation

1. Keep using the laptop normally; do not change its network driver or registry for this test.
2. When a reboot is convenient, boot a Linux live USB (without installing Linux) and identify the adapter with `lspci -nnk`. Confirm that the active driver is `r8169`.
3. Inspect whether the kernel exposes LED devices for the Ethernet interface under `/sys/class/leds/` and whether a network LED trigger is available. Read the available trigger/brightness values first; do not write to them until the user explicitly chooses to test.
4. If the expected LED controls are exposed, a separate, reversible Linux-only test can establish whether the laptop's physical Ethernet LEDs respond. This would validate hardware capability, **not** provide a Windows control path.
5. Continue searching for a documented Windows vendor interface. Until one is confirmed, keep Day/Night actions disabled.

## Product decision

NightGuard v0.2 remains a read-only Windows diagnostics app. No arbitrary hardware-register writes, EEPROM/eFuse programming, adapter disablement, or speculative registry edits will be added.
