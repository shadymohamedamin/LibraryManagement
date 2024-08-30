using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;


namespace LibraryManagement.Repositories
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllBooksAsync();
        Task<Book> GetBookByIdAsync(Guid id);
        Task AddBookAsync(Book book);
        Task UpdateBookAsync(Book book);
        Task AddBorrowTransactionAsync(BorrowTransaction transaction);
        Task RemoveBorrowTransactionAsync(BorrowTransaction transaction);
        Task SaveChangesAsync();
        Task<BorrowTransaction> GetTransactionAsync(Guid bookId, string userId);
        Task<IDbContextTransaction> BeginTransactionAsync();
        Task<int> CountTransactionsByBookAndUserAsync(Guid bookId, string userId);
        
        //Task RemoveTransactionsByBookAndUserAsync(Guid bookId, string userId);
        //Task RemoveByBookAndUserAsync(Guid bookId, string userId);
        //Task DeleteBookAsync(Guid id);
    }
}
