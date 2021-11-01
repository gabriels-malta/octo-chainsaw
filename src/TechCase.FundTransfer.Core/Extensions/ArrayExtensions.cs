using System.Text;
using System.Text.Json;

namespace System
{
    public static class ArrayExtensions
    {
        public static TResult To<TResult>(this byte[] byteArray)
        {
            var eventReceived = Encoding.UTF8.GetString(byteArray);
            return JsonSerializer.Deserialize<TResult>(eventReceived);
        }
    }
}
