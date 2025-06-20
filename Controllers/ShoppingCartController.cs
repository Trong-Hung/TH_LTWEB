using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http; // Cần cho IQueryCollection
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Cần nếu dùng _context trực tiếp (đã thay bằng repo)
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VoTrongHung2280601119.Extensions;
using VoTrongHung2280601119.Models;
using VoTrongHung2280601119.Repositories;

namespace VoTrongHung2280601119.Controllers
{
    public class ShoppingCartController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly IWarehouseRepository _warehouseRepository;
        private readonly IOrderDistributionRepository _orderDistributionRepository;
        private readonly IOrderItemRepository _orderItemRepository;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMomoService _momoService; // THÊM: Inject IMomoService

        public ShoppingCartController(IProductRepository productRepository, IWarehouseRepository warehouseRepository,
                                      IOrderDistributionRepository orderDistributionRepository, IOrderItemRepository orderItemRepository,
                                      UserManager<ApplicationUser> userManager, IMomoService momoService) // THÊM IMomoService vào constructor
        {
            _productRepository = productRepository;
            _warehouseRepository = warehouseRepository;
            _orderDistributionRepository = orderDistributionRepository;
            _orderItemRepository = orderItemRepository;
            _userManager = userManager;
            _momoService = momoService; // GÁN
        }

