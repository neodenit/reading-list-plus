using System.ComponentModel.DataAnnotations;
using System.Web;

namespace ReadingListPlus.Web.ViewModels
{
    public class ImportViewModel
    {
        [Required]
        public HttpPostedFileBase File { get; set; }

    }
}
