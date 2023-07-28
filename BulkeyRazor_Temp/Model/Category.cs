using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace BulkeyRazor_Temp.Model
{
        public class Category
        {
            [Key]
            public int Id { get; set; }
            [Required]
            [DisplayName("Category Name")]
            public string Name { get; set; }

            [Range(1, 100, ErrorMessage = "Must me between 1 to 100")]
            [DisplayName("Display Order")]
            public int DisplayOrder { get; set; }

    }
}
