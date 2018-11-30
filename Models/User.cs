using System.ComponentModel.DataAnnotations;

namespace quizrtAuthServer.Models
{
    public class User
    {
        public int UserID { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Email { get; set; }
        public string Password { get; set; }
    }
}