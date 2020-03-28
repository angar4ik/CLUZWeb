using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CLUZWeb.Models
{
    public class FeedbackForm
    {
        [Required]
        [StringLength(1000, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 1)]
        public string Text { get; set; }
    }
}
