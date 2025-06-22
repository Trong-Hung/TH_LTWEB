using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using VoTrongHung2280601119.Models;
using VoTrongHung2280601119.Repositories;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using QuestPDF.Helpers;

namespace VoTrongHung_2280601119.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.Role_Admin)]
    public class OrderController : Controller
    {
        private readonly IOrderDistributionRepository _orderDistributionRepository;
        private readonly ApplicationDbContext _context;


        public OrderController(IOrderDistributionRepository orderDistributionRepository, ApplicationDbContext context)
        {
            _orderDistributionRepository = orderDistributionRepository;
            _context = context;
        }

        // GET: Admin/Order (Danh sách đơn hàng)
        public async Task<IActionResult> Index()
        {
            var orders = await _orderDistributionRepository.GetAllAsync();
            return View(orders);
        }
            
        // POST: Admin/Order/Confirm/5 (Xác nhận đơn hàng)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id)
        {
            var order = await _orderDistributionRepository.GetByIdAsync(id);
            if (order == null) { return NotFound(); }

            order.Status = "Đã Xác nhận"; // Cập nhật trạng thái
            await _orderDistributionRepository.UpdateAsync(order);
            // TODO: Cập nhật tồn kho sản phẩm nếu cần thiết khi xác nhận đơn hàng
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Order/Cancel/5 (Hủy đơn hàng)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var order = await _orderDistributionRepository.GetByIdAsync(id);
            if (order == null) { return NotFound(); }

            order.Status = "Đã Hủy"; // Cập nhật trạng thái
            await _orderDistributionRepository.UpdateAsync(order);
            // TODO: Hoàn lại tồn kho sản phẩm nếu đã trừ khi đặt hàng
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> PrintInvoice(int orderId)
        {
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            var order = await _context.OrderDistributions
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .Include(o => o.Customer)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) return NotFound();

            var totalAmount = order.OrderItems.Sum(item => item.Quantity * item.Price);

            var pdfStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(2, Unit.Centimetre);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Arial"));

                    page.Header()
                        .PaddingBottom(10)
                        .Row(row =>
                        {

                            row.ConstantColumn(150)
                                .AlignRight()
                                .Text($"HÓA ĐƠN BÁN HÀNG\nMã đơn: {order.Id}")
                                .Bold()
                                .FontSize(14)
                                .FontColor(Colors.Blue.Darken1);
                        });

                    page.Content()
                        .PaddingVertical(10)
                        .Stack(stack =>
                        {
                            // Thông tin khách hàng
                            stack.Item().Text("Thông tin khách hàng").Bold().Underline().FontSize(14);
                            stack.Item().Text($"Họ tên: {order.Customer?.FullName ?? "N/A"}");
                            stack.Item().Text($"Email: {order.Customer?.Email ?? "N/A"}");
                            stack.Item().Text($"Địa chỉ giao hàng: {order.ShippingAddress}");
                            stack.Item().Text($"Ngày đặt: {order.OrderDate:dd/MM/yyyy HH:mm}");
                            if (!string.IsNullOrEmpty(order.Notes))
                            {
                                stack.Item().Text($"Ghi chú: {order.Notes}");
                            }

                            stack.Item().PaddingTop(10).Element(container =>
                            {
                                container.Table(table =>
                                {
                                    // Định nghĩa cột
                                    table.ColumnsDefinition(columns =>
                                    {
                                        columns.RelativeColumn(3); // Mã SP
                                        columns.RelativeColumn(6); // Tên SP
                                        columns.RelativeColumn(2); // Số lượng
                                        columns.RelativeColumn(3); // Đơn giá
                                        columns.RelativeColumn(3); // Thành tiền
                                    });

                                    // Header bảng
                                    table.Header(header =>
                                    {
                                        header.Cell().Background(Colors.Grey.Lighten3).Text("Mã SP").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten3).Text("Tên sản phẩm").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten3).AlignCenter().Text("Số lượng").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten3).AlignRight().Text("Đơn giá (VND)").Bold();
                                        header.Cell().Background(Colors.Grey.Lighten3).AlignRight().Text("Thành tiền (VND)").Bold();
                                    });

                                    // Dữ liệu sản phẩm
                                    foreach (var item in order.OrderItems)
                                    {
                                        table.Cell().Text(item.Product?.Code ?? "");
                                        table.Cell().Text(item.Product?.Name ?? "");
                                        table.Cell().AlignCenter().Text(item.Quantity.ToString());
                                        table.Cell().AlignRight().Text($"{item.Price:#,##0}");
                                        table.Cell().AlignRight().Text($"{item.Quantity * item.Price:#,##0}");
                                    }

                                    // Footer tổng tiền
                                    table.Footer(footer =>
                                    {
                                        footer.Cell().ColumnSpan(4).AlignRight().Text("Tổng tiền:").Bold();
                                        footer.Cell().AlignRight().Text($"{totalAmount:#,##0}").Bold();
                                    });
                                });
                            });
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text(txt =>
                        {
                            txt.Span("Cảm ơn quý khách!").SemiBold();
                            txt.Line("\n");
                            txt.Span("Trang ");
                            txt.CurrentPageNumber();
                            txt.Span(" / ");
                            txt.TotalPages();
                        });
                });
            }).GeneratePdf(pdfStream);

            pdfStream.Position = 0;

            return File(pdfStream.ToArray(), "application/pdf", $"HoaDon_{order.Id}.pdf");
        }


    }
}