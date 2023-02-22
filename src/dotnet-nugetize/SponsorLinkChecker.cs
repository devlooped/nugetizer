using System;
using System.Diagnostics;
using System.IO;
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

namespace Devlooped;

class SponsorLink
{
    /// <summary>
    /// Settings to configure the sponsor link checks.
    /// </summary>
    public class Settings
    {
        public Settings(string sponsorable, string product, string? packageId = default, string? version = default)
        {
            Sponsorable = sponsorable;
            Product = product;
            PackageId = packageId ?? product;
            if (version != null)
                Version = version;
        }

        public string Sponsorable { get; }

        public string Product { get; }

        public string PackageId { get; }

        public string Version { get; set; } = Assembly.GetExecutingAssembly().GetName().Version.ToString();

        /// <summary>
        /// Timeout for network requests (default: 1 second).
        /// </summary>
        public TimeSpan NetworkTimeout { get; set; } = TimeSpan.FromSeconds(1);

        ///// <summary>
        ///// Installation date for the consuming product (default: executing assembly creation date).
        ///// </summary>
        //public DateTime InstallDate { get; set; } = File.GetCreationTime(Assembly.GetExecutingAssembly().ManifestModule.FullyQualifiedName);
    }


    /// <summary>
    /// The current sponsorship status.
    /// </summary>
    public enum Status
    {
        /// <summary>
        /// The SponsorLink GitHub is not installed on the user's personal account.
        /// </summary>
        AppMissing,
        /// <summary>
        /// The user is not sponsoring the given sponsor account.
        /// </summary>
        NotSponsoring,
        /// <summary>
        /// The user has the SponsorLink GitHub app installed and is sponsoring the given sponsor account.
        /// </summary>
        Sponsoring
    }

    static readonly Random rnd = new();

    readonly HttpClient http;
    readonly Settings settings;

    public static SponsorLink Create(
        string sponsorable, string product,
        string? packageId = default,
        string? version = default)
        => Create(new Settings(sponsorable, product, packageId, version));

    public static SponsorLink Create(Settings settings)
    {
        var proxy = WebRequest.GetSystemWebProxy();
        var useProxy = !proxy.IsBypassed(new Uri("https://cdn.devlooped.com"));

        HttpMessageHandler handler;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && RuntimeInformation.FrameworkDescription.StartsWith(".NET Framework"))
        {
            // When running on Windows + .NET Framework, this guarantees proper proxy settings behavior automatically
            handler = new WinHttpHandler
            {
                ReceiveDataTimeout = settings.NetworkTimeout,
                ReceiveHeadersTimeout = settings.NetworkTimeout,
                SendTimeout = settings.NetworkTimeout
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
            Timeout = settings.NetworkTimeout
        };

        return new SponsorLink(http, settings);
    }

    public SponsorLink(HttpClient http, Settings settings)
        => (this.http, this.settings)
         = (http, settings);

    public async Task<Status?> CheckAsync(string projectDir)
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
        var installed = await CheckUrlAsync($"https://cdn.devlooped.com/sponsorlink/apps/{hash}?account={settings.Sponsorable}&product={settings.Product}&package={settings.PackageId}&version={settings.Version}", default);
        // Timeout, network error, proxy config issue, etc., exit quickly
        if (installed == null)
            return default;

        var sponsoring = await CheckUrlAsync($"https://cdn.devlooped.com/sponsorlink/{settings.Sponsorable}/{hash}?account={settings.Sponsorable}&product={settings.Product}&package={settings.PackageId}&version={settings.Version}", default);
        if (sponsoring == null)
            return default;

        var kind =
            installed == false ? Status.AppMissing :
            sponsoring == false ? Status.NotSponsoring :
            Status.Sponsoring;

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
