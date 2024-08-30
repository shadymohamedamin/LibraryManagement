using System;

namespace LibraryManagement.ViewModels
{
    public class BookCreateViewModel
    {
        public string Name { get; set; }
        public int TotalCopies { get; set; }
        public IFormFile ImageFile { get; set; }
    }

}
