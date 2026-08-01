using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;

namespace SharpProsperoCusaLocker;

internal static partial class Program
{
    private const int Port = 8080;
    private const uint LockMode = 0x16D;   // 0555 octal
    private const uint UnlockMode = 0x1FF; // 0777 octal

    private static readonly string[] DriveRoots =
    [
        string.Empty,
        "/mnt/ext0",
        "/mnt/ext1",
        "/mnt/usb0",
        "/mnt/usb1",
    ];

    private static readonly string[] ContentDirs =
    [
        "/user/app",
        "/user/addcont",
        "/user/patch",
        "/user/playgo",
    ];

    private static bool currentStatusLocked = true;

    private static void Main()
    {
        _ = setuid(0);
        _ = setgid(0);

        ApplyPermissions(LockMode, lockedState: true);
        RunHttpServer();
    }

    private static void RunHttpServer()
    {
        using TcpListener listener = new(IPAddress.Any, Port);
        listener.Start(backlog: 3);

        while (true)
        {
            using TcpClient client = listener.AcceptTcpClient();
            using NetworkStream stream = client.GetStream();

            byte[] requestBuffer = new byte[1024];
            int bytesRead = stream.Read(requestBuffer, 0, requestBuffer.Length);
            string request = Encoding.ASCII.GetString(requestBuffer, 0, bytesRead);

            if (request.Contains("GET /lock", StringComparison.Ordinal))
            {
                ApplyPermissions(LockMode, lockedState: true);
            }
            else if (request.Contains("GET /unlock", StringComparison.Ordinal))
            {
                ApplyPermissions(UnlockMode, lockedState: false);
            }

            byte[] response = BuildHttpResponse();
            stream.Write(response, 0, response.Length);
        }
    }

    private static void ApplyPermissions(uint mode, bool lockedState)
    {
        currentStatusLocked = lockedState;

        foreach (string driveRoot in DriveRoots)
        {
            foreach (string contentDir in ContentDirs)
            {
                string contentPath = driveRoot + contentDir;
                ApplyCusaPermissionsIn(contentPath, mode);
            }
        }
    }

    private static void ApplyCusaPermissionsIn(string contentPath, uint mode)
    {
        if (!Directory.Exists(contentPath))
        {
            return;
        }

        IEnumerable<string> cusaDirs;
        try
        {
            cusaDirs = Directory.EnumerateDirectories(contentPath, "CUSA*", SearchOption.TopDirectoryOnly).ToArray();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"enumerate failed for {contentPath}: {ex.Message}");
            return;
        }

        foreach (string cusaDir in cusaDirs)
        {
            ApplyDirectoryModeRecursive(cusaDir, mode);
        }
    }

    private static void ApplyDirectoryModeRecursive(string root, uint mode)
    {
        _ = chmod(root, mode);

        IEnumerable<string> children;
        try
        {
            children = Directory.EnumerateDirectories(root, "*", SearchOption.TopDirectoryOnly).ToArray();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Console.Error.WriteLine($"walk failed for {root}: {ex.Message}");
            return;
        }

        foreach (string child in children)
        {
            ApplyDirectoryModeRecursive(child, mode);
        }
    }

    private static byte[] BuildHttpResponse()
    {
        string statusText = currentStatusLocked
            ? "<span style='color: #2ecc71;'>CUSA fpkg folders locked (0555)</span>"
            : "<span style='color: #e74c3c;'>CUSA fpkg folders writable (0777)</span>";

        string body =
            "<html><head><meta charset='utf-8'><title>PS4 fpkg CUSA Locker</title>" +
            "<style>body{font-family:Arial;background:#1a1a1a;color:#fff;text-align:center;padding-top:50px;}" +
            "h1{color:#3498db;}.btn{display:inline-block;padding:15px 30px;font-size:18px;cursor:pointer;" +
            "text-decoration:none;color:#fff;border-radius:5px;margin:10px;border:none;}" +
            ".btn-lock{background:#2ecc71;}.btn-unlock{background:#e74c3c;}.status{font-size:22px;margin:20px;}" +
            "</style></head><body>" +
            "<h1>PS4 fpkg CUSA Folder Locker</h1>" +
            "<p>Locks PS4 fpkg CUSA folders on all configured drives so Rebuild Database GC cannot delete them.</p>" +
            "<p>Covers /user/app, /user/addcont, /user/patch, and /user/playgo. Save data is not touched.</p>" +
            $"<div class='status'>Status: {statusText}</div>" +
            "<br><a href='/lock'><button class='btn btn-lock'>Lock CUSA Folders</button></a>" +
            "<a href='/unlock'><button class='btn btn-unlock'>Unlock CUSA Folders</button></a>" +
            "</body></html>";

        byte[] bodyBytes = Encoding.UTF8.GetBytes(body);
        string header =
            "HTTP/1.1 200 OK\r\n" +
            "Content-Type: text/html; charset=utf-8\r\n" +
            $"Content-Length: {bodyBytes.Length}\r\n" +
            "Connection: close\r\n\r\n";

        return Encoding.ASCII.GetBytes(header).Concat(bodyBytes).ToArray();
    }

    [LibraryImport("c", SetLastError = true)]
    private static partial int chmod([MarshalAs(UnmanagedType.LPUTF8Str)] string path, uint mode);

    [LibraryImport("c", SetLastError = true)]
    private static partial int setuid(uint uid);

    [LibraryImport("c", SetLastError = true)]
    private static partial int setgid(uint gid);
}
