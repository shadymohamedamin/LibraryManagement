using LibraryManagement.Repositories;
using LibraryManagement.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibraryManagement.ViewModels;
using LibraryManagement.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;


namespace LibraryManagement.Services
{
    public class BorrowTransactionService : IBorrowTransactionService
    {
        private readonly IBorrowTransactionRepository _borrowTransactionRepository;
        private readonly IBookRepository _bookRepository;
        private readonly ApplicationDbContext _context;

        public BorrowTransactionService(ApplicationDbContext context,IBorrowTransactionRepository borrowTransactionRepository, IBookRepository bookRepository)
        {
            _borrowTransactionRepository = borrowTransactionRepository;
            _context = context;
            _bookRepository = bookRepository;
        }

        public async Task RemoveTransactionsByBookAndUserAsync(Guid bookId, string userId)
        {
            var transactions = await _borrowTransactionRepository.GetTransactionsByBookAndUserAsync(bookId, userId);
            var transactionList = transactions.ToList();

            var book = await _bookRepository.GetBookByIdAsync(bookId);

            if (book == null)
            {
                throw new ArgumentException("Book not found.");
            }

            foreach (var transaction in transactionList)
            {
                await _borrowTransactionRepository.RemoveBorrowTransactionAsync(transaction);
            }

            book.AvailableCopies += transactionList.Count;
            await _bookRepository.UpdateBookAsync(book);

            await _borrowTransactionRepository.SaveChangesAsync();
        }

        public async Task<BookDetailsViewModel> GetBookDetailsAsync(Guid bookId)
        {
            var book = await _context.Books
                .Where(b => b.Id == bookId)
                .FirstOrDefaultAsync();

            if (book == null)
            {
                return null; 
            }

            var borrowTransactions = await _context.BorrowTransactions
                .Where(bt => bt.BookId == bookId)
                .Include(bt => bt.User)
                .ToListAsync();

            var viewModel = new BookDetailsViewModel
            {
                BookId = book.Id,
                ImagePath=book.ImagePath,
                BookName = book.Name,
                TotalCopies = book.TotalCopies,
                AvailableCopies = book.AvailableCopies,
                Borrowers = borrowTransactions.Select(bt => new UserBookViewModel 
                {
                    UserName = bt.User.UserName,
                    CopiesBorrowed = bt.Copies
                }).ToList()
            };

            return viewModel;
        }



        public async Task<IEnumerable<BorrowedBookViewModel>> GetBorrowedBooksByUserAsync(string userId)
        {
            var borrowTransactions = await _context.BorrowTransactions
                .Where(bt => bt.UserId == userId)
                .Include(bt => bt.Book) 
                .ToListAsync();
            var borrowedBooks = borrowTransactions.Select(bt => new BorrowedBookViewModel
            {
                BookId = bt.Book.Id,
                ImagePath = bt.Book.ImagePath,
                BookName = bt.Book.Name,
                CopiesBorrowed = bt.Copies 
            }).ToList();

            return borrowedBooks;
        }

        public async Task<IEnumerable<BorrowTransaction>> GetTransactionsByBookAsync(Guid bookId)
        {
            return await _borrowTransactionRepository.GetTransactionsByBookAsync(bookId);
        }

    }
}
