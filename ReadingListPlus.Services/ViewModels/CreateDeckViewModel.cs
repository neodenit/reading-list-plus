using System.ComponentModel.DataAnnotations;

namespace ReadingListPlus.Services.ViewModels
{
    public class CreateDeckViewModel
    {
        [Required]
        public string Title { get; set; }
    }
}