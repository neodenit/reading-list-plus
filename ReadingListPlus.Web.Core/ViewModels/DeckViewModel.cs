using System.ComponentModel.DataAnnotations;

namespace ReadingListPlus.Web.Core.ViewModels
{
    public class DeckViewModel
    {
        [Required]
        public string Title { get; set; }
    }
}