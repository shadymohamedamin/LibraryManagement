using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryManagement.Models;
using LibraryManagement.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;


namespace LibraryManagement.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly IBorrowTransactionRepository _borrowTransactionRepository;


        public BookService(IBookRepository bookRepository, IBorrowTransactionRepository borrowTransactionRepository)
        {
            _bookRepository = bookRepository;
            _borrowTransactionRepository = borrowTransactionRepository;
        }

        public async Task<IEnumerable<Book>> GetAllBooksAsync()
        {
            return await _bookRepository.GetAllBooksAsync();
        }

        public async Task<Book> GetBookByIdAsync(Guid id)
        {
            return await _bookRepository.GetBookByIdAsync(id);
        }

        public async Task AddBookAsync(Book book)
        {
            await _bookRepository.AddBookAsync(book);
        }

        public async Task UpdateBookAsync(Book book)
        {
            await _bookRepository.UpdateBookAsync(book);
        }

        /*public async Task DeleteBookAsync(Guid id)
        {
            await _bookRepository.DeleteBookAsync(id);
        }*/

        public async Task<bool> IsBookAvailableAsync(Guid id)
        {
            var book = await _bookRepository.GetBookByIdAsync(id);
            return book != null && book.IsAvailable; // Assuming Book has an IsAvailable property
        }
        /*public async Task BorrowBookAsync(Guid bookId, string userId)
        {
            using var transaction = await _bookRepository.BeginTransactionAsync();
            try
            {
                var book = await _bookRepository.GetBookByIdAsync(bookId);
                if (book == null || book.AvailableCopies <= 0)
                {
                    throw new InvalidOperationException("Book is not available.");
                }

                book.AvailableCopies -= 1;
                await _bookRepository.UpdateBookAsync(book);

                var borrowTransaction = new BorrowTransaction
                {
                    Id = Guid.NewGuid(),
                    BookId = bookId,
                    UserId = userId,
                    // Set additional properties as needed
                };

                await _bookRepository.AddBorrowTransactionAsync(borrowTransaction);
                await _bookRepository.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }*/
        public async Task BorrowBookAsync(Guid bookId, string userId)
        {
            var book = await _bookRepository.GetBookByIdAsync(bookId);
            if (book == null || book.AvailableCopies <= 0)
            {
                throw new Exception("Book not available");
            }

            using (var transaction = await _bookRepository.BeginTransactionAsync())
            {
                try
                {
                    // Decrease the available copies
                    book.AvailableCopies -= 1;
                    await _bookRepository.UpdateBookAsync(book);

                    // Create and save the borrow transaction
                    var borrowTransaction = new BorrowTransaction
                    {
                        BookId = bookId,
                        UserId = userId,
                        //BorrowDate = DateTime.UtcNow
                    };

                    await _bookRepository.AddBorrowTransactionAsync(borrowTransaction);

                    await _bookRepository.SaveChangesAsync();
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        /*public async Task RemoveByBookAndUserAsync(Guid bookId, string userId)
        {
            var transactions = _context.BorrowTransactions
                .Where(t => t.BookId == bookId && t.UserId == userId);

            _context.BorrowTransactions.RemoveRange(transactions);
            await _context.SaveChangesAsync();
        }*/


        /*public async Task<IEnumerable<Book>> GetBorrowedBooksAsync(string userId)
        {
            return await _bookRepository.GetBorrowedBooksByUserAsync(userId);
        }*/

        // Handle the logic of returning a book
        /*public async Task ReturnBookAsync(Guid bookId, string userId)
        {
            using var transaction = await _bookRepository.BeginTransactionAsync();

            try
            {
                // Get the book
                var book = await _bookRepository.GetBookByIdAsync(bookId);
                if (book == null)
                {
                    throw new ArgumentException("Book not found.");
                }

                // Count the number of transactions
                int transactionCount = await _bookRepository.CountTransactionsByBookAndUserAsync(bookId, userId);

                // Remove all transactions for the book and user
                await _borrowTransactionRepository.RemoveByBookAndUserAsync(bookId, userId);

                // Update the book's copies
                book.AvailableCopies += transactionCount;
                book.TotalCopies += transactionCount; // Adjust if necessary
                await _bookRepository.UpdateBookAsync(book);

                // Commit the transaction
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new InvalidOperationException("Error processing the return request.", ex);
            }
        }*/
        public async Task ReturnBookAsync(Guid bookId, string userId)
        {
            using (var transaction = await _bookRepository.BeginTransactionAsync())
            {
                try
                {
                    // Count the number of borrowed copies
                    var borrowedTransactions = await _borrowTransactionRepository.GetTransactionsByBookAndUserAsync(bookId, userId);
                    var borrowedCount = borrowedTransactions.Count();

                    // Remove transactions
                    await _borrowTransactionRepository.RemoveByBookAndUserAsync(bookId, userId);

                    // Update book copies
                    var book = await _bookRepository.GetBookByIdAsync(bookId);
                    book.AvailableCopies += borrowedCount;
                    //book.TotalCopies += borrowedCount;

                    await _bookRepository.UpdateBookAsync(book);
                    await transaction.CommitAsync();
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
        }

        public async Task<IEnumerable<Book>> GetBorrowedBooksAsync(string userId)
        {
            var transactions = await _borrowTransactionRepository.GetTransactionsByUserAsync(userId);
            var bookIds = transactions.Select(t => t.BookId).Distinct();

            var books = new List<Book>();
            foreach (var bookId in bookIds)
            {
                var book = await _bookRepository.GetBookByIdAsync(bookId);
                if (book != null)
                {
                    books.Add(book);
                }
            }

            return books;
        }
    }
}
