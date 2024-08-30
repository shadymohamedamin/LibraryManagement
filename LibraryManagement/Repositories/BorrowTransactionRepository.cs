using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore.Storage;
using LibraryManagement.Repositories;
using System;
using System.Data;


namespace LibraryManagement.Repositories
{

    public class BorrowTransactionRepository : IBorrowTransactionRepository
    {
        private readonly ApplicationDbContext _context;

        public BorrowTransactionRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /*public async Task<IEnumerable<BorrowTransaction>> GetTransactionsByUserAsync(string userId)
        {
            return await _context.BorrowTransactions
                                .Where(t => t.UserId == userId)
                                .ToListAsync();
        }*/

        // public async Task<IEnumerable<BorrowTransaction>> GetTransactionsByBookAndUserAsync(Guid bookId, string userId)
        // {
        //     return await _context.BorrowTransactions
        //                         .Where(t => t.BookId == bookId && t.UserId == userId)
        //                         .ToListAsync();
        // }

        // public async Task RemoveByBookAndUserAsync(Guid bookId, string userId)
        // {
        //     var transactions = await _context.BorrowTransactions
        //                                     .Where(t => t.BookId == bookId && t.UserId == userId)
        //                                     .ToListAsync();
        //     _context.BorrowTransactions.RemoveRange(transactions);
        //     await _context.SaveChangesAsync();
        // }
        public async Task<IEnumerable<BorrowTransaction>> GetTransactionsByBookAndUserAsync(Guid bookId, string userId)
        {
            return await _context.BorrowTransactions
                                .Where(bt => bt.BookId == bookId && bt.UserId == userId)
                                .ToListAsync();
        }

        public async Task RemoveByBookAndUserAsync(Guid bookId, string userId)
        {
            var transactions = await _context.BorrowTransactions
                                            .Where(bt => bt.BookId == bookId && bt.UserId == userId)
                                            .ToListAsync();
            _context.BorrowTransactions.RemoveRange(transactions);
            await _context.SaveChangesAsync();
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task RemoveBorrowTransactionAsync(BorrowTransaction transaction)
        {
            // Find the transaction by its Id, if needed
            var existingTransaction = await _context.BorrowTransactions
                .FindAsync(transaction.Id);

            if (existingTransaction != null)
            {
                _context.BorrowTransactions.Remove(existingTransaction);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Handle the case where the transaction is not found
                throw new InvalidOperationException("Transaction not found.");
            }
        }
        public async Task<IEnumerable<BorrowTransaction>> GetTransactionsByUserAsync(string userId)
        {
            return await _context.BorrowTransactions
                                .Include(t => t.Book) // Include related entities if necessary
                                .Where(t => t.UserId == userId)
                                .ToListAsync();
        }
        public async Task<IEnumerable<BorrowTransaction>> GetTransactionsByBookAsync(Guid bookId)
        {
            return await _context.BorrowTransactions
                .Where(bt => bt.BookId == bookId)
                .ToListAsync();
        }




        public async Task<BorrowTransaction> GetBorrowTransactionAsync(Guid bookId,string userId)
    {
        return await _context.BorrowTransactions
            .FirstOrDefaultAsync(bt => bt.UserId == userId && bt.BookId == bookId);
    }

    public async Task AddBorrowTransactionAsync(BorrowTransaction borrowTransaction)
    {
        await _context.BorrowTransactions.AddAsync(borrowTransaction);
    }

    public async Task UpdateBorrowTransactionAsync(BorrowTransaction borrowTransaction)
    {
        _context.BorrowTransactions.Update(borrowTransaction);
    }

    /*public async Task RemoveBorrowTransactionAsync(BorrowTransaction borrowTransaction)
    {
        _context.BorrowTransactions.Remove(borrowTransaction);
    }*/

    public async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel)
    {
        return await _context.Database.BeginTransactionAsync(isolationLevel);
    }

    }
}