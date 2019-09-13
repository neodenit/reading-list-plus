using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ReadingListPlus.Services.ViewModels
{
    public class ImportViewModel
    {
        [Required]
        public IFormFile File { get; set; }
    }
}
