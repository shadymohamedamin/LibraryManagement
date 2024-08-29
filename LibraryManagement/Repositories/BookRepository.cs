using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LibraryManagement.Data;
using LibraryManagement.Models;
using Microsoft.EntityFrameworkCore.Storage;


namespace LibraryManagement.Repositories
{
    public class BookRepository : IBookRepository
    {
        private readonly ApplicationDbContext _context;

        public BookRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync()
        {
            return await _context.Books.ToListAsync();
        }

        public async Task<Book> GetBookByIdAsync(Guid id)
        {
            return await _context.Books.FindAsync(id);
        }

        public async Task AddBookAsync(Book book)
        {
            _context.Books.Add(book);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateBookAsync(Book book)
        {
            _context.Books.Update(book);
            await _context.SaveChangesAsync();
        }
        public async Task AddBorrowTransactionAsync(BorrowTransaction transaction)
        {
            _context.BorrowTransactions.Add(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task RemoveBorrowTransactionAsync(BorrowTransaction transaction)
        {
            _context.BorrowTransactions.Remove(transaction);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<BorrowTransaction> GetTransactionAsync(Guid bookId, string userId)
        {
            return await _context.BorrowTransactions
                .FirstOrDefaultAsync(t => t.BookId == bookId && t.UserId == userId);
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }

        /*public async Task DeleteBookAsync(Guid id)
        {
            var book = await _context.Books.FindAsync(id);
            if (book != null)
            {
                _context.Books.Remove(book);
                await _context.SaveChangesAsync();
            }
        }*/
    }
}
