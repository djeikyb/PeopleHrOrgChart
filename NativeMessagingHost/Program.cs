using System.Buffers.Binary;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using OrgChart.Core;

namespace NativeMessagingHost;

public static class Program
{
    public static int Main(string[] args)
    {
        // foreach (var en in Enum.GetValues<Environment.SpecialFolder>()) Console.WriteLine($"{en}: {Environment.GetFolderPath(en)}");
        // return 0;

        // ~/Library/Application Support/Chromium/NativeMessagingHosts/com.my_company.my_application.json

        // FYI stderr goes to:
        // - firefox: tools > browser console

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

            var buf = new byte[1024 * 1024 * 1]; // HRM this could be a span
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
                    throw new Exception($"""
                                         Expected 4 bytes for int32 payload length
                                         but got {read} bytes
                                         """);
                }

                var payloadLength = BinaryPrimitives.ReadInt32LittleEndian(buf);
                if (payloadLength >= buf.Length)
                {
                    // oop too big
                    throw new Exception($"Exceeded max message size of {buf.Length}. Was sent {payloadLength}.");
                }

                // stdin must be buffered
                // initial trials w/ a 76k json payload..
                // first chunk was 65536 then..
                // oh wait. that's a key marker lol.
                // i bet the firefox postMessage has a byte[sizeof(ushort)] buffer
                // best not to rely on that specific size, but just keep reading..
                int payloadRead;
                for (payloadRead = 0; payloadRead < payloadLength;)
                {
                    if (ct.IsCancellationRequested) break;
                    var want = payloadLength - payloadRead;
                    read = stdin.Read(buf, payloadRead, want);
                    payloadRead += read;
                }

                PersonRoot? root;
                try
                {
                    if (payloadRead != payloadLength)
                    {
                        // oh no
                        throw new Exception($"""
                                             Expected {payloadLength} bytes
                                              but got {payloadRead} bytes
                                             """);
                    }

                    var span = buf.AsSpan(0, payloadRead);
                    var s = Encoding.UTF8.GetString(span);
                    File.WriteAllText("/Users/jacob/dev/me/active/ava/PeopleHrOrgChart/latest-received.txt", s);
                    root = JsonSerializer.Deserialize(span, PersonJsonContext.Default.PersonRoot);
                    if (root is not null)
                    {
                        using var db = Db.Open();
                        db.Save(root);
                    }
                }
                catch (Exception e)
                {
                    var span = buf.AsSpan(0, read);
                    var s = Encoding.UTF8.GetString(span);
                    File.WriteAllText("/Users/jacob/dev/me/active/ava/PeopleHrOrgChart/latest-error.txt", s);
                    root = null;
                    Console.Error.WriteLine(e);
                }

                if (root != null) nmw.Write("PONG");
                else nmw.Write("pong");

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
