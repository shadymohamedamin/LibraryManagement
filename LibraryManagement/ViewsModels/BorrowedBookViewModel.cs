using System;

namespace LibraryManagement.ViewModels
{
    public class BorrowedBookViewModel
    {
        public Guid BookId { get; set; }         
        public string ImagePath { get; set; }
        public string BookName { get; set; } 
        public string UserName { get; set; }    
        public int CopiesBorrowed { get; set; }  
    }
    public class BorrowerDetails
    {
        public string UserName { get; set; }
        public int CopiesBorrowed { get; set; }
    }
}
