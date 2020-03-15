using System.ComponentModel.DataAnnotations;

namespace CLUZWeb.Models
{
    public class CreateGame
    {
        [Required]
        [StringLength(35, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 1)]
        public string Name { get; set; }

        [Required]
        [StringLength(35, ErrorMessage = "{0} length must be between {2} and {1}.", MinimumLength = 1)]
        public string Password { get; set; }
    }
}
