using System.ComponentModel.DataAnnotations;

namespace ReadingListPlus.Web.ViewModels
{
    public class DeckViewModel
    {
        [Required]
        public string Title { get; set; }
    }
}