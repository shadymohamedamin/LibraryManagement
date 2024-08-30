using System;

namespace LibraryManagement.ViewModels
{
    public class BorrowedBookViewModel
    {
        public Guid BookId { get; set; }         // Ensure this matches the property in the view
        public string BookName { get; set; } 
        public string UserName { get; set; }    // Ensure this matches the property in the view
        public int CopiesBorrowed { get; set; }  // Ensure this matches the property in the view
    }
    public class BorrowerDetails
    {
        public string UserName { get; set; }
        public int CopiesBorrowed { get; set; }
    }
}
