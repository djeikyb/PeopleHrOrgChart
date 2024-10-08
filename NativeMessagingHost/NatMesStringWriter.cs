using System.Buffers.Binary;
using System.Text.Json;

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

        ReadOnlySpan<byte> message = ms.ToArray();

        // HRM could this replace the above?
        // var jet = JsonEncodedText.Encode(s);
        // var message = jet.EncodedUtf8Bytes;

        var lbuf = new byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(lbuf, message.Length);
        stdout.Write(lbuf);
        stdout.Write(message);
    }
}
