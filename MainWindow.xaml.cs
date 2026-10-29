using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace EthernetNightGuard;

public partial class MainWindow : Window
{
    private const string AdapterName = "Ethernet 18";

    public MainWindow()
    {
        InitializeComponent();
        RefreshAdapterStatus();
    }

    private void Refresh_Click(object sender, RoutedEventArgs e) => RefreshAdapterStatus();

    private void RefreshAdapterStatus()
    {
        try
        {
            // Read-only adapter discovery through PowerShell. This does not alter network settings.
            var start = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-NoProfile -NonInteractive -Command \"Get-NetAdapter -Name 'Ethernet 18' -ErrorAction SilentlyContinue | Select-Object -First 1 -ExpandProperty Status\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(start);
            if (process is null)
            {
                ConnectionStatus.Text = "Status unavailable";
                AdapterDetails.Text = "Could not start the Windows adapter query.";
                return;
            }

            string output = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit(3000);

            if (output.Equals("Up", StringComparison.OrdinalIgnoreCase))
            {
                ConnectionStatus.Text = "Connected";
                AdapterDetails.Text = "Realtek PCIe GbE Family Controller · Ethernet 18";
            }
            else if (!string.IsNullOrWhiteSpace(output))
            {
                ConnectionStatus.Text = output;
                AdapterDetails.Text = "Realtek PCIe GbE Family Controller · Ethernet 18";
            }
            else
            {
                ConnectionStatus.Text = "Adapter not found";
                AdapterDetails.Text = "Expected adapter alias: Ethernet 18. Refresh after checking Windows adapter names.";
            }
        }
        catch (Exception ex)
        {
            ConnectionStatus.Text = "Status unavailable";
            AdapterDetails.Text = ex.Message;
        }
    }
}
