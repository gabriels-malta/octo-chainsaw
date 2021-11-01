using System.Text;
using System.Text.Json;

namespace System.IO
{
    public static class StreamExtensions
    {
        public static string ToEncodedString(this Stream stream)
        {
            byte[] bytes = new byte[stream.Length];
            stream.Position = 0;
            stream.Read(bytes, 0, (int)stream.Length);
            return Encoding.UTF8.GetString(bytes);
        }

        public static TResult To<TResult>(this Stream stream) where TResult: class
        {
            var encoded = stream.ToEncodedString();
            return JsonSerializer.Deserialize<TResult>(encoded);
        }
    }
}
