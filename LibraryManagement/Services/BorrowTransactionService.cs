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
            // Retrieve all transactions for the given book and user
            var transactions = await _borrowTransactionRepository.GetTransactionsByBookAndUserAsync(bookId, userId);
            var transactionList = transactions.ToList();

            // Retrieve the book to update its available copies
            var book = await _bookRepository.GetBookByIdAsync(bookId);

            if (book == null)
            {
                throw new ArgumentException("Book not found.");
            }

            // Remove all transactions
            foreach (var transaction in transactionList)
            {
                await _borrowTransactionRepository.RemoveBorrowTransactionAsync(transaction);
            }

            // Update the book's available copies
            book.AvailableCopies += transactionList.Count; // Add the number of returned books
            await _bookRepository.UpdateBookAsync(book);

            // Commit changes
            await _borrowTransactionRepository.SaveChangesAsync();
        }

        // public async Task<IEnumerable<BorrowedBookViewModel>> GetBorrowedBooksByUserAsync(string userId)
        // {
        //     // Fetch all borrow transactions for the user
        //     var transactions = await _borrowTransactionRepository.GetTransactionsByUserAsync(userId);
            
        //     // Fetch book details based on transactions
        //     var borrowedBooks = transactions.GroupBy(t => t.BookId)
        //                                     .Select(g => new BorrowedBookViewModel
        //                                     {
        //                                         BookId = g.Key,
        //                                         BookName = g.First().Book.Name, // Ensure 'Book' property exists in BorrowTransaction
        //                                         CopiesBorrowed = g.Count()
        //                                     }).ToList();

        //     return borrowedBooks;
        // }
        public async Task<BookDetailsViewModel> GetBookDetailsAsync(Guid bookId)
{
    var book = await _context.Books
        .Where(b => b.Id == bookId)
        .FirstOrDefaultAsync();

    if (book == null)
    {
        return null; // Handle not found case
    }

    var borrowTransactions = await _context.BorrowTransactions
        .Where(bt => bt.BookId == bookId)
        .Include(bt => bt.User)
        .ToListAsync();

    var viewModel = new BookDetailsViewModel
    {
        BookId = book.Id,
        BookName = book.Name,
        TotalCopies = book.TotalCopies,
        AvailableCopies = book.AvailableCopies,
        Borrowers = borrowTransactions.Select(bt => new UserBookViewModel // Change to UserBookViewModel if needed
        {
            UserName = bt.User.UserName,
            CopiesBorrowed = book.TotalCopies//bt.Copies // Adjust if needed
        }).ToList()
    };

    return viewModel;
}



    public async Task<IEnumerable<BorrowedBookViewModel>> GetBorrowedBooksByUserAsync(string userId)
        {
            var borrowTransactions = await _context.BorrowTransactions
                .Where(bt => bt.UserId == userId)
                .Include(bt => bt.Book) // Ensure Book is loaded
                .ToListAsync();

            var borrowedBooks = borrowTransactions.Select(bt => new BorrowedBookViewModel
            {
                BookId = bt.Book.Id,
                BookName = bt.Book.Name,
                CopiesBorrowed = bt.Book.TotalCopies // Or use any other property relevant to your data
            }).ToList();

            return borrowedBooks;
        }

        public async Task<IEnumerable<BorrowTransaction>> GetTransactionsByBookAsync(Guid bookId)
        {
            return await _borrowTransactionRepository.GetTransactionsByBookAsync(bookId);
        }

    }
}
