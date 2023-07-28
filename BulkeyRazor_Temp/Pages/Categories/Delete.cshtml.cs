using BulkeyRazor_Temp.Data;
using BulkeyRazor_Temp.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BulkeyRazor_Temp.Pages.Categories
{
    public class DeleteModel : PageModel
    {

        private readonly ApplicationDbContext _db;
        [BindProperty]
        public Category category { get; set; }


        public DeleteModel(ApplicationDbContext db)
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

        public IActionResult OnPost()
        {
            _db.Categories.Remove(category);
            _db.SaveChanges();
            TempData["success"] = "CATEGORY DELETED SUCCESSFULLY";
            return RedirectToPage("Index");
        }
    }
}
