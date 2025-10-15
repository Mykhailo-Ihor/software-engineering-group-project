using Duende.IdentityModel.OidcClient.Browser;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class SystemBrowser : IBrowser
{
    private readonly int _port;

    public SystemBrowser(int port = 0)
    {
        _port = port;
    }

    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
    {
        // Start local HTTP listener for callback
        using var listener = new HttpListener();

        // Get available port
        var port = _port != 0 ? _port : GetRandomUnusedPort();
        var redirectUri = $"http://localhost:{port}/callback";

        listener.Prefixes.Add($"http://localhost:{port}/");
        listener.Start();

        // Open system browser with auth URL
        OpenBrowser(options.StartUrl);

        // Wait for callback
        var context = await listener.GetContextAsync();

        // Extract response
        var response = context.Request.Url.ToString();

        // Send response to browser
        var responseString = "<html><body>You can close this window.</body></html>";
        var buffer = Encoding.UTF8.GetBytes(responseString);
        context.Response.ContentLength64 = buffer.Length;
        context.Response.OutputStream.Write(buffer, 0, buffer.Length);
        context.Response.OutputStream.Close();

        listener.Stop();

        return new BrowserResult
        {
            Response = response,
            ResultType = BrowserResultType.Success
        };
    }

    private void OpenBrowser(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch
        {
            // Hack for different OS
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
        }
    }

    private int GetRandomUnusedPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }
}