using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ReadingListPlus.Web.Core.ViewModels
{
    public class ImportViewModel
    {
        [Required]
        public IFormFile File { get; set; }

    }
}
