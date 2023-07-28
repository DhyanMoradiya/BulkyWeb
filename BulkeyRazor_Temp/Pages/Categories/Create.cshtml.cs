using BulkeyRazor_Temp.Data;
using BulkeyRazor_Temp.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkeyRazor_Temp.Pages.Categories
{
    public class CreateModel : PageModel
    {

        private readonly ApplicationDbContext _db;
        public CreateModel(ApplicationDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public Category category { get; set; }


        public void OnGet()
        {
        }

        public IActionResult OnPost()
        {
            _db.Categories.Add(category); 
            _db.SaveChanges();
            TempData["success"] = "CATEGORY CREATED SUCCESSFULLY";
            return RedirectToPage("Index");
        }
    }
}
