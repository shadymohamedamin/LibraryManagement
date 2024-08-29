using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class BorrowTransaction
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Guid BookId { get; set; } 
        public Book Book { get; set; }

        [Required]
        public string UserId { get; set; }
        public IdentityUser User { get; set; }

        // [Required]
        // public int NumberOfCopies { get; set; }

        // [Required]
        // public DateTime DateBorrowed { get; set; } = DateTime.UtcNow;
    }

}
