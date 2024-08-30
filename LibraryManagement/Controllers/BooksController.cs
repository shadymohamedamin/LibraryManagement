using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Identity;
using LibraryManagement.Repositories;
using LibraryManagement.ViewModels;
using LibraryManagement.Data;

namespace LibraryManagement.Controllers
{
    //[Authorize]
    public class BooksController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<BooksController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IBorrowTransactionService _borrowTransactionService;
        private readonly ApplicationDbContext _context;

        public BooksController(ApplicationDbContext context,IBookRepository bookRepository, IBookService bookService, IBorrowTransactionService borrowTransactionService, ILogger<BooksController> logger, UserManager<IdentityUser> userManager)
        {
            _bookService = bookService;
            _context = context;
            _bookRepository = bookRepository;
            _logger = logger;
            _userManager = userManager;
            _borrowTransactionService = borrowTransactionService;
        }

        public async Task<IActionResult> Index()
        {
            var books = await _bookService.GetAllBooksAsync();
            return View(books);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        [HttpGet]
        public IActionResult Create()
        {
            // Initialize a new Book object and pass it to the view
            var book = new Book();
            return View(book);
        }

        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Book book)
        {
            if (book == null)
            {
                // Handle the case where book is null
                _logger.LogError("Received null book model in Create action.");
                return BadRequest("Book model is null.");
            }

            if (ModelState.IsValid)
            {
                // Add the book to the database
                book.AvailableCopies = book.TotalCopies;
                await _bookService.AddBookAsync(book);
                return RedirectToAction(nameof(Index));
            }

            return View(book);
        }*/
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,TotalCopies")] Book book)
        {
            if (ModelState.IsValid)
            {
                book.AvailableCopies = book.TotalCopies; // Ensure AvailableCopies matches TotalCopies

                try
                {
                    await _bookService.AddBookAsync(book);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating book");
                    ModelState.AddModelError("", "Unable to save changes. Please try again later.");
                }
            }
            return View(book);
        }



        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }
            return View(book);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,Name,TotalCopies")] Book book)
        {
            if (id != book.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Fetch existing book details
                    var existingBook = await _bookService.GetBookByIdAsync(id);
                    if (existingBook == null)
                    {
                        return NotFound();
                    }

                    // Ensure AvailableCopies does not exceed TotalCopies
                    if (book.TotalCopies < existingBook.AvailableCopies)
                    {
                        ModelState.AddModelError("TotalCopies", "Total copies cannot be less than available copies.");
                        return View(book);
                    }

                    if (book.TotalCopies > existingBook.TotalCopies)
                    {
                        var difference = book.TotalCopies - existingBook.TotalCopies;
                        existingBook.AvailableCopies += difference;
                    }

                    // Update the book
                    existingBook.Name = book.Name;
                    existingBook.TotalCopies = book.TotalCopies;
                    existingBook.AvailableCopies = Math.Min(existingBook.AvailableCopies, book.TotalCopies);

                    await _bookService.UpdateBookAsync(existingBook);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating book");
                    ModelState.AddModelError("", "Unable to save changes. Please try again later.");
                }
            }
            return View(book);
        }

