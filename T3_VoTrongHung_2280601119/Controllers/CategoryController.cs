using Microsoft.AspNetCore.Mvc;
using T3_VoTrongHung_2280601119.Models;
using Microsoft.EntityFrameworkCore;


namespace T3_VoTrongHung_2280601119.Controllers
{
    public class CategoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Index()
        {
            var categories = _context.Categorie.ToList();
            return View(categories);
        }

        public IActionResult Details(int id)
        {
            var category = _context.Categorie
                .Include(c => c.Products)
                .FirstOrDefault(c => c.Id == id);
            if (category == null) return NotFound();

            return View(category);
        }
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categorie.Add(category);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }


        public IActionResult Edit(int id)
        {
            var category = _context.Categorie.Find(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost]
        public IActionResult Edit(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Update(category);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        public IActionResult Delete(int id)
        {
            var category = _context.Categorie.Find(id);
            if (category == null) return NotFound();
            return View(category);
        }

        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(int id)
        {
            var category = _context.Categorie
                .Include(c => c.Products)
                .FirstOrDefault(c => c.Id == id);

            if (category != null)
            {
                // Retrieve the default category before updating products
                var defaultCategory = _context.Categorie.FirstOrDefault(c => c.Name == "Không phân loại");

                if (defaultCategory != null)
                {
                    foreach (var product in category.Products.ToList())
                    {
                        product.CategoryId = defaultCategory.Id;
                        _context.Product.Update(product);
                    }
                }

                _context.Categorie.Remove(category);
                _context.SaveChanges();
            }

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Add(Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Categorie.Add(category);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(category);
        }
    }
}
