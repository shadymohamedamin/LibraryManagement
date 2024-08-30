using System.Collections.Generic;
using LibraryManagement.Models;

namespace LibraryManagement.ViewModels
{
    public class BookDetailsViewModel
    {
        public Guid BookId { get; set; }
        public string ImagePath { get; set; }
        public string BookName { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public IEnumerable<UserBookViewModel> Borrowers { get; set; } 
    }

    public class UserBookViewModel
    {
        public string UserName { get; set; }
        public int CopiesBorrowed { get; set; }
    }
}

