using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;


namespace LibraryManagement.Services
{
    public interface IBookService
    {
        Task<IEnumerable<Book>> GetAllBooksAsync();
        Task<Book> GetBookByIdAsync(Guid id);
        Task AddBookAsync(Book book);
        Task UpdateBookAsync(Book book);
        Task<bool> IsBookAvailableAsync(Guid id);
        Task BorrowBookAsync(Guid bookId, string userId);
        Task ReturnBookAsync(Guid bookId, string userId);
        Task<IEnumerable<Book>> GetBorrowedBooksAsync(string userId);
    }
}