        // GET: /ShoppingCart/Index (Hiển thị giỏ hàng)
        public IActionResult Index()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            return View(cart);
        }

        // POST: /ShoppingCart/AddToCart (Thêm sản phẩm vào giỏ hàng)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddToCart(int productId, int quantity, int? selectedWarehouseId)
        {
            if (quantity <= 0)
            {
                TempData["Error"] = "Số lượng sản phẩm phải lớn hơn 0.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            var product = await _productRepository.GetByIdAsync(productId);
            if (product == null)
            {
                TempData["Error"] = "Sản phẩm không tìm thấy.";
                return RedirectToAction("Index", "Home");
            }

            if (product.Stock < quantity)
            {
                TempData["Error"] = $"Số lượng tồn kho của {product.Name} không đủ. Chỉ còn {product.Stock} sản phẩm.";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            string warehouseName = "Không xác định";
            if (selectedWarehouseId.HasValue && selectedWarehouseId.Value != 0)
            {
                var warehouse = await _warehouseRepository.GetByIdAsync(selectedWarehouseId.Value);
                if (warehouse == null)
                {
                    TempData["Error"] = "Kho đã chọn không tìm thấy.";
                    return RedirectToAction("Details", "Product", new { id = productId });
                }
                warehouseName = warehouse.Name;
            }

            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();

            var newItem = new CartItem
            {
                ProductId = productId,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = quantity,
                ImageUrl = product.ImageUrl,
                WarehouseId = selectedWarehouseId ?? 0,
                WarehouseName = warehouseName
            };

            cart.AddItem(newItem);
            HttpContext.Session.SetObjectAsJson("Cart", cart);

            TempData["Success"] = $"Đã thêm {quantity} sản phẩm {product.Name} vào giỏ hàng.";
            return RedirectToAction(nameof(Index));
        }

        // POST: /ShoppingCart/RemoveFromCart (Xóa sản phẩm khỏi giỏ hàng)
        [HttpPost]
        public IActionResult RemoveFromCart(int productId, int warehouseId)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart != null)
            {
                cart.RemoveItem(productId, warehouseId);
                HttpContext.Session.SetObjectAsJson("Cart", cart);
                TempData["Success"] = "Sản phẩm đã được xóa khỏi giỏ hàng.";
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: /ShoppingCart/Checkout (Hiển thị form đặt hàng)
        [Authorize]
        public async Task<IActionResult> Checkout()
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart") ?? new ShoppingCart();
            if (!cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.GetUserAsync(User);
            var order = new OrderDistribution
            {
                CustomerId = user?.Id,
                ShippingAddress = user?.Email // Hoặc user?.Address nếu có trường Address
            };

            ViewBag.CartTotal = cart.CalculateTotal();
            return View(order);
        }

        // POST: /ShoppingCart/Checkout (Xử lý đặt hàng - LỰA CHỌN THANH TOÁN KHÁC MOMO)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Checkout(OrderDistribution order)
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để đặt hàng.";
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            order.CustomerId = user.Id;
            order.OrderDate = DateTime.UtcNow;
            order.Status = "Chờ Xác nhận";
            order.TotalPrice = cart.CalculateTotal();

            // Loại bỏ validate cho các trường được gán thủ công hoặc tính toán
            ModelState.Remove("OrderItems"); // Không validate OrderItems từ form
            ModelState.Remove("OrderDate");
            ModelState.Remove("Status");
            ModelState.Remove("TotalPrice");
            ModelState.Remove("Customer"); // Không validate Navigation Property Customer

            if (ModelState.IsValid)
            {
                // Kiểm tra tồn kho lần cuối
                foreach (var item in cart.Items)
                {
                    var productInDb = await _productRepository.GetByIdAsync(item.ProductId);
                    if (productInDb == null || productInDb.Stock < item.Quantity)
                    {
                        TempData["Error"] = $"Sản phẩm '{item.ProductName}' không đủ tồn kho. Chỉ còn {(productInDb?.Stock ?? 0)} sản phẩm.";
                        ViewBag.CartTotal = cart.CalculateTotal();
                        return View(order);
                    }
                }

                // Tạo OrderItems từ CartItems
                order.OrderItems = new List<OrderItem>();
                foreach (var item in cart.Items)
                {
                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        WarehouseId = item.WarehouseId
                    });

                    // Trừ tồn kho
                    var productToUpdate = await _productRepository.GetByIdAsync(item.ProductId);
                    if (productToUpdate != null)
                    {
                        productToUpdate.Stock -= item.Quantity;
                        await _productRepository.UpdateAsync(productToUpdate);
                    }
                }

                await _orderDistributionRepository.AddAsync(order);

                cart.Clear();
                HttpContext.Session.SetObjectAsJson("Cart", cart);

                TempData["Success"] = $"Đơn hàng {order.Id} của bạn đã được đặt thành công!";
                return RedirectToAction("OrderConfirmation", new { id = order.Id });
            }

            ViewBag.CartTotal = cart.CalculateTotal();
            return View(order);
        }

        // POST: /ShoppingCart/CreateMomoPayment (Action để tạo yêu cầu thanh toán Momo)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateMomoPayment(OrderDistribution order) // Nhận thông tin order từ form
        {
            var cart = HttpContext.Session.GetObjectFromJson<ShoppingCart>("Cart");
            if (cart == null || !cart.Items.Any())
            {
                TempData["Error"] = "Giỏ hàng của bạn đang trống.";
                return RedirectToAction("Index", "Home");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                TempData["Error"] = "Vui lòng đăng nhập để đặt hàng.";
                return RedirectToPage("/Account/Login", new { area = "Identity" });
            }

            // Gán thông tin Order từ Controller (không lấy từ form OrderDistribution đầy đủ)
            // Hoặc bạn có thể tạo một OrderDistribution tạm thời để lấy TotalPrice và OrderItems cho Momo
            // Logic này cần được đồng bộ với Checkout POST bên trên
            // Để đơn giản, giả sử bạn đã có OrderDistribution ở đâu đó hoặc tạo mới

            var orderInfo = new OrderInfoModel // OrderInfoModel để truyền cho Momo
            {
                FullName = user.FullName,
                Amount = (long)cart.CalculateTotal(), // Momo dùng Long, không có số thập phân
                OrderInfo = $"Thanh toán đơn hàng {user.Email} tổng tiền {cart.CalculateTotal()}",
                // OrderId sẽ được tạo trong MomoService.CreatePaymentAsync
            };

            var momoResponse = await _momoService.CreatePaymentAsync(orderInfo);

            if (momoResponse != null && momoResponse.PayUrl != null)
            {
                // TODO: Lưu order vào DB với trạng thái "Pending" trước khi chuyển hướng
                // Bạn cần lưu order vào DB ở đây với trạng thái "Đang chờ thanh toán Momo"
                // Và cập nhật nó trong MomoPaymentCallBack.
                // Để đơn giản hóa, chúng ta bỏ qua việc lưu vào DB ở đây và chỉ chuyển hướng.
                // Nếu muốn lưu, bạn cần truyền đủ thông tin order vào đây từ giỏ hàng.

                return Redirect(momoResponse.PayUrl); // Chuyển hướng người dùng đến trang thanh toán Momo
            }
            else
            {
                TempData["Error"] = $"Lỗi tạo yêu cầu thanh toán Momo: {momoResponse?.Message ?? "Phản hồi rỗng"}";
                ViewBag.CartTotal = cart.CalculateTotal();
                return View("Checkout", order); // Quay về trang Checkout với lỗi
            }
        }

        // GET: /ShoppingCart/MomoPaymentCallBack (URL Momo gọi lại sau khi thanh toán)
        public async Task<IActionResult> MomoPaymentCallBack(IQueryCollection collection)
        {
            var response = _momoService.PaymentExecuteAsync(collection);

            // TODO: Xử lý kết quả từ Momo
            // 1. Kiểm tra signature và các thông tin bảo mật
            // 2. Cập nhật trạng thái đơn hàng trong DB của bạn dựa trên OrderId từ MomoResponse
            // response.OrderId, response.Amount, response.Message, etc.
            // Nếu thành công:
            // var order = await _orderDistributionRepository.GetByIdAsync(int.Parse(response.OrderId));
            // if(order != null) { order.Status = "Đã thanh toán Momo"; await _orderDistributionRepository.UpdateAsync(order); }

            // Chuyển hướng đến trang xác nhận OrderCompleted hoặc thông báo lỗi
            if (response.Message == "Successful.") // Giả định Momo trả về "Successful." khi thành công
            {
                TempData["Success"] = $"Thanh toán Momo thành công! Mã đơn hàng: {response.OrderId}";
                // Nếu bạn đã lưu order vào DB ở CreateMomoPayment, bạn cần lấy order đó ra và cập nhật trạng thái
                return RedirectToAction("OrderConfirmation", new { id = int.Parse(response.OrderId) });
            }
            else
            {
                TempData["Error"] = $"Thanh toán Momo thất bại: {response.Message}. Mã đơn hàng: {response.OrderId}";
                return RedirectToAction("Index", "ShoppingCart"); // Quay về giỏ hàng
            }
        }

        // POST: /ShoppingCart/MomoNotify (URL Momo gọi để thông báo kết quả giao dịch)
        // Đây là một endpoint webhook, Momo sẽ gửi POST request đến đây
        [HttpPost]
        public async Task<IActionResult> MomoNotify([FromBody] object payload) // Nhận payload từ Momo
        {
            // TODO: Xử lý thông báo từ Momo (Webhook)
            // Bạn cần phân tích payload để xác minh chữ ký và cập nhật trạng thái đơn hàng trong DB
            // Đây là cách an toàn nhất để cập nhật trạng thái đơn hàng từ Momo
            Console.WriteLine("Momo Notify received: " + JsonConvert.SerializeObject(payload));
            return Ok(); // Trả về 200 OK để Momo biết bạn đã nhận được thông báo
        }

        // GET: /ShoppingCart/OrderConfirmation/id (Trang xác nhận đơn hàng)
        public async Task<IActionResult> OrderConfirmation(int id)
        {
            var order = await _orderDistributionRepository.GetByIdAsync(id);
            if (order == null || order.CustomerId != _userManager.GetUserId(User))
            {
                return NotFound();
            }
            return View(order);
        }
    }
}