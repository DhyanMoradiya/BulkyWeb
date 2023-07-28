using Bulky.Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Bulky.Model.ViewModels
{
    public class ProductVM
    {
        public Product product { get; set; }
        [ValidateNever]
        public IEnumerable<SelectListItem> categoryList { get; set; }
    }
}
