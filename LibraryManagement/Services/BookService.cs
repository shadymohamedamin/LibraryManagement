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

        public BookService(IBookRepository bookRepository)
        {
            _bookRepository = bookRepository;
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


        public async Task ReturnBookAsync(Guid bookId, string userId)
        {
            using var transaction = await _bookRepository.BeginTransactionAsync();
            try
            {
                var borrowTransaction = await _bookRepository.GetTransactionAsync(bookId, userId);
                if (borrowTransaction == null)
                {
                    throw new InvalidOperationException("No borrow record found.");
                }

                var book = await _bookRepository.GetBookByIdAsync(bookId);
                if (book == null)
                {
                    throw new InvalidOperationException("Book not found.");
                }

                book.AvailableCopies += 1;
                await _bookRepository.UpdateBookAsync(book);

                await _bookRepository.RemoveBorrowTransactionAsync(borrowTransaction);
                await _bookRepository.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
