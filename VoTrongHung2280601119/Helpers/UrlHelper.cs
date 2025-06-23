using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace VoTrongHung2280601119.Helpers // Chỉ một khai báo namespace
{
    // Chỉ một khai báo class UrlHelper, và nó nên là static nếu GenerateSlug là static
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

            // Thay thế khoảng trắng bằng dấu gạch ngang (chỉ một lần thay thế)
            // LƯU Ý: Regex.Replace(title, @"\s+", "-").Trim() thường hiệu quả hơn
            // để xử lý nhiều khoảng trắng liên tiếp và khoảng trắng ở đầu/cuối
            title = Regex.Replace(title, @"\s+", " ").Trim(); // Xóa nhiều khoảng trắng thành một, trim đầu cuối
            title = Regex.Replace(title, @"\s", "-");          // Thay khoảng trắng thành dấu gạch ngang

            // Có thể thêm bước này để loại bỏ các dấu gạch ngang liên tiếp nếu có
            title = Regex.Replace(title, @"-+", "-");

            return title;
        }
    }
}