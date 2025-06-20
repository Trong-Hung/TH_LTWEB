using Microsoft.AspNetCore.Http;
using System.Text.Json;
using System.Text.Json.Serialization; // Thêm dòng này nếu chưa có

namespace VoTrongHung2280601119.Extensions // ĐẢM BẢO NAMESPACE NÀY ĐÚNG
{
    public static class SessionExtensions
    {
        public static void SetObjectAsJson(this ISession session, string key, object value)
        {
            session.SetString(key, JsonSerializer.Serialize(value));
        }

        public static T GetObjectFromJson<T>(this ISession session, string key)
        {
            var value = session.GetString(key);
            return value == null ? default(T) : JsonSerializer.Deserialize<T>(value);
        }
    }
}