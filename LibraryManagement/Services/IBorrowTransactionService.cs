using System;
using System.Threading.Tasks;
using LibraryManagement.Repositories;
using LibraryManagement.Models;
using LibraryManagement.ViewModels;

namespace LibraryManagement.Services
{
    public interface IBorrowTransactionService
    {
        Task RemoveTransactionsByBookAndUserAsync(Guid bookId, string userId);
        Task<IEnumerable<BorrowedBookViewModel>> GetBorrowedBooksByUserAsync(string userId);
        Task<IEnumerable<BorrowTransaction>> GetTransactionsByBookAsync(Guid bookId);
    }
}
