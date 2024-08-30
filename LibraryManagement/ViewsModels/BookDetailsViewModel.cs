using System.Collections.Generic;
using LibraryManagement.Models;

// public class BookDetailsViewModel
// {
//     public Book Book { get; set; }
//     public List<BorrowedBookViewModel> BorrowTransactions { get; set; }
// }

// public class BorrowedBookViewModel
// {
//     public string UserName { get; set; }
//     public int BorrowedCopies { get; set; }
// }


namespace LibraryManagement.ViewModels
{
    public class BookDetailsViewModel
    {
        public Guid BookId { get; set; }
        public string BookName { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public IEnumerable<UserBookViewModel> Borrowers { get; set; } // Ensure this property exists
    }

    public class UserBookViewModel
    {
        public string UserName { get; set; }
        public int CopiesBorrowed { get; set; }
    }
}

