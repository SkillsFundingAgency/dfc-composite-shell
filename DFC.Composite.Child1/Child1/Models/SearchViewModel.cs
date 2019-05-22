using System.ComponentModel.DataAnnotations;

namespace Child1.Models
{
    public class SearchViewModel
    {
        [Display(Name = "Search Clue", Prompt = "Search Clue", Description = "Enter a Search Clue for a Course")]
        public string Clue { get; set; }
    }
}
