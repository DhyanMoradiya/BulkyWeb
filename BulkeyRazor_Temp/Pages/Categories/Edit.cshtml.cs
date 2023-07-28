using BulkeyRazor_Temp.Data;
using BulkeyRazor_Temp.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkeyRazor_Temp.Pages.Categories
{
    public class EditModel : PageModel
    {

        private readonly ApplicationDbContext _db;

        [BindProperty]
        public Category category { get; set; }


        public EditModel(ApplicationDbContext db)
        {
            _db = db;
        }
        public void OnGet(int? Id)
        {
            if (Id != null)
            {
                category = _db.Categories.Find(Id);
            }
        }

        public IActionResult OnPost() { 
            _db.Categories.Update(category);
            _db.SaveChanges();
            TempData["success"] = "CATEGORY UPDATED SUCCESSFULLY";
            return RedirectToPage("Index");
         }
    }
}
