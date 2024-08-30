using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LibraryManagement.Repositories
{
    public interface IBorrowTransactionRepository
    {
        Task<IEnumerable<BorrowTransaction>> GetTransactionsByUserAsync(string userId);
        Task<IEnumerable<BorrowTransaction>> GetTransactionsByBookAndUserAsync(Guid bookId, string userId);
        Task RemoveByBookAndUserAsync(Guid bookId, string userId);
        Task RemoveBorrowTransactionAsync(BorrowTransaction transaction);
        Task SaveChangesAsync();
        Task<IEnumerable<BorrowTransaction>> GetTransactionsByBookAsync(Guid bookId);
        //Task<IEnumerable<BorrowTransaction>> GetTransactionsByUserAsync(string userId);
        
    }
}
