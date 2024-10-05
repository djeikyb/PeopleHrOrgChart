using System.Buffers.Binary;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace NativeMessagingHost;

public class NatMesStringWriter(Stream stdout)
{
    /// Mind it's a very simple string, or already sufficiently escaped for json.
    public void Write(string s)
    {
        var ms = new MemoryStream();
        using (var wr = new Utf8JsonWriter(ms))
        {
            wr.WriteRawValue("\"" + s + "\"");
        }

        var message = ms.ToArray();
        var lbuf = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(lbuf, message.Length);
        stdout.Write(lbuf);
        stdout.Write(message);
    }
}

public static class Program
{
    public static int Main(string[] args)
    {
        // foreach (var en in Enum.GetValues<Environment.SpecialFolder>()) Console.WriteLine($"{en}: {Environment.GetFolderPath(en)}");
        // return 0;

        // ~/Library/Application Support/Chromium/NativeMessagingHosts/com.my_company.my_application.json

        Console.Error.WriteLine("starting program");

        var cts = new CancellationTokenSource();
        var ct = cts.Token;

        Console.Error.WriteLine("adding sigint");

        void Handler(PosixSignalContext context)
        {
            Console.Error.WriteLine($"caught {context.Signal}");
            context.Cancel = true;
            cts.Cancel();
            Console.Error.WriteLine("Requested cancel.");
        }

        Console.Error.WriteLine("adding sigint");
        using var sigint = PosixSignalRegistration.Create(PosixSignal.SIGINT, Handler);
        Console.Error.WriteLine("adding sigterm");
        using var sigterm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, Handler);

        var stdin = Console.OpenStandardInput();
        var stdout = Console.OpenStandardOutput();

        var nmw = new NatMesStringWriter(stdout);
        nmw.Write("connected!");

        try
        {
            // while (!Debugger.IsAttached)
            // {
            //     if (ct.IsCancellationRequested) return 0;
            //     Thread.Sleep(100);
            // }

            var buf = new byte[1024 * 5];
            while (!ct.IsCancellationRequested)
            {
                // UnixConsoleStream has a blocking read
                // it'll wait until.. well, not sure when?
                // i observe it waiting for _some_ input
                // ideally, it'd wait to fill the buffer?
                var read = stdin.Read(buf, 0, 4);
                if (read != 4)
                {
                    // The protocol is, first you get an int32 length.
                    // If we didn't get this, abandon ship.
                    return -1;
                }

                // throw new Exception("oops");

                var len = BinaryPrimitives.ReadInt32LittleEndian(buf);
                if (len >= buf.Length)
                {
                    // oop too big
                    throw new Exception($"Exceeded max message size of {buf.Length}. Was sent {len}.");
                }

                read = stdin.Read(buf, 0, len);
                var s = Encoding.UTF8.GetString(buf.AsSpan(0, read));

                nmw.Write("pong");
                // var pong = "\"pong\""u8;
                // BinaryPrimitives.WriteInt32LittleEndian(buf, pong.Length);
                // stdout.Write(buf.AsSpan().Slice(0, 4));
                // stdout.Write(pong);
            }
        }
        catch (OperationCanceledException)
        {
            // gracefully exit
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            // nmw.Write(e.ToString());
            return -1;
        }

        return 0;

        // var userAppDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        // if (string.Empty.Equals(userAppDataDir)) throw new Exception("Base app data folder does not exist.");
        //
        // var di = new DirectoryInfo(userAppDataDir).CreateSubdirectory("merviche.orgchart");
        // var socketFilePath = Path.Combine(di.FullName, "nativemessaging.sock");
        // // var f = File.Create(socketFilePath);
        // var uds = new UnixDomainSocketEndPoint(socketFilePath);
        // using var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
        // socket.Connect(uds);
        //
        //
        // Span<byte> sizeBuf = stackalloc byte[4]; // size of int32
        // while (!ct.IsCancellationRequested)
        // {
        //     try
        //     {
        //         stdin.ReadExactly(sizeBuf); // HRM what if there wasn't enough.. _should_ it crash?
        //         var len = BinaryPrimitives.ReadInt32LittleEndian(sizeBuf); // TODO check BitConverter.IsLittleEndian
        //
        //         var tenMiB = 1024 * 1024 * 10;
        //         if (len > tenMiB)
        //         {
        //             Console.Error.WriteLine("lol what're ya doin");
        //             continue;
        //         }
        //
        //         Span<byte> data = new byte[len];
        //         stdin.ReadExactly(data); // HRM what if there wasn't enough.. _should_ it crash?
        //
        //         socket.Send(sizeBuf);
        //         socket.Send(data);
        //
        //         Console.Write("pong");
        //     }
        //     catch (Exception e)
        //     {
        //         var s = e.ToString();
        //         Console.WriteLine(s);
        //         socket.Send(Encoding.UTF8.GetBytes(s));
        //         return -1;
        //     }
        // }
        //
        // return 0;
    }
}
