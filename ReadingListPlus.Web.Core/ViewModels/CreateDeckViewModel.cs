using System.ComponentModel.DataAnnotations;

namespace ReadingListPlus.Web.Core.ViewModels
{
    public class CreateDeckViewModel
    {
        [Required]
        public string Title { get; set; }
    }
}