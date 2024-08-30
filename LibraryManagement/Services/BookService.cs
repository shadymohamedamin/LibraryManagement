using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryManagement.Models;
using LibraryManagement.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

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

        public async Task<bool> IsBookAvailableAsync(Guid id)
        {
            var book = await _bookRepository.GetBookByIdAsync(id);
            return book != null && book.IsAvailable; // Assuming Book has an IsAvailable property
        }

        public async Task BorrowBookAsync(Guid bookId,string userId)
        {
            using var transaction = await _borrowTransactionRepository.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var borrowTransaction = await _borrowTransactionRepository.GetBorrowTransactionAsync(bookId,userId);
                var book = await _bookRepository.GetBookByIdAsync(bookId);

                if (book == null || book.AvailableCopies < 1)
                {
                    throw new InvalidOperationException("Book not available.");
                }

                if (borrowTransaction != null)
                {
                    // Increment the number of copies borrowed
                    borrowTransaction.Copies++;
                    await _borrowTransactionRepository.UpdateBorrowTransactionAsync(borrowTransaction);
                }
                else
                {
                    // Create a new borrow transaction
                    borrowTransaction = new BorrowTransaction
                    {
                        Id = Guid.NewGuid(),
                        UserId = userId,
                        BookId = bookId,
                        Copies = 1
                    };
                    await _borrowTransactionRepository.AddBorrowTransactionAsync(borrowTransaction);
                }

                // Decrease available copies of the book
                book.AvailableCopies--;
                await _bookRepository.UpdateBookAsync(book);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task ReturnBookAsync( Guid bookId,string userId)
        {
            using var transaction = await _borrowTransactionRepository.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                var borrowTransaction = await _borrowTransactionRepository.GetBorrowTransactionAsync(bookId,userId );
                if (borrowTransaction == null)
                {
                    throw new InvalidOperationException("No transaction found for this user and book.");
                }

                var book = await _bookRepository.GetBookByIdAsync(bookId);

                // Increase available copies by the number of borrowed copies
                book.AvailableCopies += borrowTransaction.Copies;

                // Remove the transaction as all copies are returned
                await _borrowTransactionRepository.RemoveBorrowTransactionAsync(borrowTransaction);
                await _bookRepository.UpdateBookAsync(book);

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
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
