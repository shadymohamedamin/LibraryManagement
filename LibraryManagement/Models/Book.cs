
using System.ComponentModel.DataAnnotations;

namespace LibraryManagement.Models
{
    public class Book
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(100, ErrorMessage = "The title must be between 1 and 100 characters long.", MinimumLength = 1)]
        public string Name { get; set; } = string.Empty;
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "The number of copies must be at least 1.")]
        public int TotalCopies { get; set; }

        public int AvailableCopies { get; set; }
        public bool IsAvailable { get; set; } = true;

        public string ImagePath { get; set; }

        public ICollection<BorrowTransaction> BorrowTransactions { get; set; } = new List<BorrowTransaction>();

    }
}
