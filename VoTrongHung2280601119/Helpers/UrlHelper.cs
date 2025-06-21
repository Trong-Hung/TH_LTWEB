using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace VoTrongHung2280601119.Helpers
{
    public static class UrlHelper
    {
        public static string GenerateSlug(string title)
        {
            if (string.IsNullOrEmpty(title)) return "";

            // Chuyển chuỗi về chữ thường
            title = title.ToLowerInvariant();

            // Thay thế chữ 'đ' thành 'd'
            title = title.Replace('đ', 'd');

            // Bỏ dấu tiếng Việt bằng phương pháp chuẩn (Normalize)
            var normalizedString = title.Normalize(NormalizationForm.FormD);
            var stringBuilder = new StringBuilder();

            foreach (var c in normalizedString)
            {
                var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                {
                    stringBuilder.Append(c);
                }
            }

            // Gán lại chuỗi đã bỏ dấu
            title = stringBuilder.ToString().Normalize(NormalizationForm.FormC);

            // Xóa các ký tự đặc biệt không mong muốn
            title = Regex.Replace(title, @"[^a-z0-9\s-]", "");

            // Thay thế khoảng trắng bằng dấu gạch ngang
            title = Regex.Replace(title, @"\s+", " ").Trim();
            title = Regex.Replace(title, @"\s", "-");

            return title;
        }
    }
}