/*[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Borrow(Guid bookId)
{
    _logger.LogInformation("Borrow request received with Book ID: {BookId}", bookId);

    if (!User.Identity.IsAuthenticated)
    {
        _logger.LogWarning("User not authenticated.");
        return Unauthorized();
    }

    var user = await _userManager.GetUserAsync(User);
    if (user == null)
    {
        _logger.LogWarning("User not found.");
        return Unauthorized();
    }

    try
    {
        if (bookId == Guid.Empty)
        {
            _logger.LogWarning("Invalid book ID.");
            return BadRequest("Invalid book ID.");
        }

        var book = await _bookService.GetBookByIdAsync(bookId);
        if (book == null)
        {
            _logger.LogWarning("Book not found.");
            return NotFound("Book not found.");
        }
        
        if (book.AvailableCopies <= 0)
        {
            _logger.LogWarning("No copies available for borrowing.");
            return BadRequest("No copies available for borrowing.");
        }

        await _bookService.BorrowBookAsync(bookId, user.Id);
        _logger.LogInformation("Book with ID: {BookId} borrowed successfully.", bookId);
        return Json(new { success = true });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error occurred while borrowing book with ID: {BookId}", bookId);
        return Json(new { success = false, message = $"An error occurred: {ex.Message}" });
    }
}*/
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Borrow(Guid bookId)
    {
        if (!User.Identity.IsAuthenticated)
        {
            return Unauthorized();
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Unauthorized();
        }

        try
        {
            await _bookService.BorrowBookAsync(bookId, user.Id);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error borrowing book");
            return StatusCode(500, "Internal server error");
        }
    }





        // public async Task<IActionResult> MyBooks()
        // {
        //     var currentUser = await _userManager.GetUserAsync(User);
        //     if (currentUser == null)
        //     {
        //         return RedirectToAction("Login", "Account");
        //     }

        //     var borrowedBooks = await _bookService.GetBorrowedBooksAsync(currentUser.Id);
        //     return View(borrowedBooks);
        // }
        public async Task<IActionResult> MyBooks()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var borrowedBooks = await _borrowTransactionService.GetBorrowedBooksByUserAsync(user.Id);
            return View(borrowedBooks);
        }
        // public async Task<IActionResult> MyBooks()
        // {
        //     var userId = User.Identity.Name; // Or however you get the current user's ID
        //     var borrowedBooks = await _borrowTransactionService.GetBorrowedBooksByUserAsync(userId);

        //     return View(borrowedBooks);
        // }

        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public async Task<IActionResult> ReturnBook(Guid bookId)
        // {
        //     if (bookId == Guid.Empty)
        //     {
        //         return BadRequest();
        //     }

        //     try
        //     {
        //         // Get the current user
        //         var user = await _userManager.GetUserAsync(User);
        //         if (user == null)
        //         {
        //             return Unauthorized();
        //         }

        //         // Get the book and existing borrow transactions
        //         var book = await _bookService.GetBookByIdAsync(bookId);
        //         if (book == null)
        //         {
        //             return NotFound();
        //         }

        //         // Remove all borrow transactions for the current user and the specified book
        //         await _borrowTransactionService.RemoveTransactionsByBookAndUserAsync(bookId, user.Id);

        //         // Update the available copies of the book
        //         book.AvailableCopies++;
        //         await _bookService.UpdateBookAsync(book);

        //         return Ok();
        //     }
        //     catch (Exception ex)
        //     {
        //         _logger.LogError(ex, "Error returning the book");
        //         return StatusCode(500, "Internal server error");
        //     }
        // }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ReturnBook(Guid bookId)
        {
            if (bookId == Guid.Empty)
            {
                return BadRequest();
            }

            try
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return Unauthorized();
                }

                await _bookService.ReturnBookAsync(bookId, user.Id);

                return RedirectToAction(nameof(MyBooks)); // Adjust the action name as needed
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error returning the book");
                return StatusCode(500, "Internal server error");
            }
        }



        /*public async Task<IActionResult> BookDetails(Guid id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
                return NotFound();
            }

            var transactions = await _borrowTransactionService.GetTransactionsByBookAsync(id);

            var viewModel = new BookDetailsViewModel
            {
                Book = book,
                BorrowTransactions = transactions
                    .Select(t => new BorrowedBookViewModel
                    {
                        UserName = t.UserName, // Adjust according to your model
                        BorrowedCopies = t.Copies // Adjust according to your model
                    })
                    .ToList()
            };

            return View(viewModel);
        }*/
        // public async Task<IActionResult> BookDetails(Guid bookId)
        // {
        //     var transactions = await _borrowTransactionService.GetTransactionsByBookAsync(bookId);
        //     // Prepare the view model and return the view
        //     return View(transactions);
        // }
        // public async Task<IActionResult> BookDetails(Guid id)
        // {
        //     var viewModel = await _borrowTransactionService.GetBookDetailsAsync(id);

        //     if (viewModel == null)
        //     {
        //         return NotFound(); // Handle not found case
        //     }

        //     return View(viewModel);
        // }
       public async Task<IActionResult> BookDetails(Guid bookId)
{
    var book = await _bookRepository.GetBookByIdAsync(bookId);
    if (book == null)
    {
        return NotFound(); // Handle not found case
    }

    var borrowTransactions = await _context.BorrowTransactions
        .Where(bt => bt.BookId == bookId)
        .Include(bt => bt.User) // Ensure User is loaded
        .ToListAsync();

    var viewModel = new BookDetailsViewModel
    {
        BookId = book.Id,
        BookName = book.Name,
        TotalCopies = book.TotalCopies,
        AvailableCopies = book.AvailableCopies,
        Borrowers = borrowTransactions.Select(bt => new UserBookViewModel
        {
            UserName = bt.User?.UserName ?? "Unknown User", // Handle null User
            CopiesBorrowed = book.AvailableCopies//bt.Copies // Use the correct property to show borrowed count
        }).ToList()
    };

    return View(viewModel);
}






        // [HttpPost]
        // public async Task<IActionResult> Delete(int id)
        // {
        //     await _bookService.DeleteBookAsync(id);
        //     return RedirectToAction(nameof(Index));
        // }
        /*[HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string id)
        {
            Console.WriteLine("id", id);
            Console.WriteLine($"Received delete request for ID: {id}");
            if (string.IsNullOrEmpty(id))
            {
                return BadRequest(new { success = false, message = "Invalid book ID." });
            }
            try
            {
                Guid bookId = new Guid(id);
                await _bookService.DeleteBookAsync(bookId);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting book");
                Console.WriteLine(ex.Message);
                return BadRequest(new { success = false, message = " bad request Unable to delete the book." });
                //return Json(new { success = false, message = "Error deleting book. Please try again later." });
            }
        }*/
    }
}
