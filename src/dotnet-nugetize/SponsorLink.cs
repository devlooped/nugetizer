using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Spectre.Console;

namespace NuGetize;

/// <summary>
/// The kind of SponsorLink diagnostic to report.
/// </summary>
public enum DiagnosticKind
{
    /// <summary>
    /// The SponsorLink GitHub is not installed on the user's personal account.
    /// </summary>
    AppNotInstalled,
    /// <summary>
    /// The user is not sponsoring the given sponsor account.
    /// </summary>
    UserNotSponsoring,
    /// <summary>
    /// The user has the SponsorLink GitHub app installed and is sponsoring the given sponsor account.
    /// </summary>
    Thanks
}

class SponsorLink
{
    const int DefaultMaxPause = 4000;
    const int DefaultQuietDays = 15;
    static TimeSpan NetworkTimeout { get; } = TimeSpan.FromSeconds(1);

    static readonly DateTime installTime = File.GetCreationTime(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName);
    static readonly Random rnd = new();

    readonly HttpClient http;
    readonly string sponsorable;
    readonly string product;
    readonly string packageId;
    readonly string version;
    readonly int pauseMin;
    readonly int pauseMax;
    readonly int quietDays;

    public static async Task<SponsorLink> CreateAsync(
        string sponsorable, string product,
        string? packageId = default,
        string? version = default,
        int pauseMin = 0,
        int pauseMax = DefaultMaxPause,
        int? quietDays = default)
    {
        var proxy = WebRequest.GetSystemWebProxy();
        var useProxy = !proxy.IsBypassed(new Uri("https://cdn.devlooped.com"));

        HttpMessageHandler handler;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework"))
        {
            // When running on Windows + .NET Framework, this guarantees proper proxy settings behavior automatically
            handler = new WinHttpHandler
            {
                ReceiveDataTimeout = NetworkTimeout,
                ReceiveHeadersTimeout = NetworkTimeout,
                SendTimeout = NetworkTimeout
            };
        }
        else if (useProxy)
        {
            handler = new HttpClientHandler
            {
                UseProxy = true,
                Proxy = proxy,
                DefaultProxyCredentials = CredentialCache.DefaultCredentials
            };
        }
        else
        {
            handler = new HttpClientHandler();
        }

        var http = new HttpClient(handler)
        {
            // Customize network timeout so we don't become unusable when target is 
            // unreachable (i.e. a proxy prevents access or misconfigured)
            Timeout = NetworkTimeout
        };

        if (quietDays == null)
        {
            try
            {
                // Reads settings from storage, best-effort
                var ini = await http.GetStringAsync("https://cdn.devlooped.com/sponsorlink/settings.ini");
                var values = ini
                    .Split(new[] { "\r\n", "\r" }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(x => x[0] != '#')
                    .Select(x => x.Split(new[] { '=' }, 2))
                    .ToDictionary(x => x[0].Trim(), x => x[1].Trim());

                if (values.TryGetValue("quiet", out var value) && int.TryParse(value, out var days))
                    quietDays = days;
                else
                    quietDays = DefaultQuietDays;
            }
            catch
            {
                quietDays = DefaultQuietDays;
            }
        }

        return new SponsorLink(http, sponsorable, product,
            packageId ?? product, version, pauseMin, pauseMax, quietDays.Value);
    }

    public SponsorLink(HttpClient http, string sponsorable, string product, string packageId, string? version, int pauseMin, int pauseMax, int quietDays)
        => (this.http, this.sponsorable, this.product, this.packageId, this.version, this.pauseMin, this.pauseMax, this.quietDays)
         = (http, sponsorable, product, packageId, version, pauseMin, pauseMax, quietDays);

    public async Task<DiagnosticKind?> CheckAsync(string projectDir)
    {
        // If there is no network at all, don't do anything.
        if (!NetworkInterface.GetIsNetworkAvailable())
            return default;

        var email = GetEmail(Path.GetDirectoryName(projectDir));
        // No email configured in git. Weird.
        if (string.IsNullOrEmpty(email))
            return default;

        var data = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(email));
        var hash = Base62.Encode(BigInteger.Abs(new BigInteger(data)));

        // Check app install and sponsoring status
        var installed = await CheckUrlAsync($"https://cdn.devlooped.com/sponsorlink/apps/{hash}?account={sponsorable}&product={product}&package={packageId}&version={version}", default);
        // Timeout, network error, proxy config issue, etc., exit quickly
        if (installed == null)
            return default;

        var sponsoring = await CheckUrlAsync($"https://cdn.devlooped.com/sponsorlink/{sponsorable}/{hash}?account={sponsorable}&product={product}&package={packageId}&version={version}", default);
        if (sponsoring == null)
            return default;

        var kind =
            installed == false ? DiagnosticKind.AppNotInstalled :
            sponsoring == false ? DiagnosticKind.UserNotSponsoring :
            DiagnosticKind.Thanks;

        var (warn, pause, suffix) = GetPause();
        if (!warn)
            return kind;

        //if (installed == false)
        //    AnsiConsole.Write(new Paragraph($":warning: {ThisAssembly.Strings.AppNotInstalled.Message(product, sponsorable, suffix)}", new Style(Color.Yellow, decoration: Decoration.Bold)));
        //else if (sponsoring == false)
        //    AnsiConsole.Write(new Paragraph($":warning: {ThisAssembly.Strings.UserNotSponsoring.Message(product, sponsorable, suffix)}", new Style(Color.Yellow, decoration: Decoration.Bold)));

        return kind;
    }

    static string? GetEmail(string workingDirectory)
    {
        try
        {
            var proc = Process.Start(new ProcessStartInfo("git", "config --get user.email")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = workingDirectory
            });
            proc.WaitForExit();

            // Couldn't run git config, so we can't check for sponsorship, no email to check.
            if (proc.ExitCode != 0)
                return null;

            return proc.StandardOutput.ReadToEnd().Trim();
        }
        catch
        {
            // Git not even installed.
        }

        return null;
    }

    (bool warn, int pause, string suffix) GetPause()
    {
        var daysOld = (int)DateTime.Now.Subtract(installTime).TotalDays;

        // Never warn during the quiet days.
        if (daysOld < quietDays)
            return (false, 0, "");

        if (quietDays == 0 && daysOld == 0)
            daysOld = 1;

        // At this point, daysOld is greater than quietDays and greater than 1.
        var nonQuietDays = daysOld - quietDays;
        // Turn days pause (starting at 1sec max pause into milliseconds, used for the pause.
        var daysMaxPause = nonQuietDays * 1000;

        // From second day, the max pause will increase from days old until the max pause.
        return GetPaused(rnd.Next(pauseMin, Math.Min(daysMaxPause, pauseMax)));
    }

    static (bool warn, int pause, string suffix) GetPaused(int pause)
        => (true, pause, pause > 0 ? $"Run paused for {pause}ms" : "");

    async Task<bool?> CheckUrlAsync(string url, CancellationToken cancellation)
    {
        try
        {
            // We perform a GET since that can be cached by the CDN, but HEAD cannot.
            var response = await http.GetAsync(url, cancellation);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return null;
        }
    }
}
