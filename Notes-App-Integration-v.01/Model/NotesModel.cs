using System.ComponentModel.DataAnnotations;

namespace Notes_App_Integration_v._01.Model
{
    public class NotesModel
    {
        [Key]
        public Guid Id { get; set; }
        [Required]
        public String title { get; set; }
        [Required]
        public string content { get; set; }

    }
}
