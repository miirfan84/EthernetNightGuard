using System;
using System.Diagnostics;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace EthernetNightGuard;

public partial class MainWindow : Window
{
    private bool _refreshInProgress;

    public MainWindow()
    {
        InitializeComponent();
        Loaded += async (_, _) => await RefreshAdapterStatusAsync();
    }

    private async void Refresh_Click(object sender, RoutedEventArgs e)
    {
        await RefreshAdapterStatusAsync();
    }

    private async Task RefreshAdapterStatusAsync()
    {
        if (_refreshInProgress)
            return;

        _refreshInProgress = true;
        ConnectionStatus.Text = "Checking adapters…";
        AdapterDetails.Text = "Running read-only Windows network diagnostics.";
        try
        {
            var result = await Task.Run(QueryAdapter);
            if (result.TimedOut)
            {
                ConnectionStatus.Text = "Query timed out";
                AdapterDetails.Text = "Windows did not return adapter information within 5 seconds. Nothing was changed; try Refresh again.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(result.Error))
            {
                ConnectionStatus.Text = "Status unavailable";
                AdapterDetails.Text = result.Error;
                return;
            }

            if (string.IsNullOrWhiteSpace(result.Output))
            {
                ConnectionStatus.Text = "No Ethernet adapter found";
                AdapterDetails.Text = "No physical adapter matching Ethernet, Realtek or GbE was found. Check Windows Network Connections.";
                return;
            }

            using var document = JsonDocument.Parse(result.Output);
            var root = document.RootElement;
            // ConvertTo-Json returns an object for one result and an array for multiple results.
            var adapter = root.ValueKind == JsonValueKind.Array ? root[0] : root;
            var name = ReadString(adapter, "Name", "Unknown adapter");
            var description = ReadString(adapter, "InterfaceDescription", "Description unavailable");
            var status = ReadString(adapter, "Status", "Unknown");
            var speed = ReadString(adapter, "LinkSpeed", "Unknown");

            ConnectionStatus.Text = status.Equals("Up", StringComparison.OrdinalIgnoreCase)
                ? "Connected"
                : status;
            AdapterDetails.Text = $"{name} · {description}\nLink speed: {speed}";
        }
        catch (JsonException)
        {
            ConnectionStatus.Text = "Unexpected query result";
            AdapterDetails.Text = "Windows returned data in an unexpected format. No network settings were changed.";
        }
        catch (Exception ex)
        {
            ConnectionStatus.Text = "Status unavailable";
            AdapterDetails.Text = $"{ex.GetType().Name}: {ex.Message}";
        }
        finally
        {
            _refreshInProgress = false;
        }
    }

    private static string ReadString(JsonElement element, string propertyName, string fallback)
    {
        return element.TryGetProperty(propertyName, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? fallback
            : fallback;
    }

    private static QueryResult QueryAdapter()
    {
        const string command = "Get-NetAdapter -Physical -ErrorAction SilentlyContinue | Where-Object { $_.InterfaceDescription -match 'Realtek|Ethernet|GbE' -or $_.Name -match 'Ethernet' } | Select-Object -First 1 Name, InterfaceDescription, Status, LinkSpeed | ConvertTo-Json -Compress";
        var start = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoLogo -NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"{command}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = start };
        if (!process.Start())
            return new QueryResult("", "Could not start Windows PowerShell.", false);

        // Drain both redirected streams concurrently so a full pipe cannot hang the process.
        var stdoutTask = process.StandardOutput.ReadToEndAsync();
        var stderrTask = process.StandardError.ReadToEndAsync();

        try
        {
            if (!process.WaitForExit(5000))
            {
                try { process.Kill(entireProcessTree: true); } catch { /* Best-effort cleanup. */ }
                return new QueryResult("", "", true);
            }
        }
        catch (Exception ex)
        {
            try { process.Kill(entireProcessTree: true); } catch { /* Best-effort cleanup. */ }
            return new QueryResult("", ex.Message, false);
        }

        Task.WaitAll(stdoutTask, stderrTask);
        var output = stdoutTask.Result.Trim();
        var error = stderrTask.Result.Trim();
        if (process.ExitCode != 0 && string.IsNullOrWhiteSpace(output))
            return new QueryResult("", string.IsNullOrWhiteSpace(error) ? "Adapter query failed." : error, false);

        return new QueryResult(output, "", false);
    }

    private sealed record QueryResult(string Output, string Error, bool TimedOut);
}
