using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using LibraryManagement.Models;
using LibraryManagement.Services;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.AspNetCore.Identity;

namespace LibraryManagement.Controllers
{
    //[Authorize]
    public class BooksController : Controller
    {
        private readonly IBookService _bookService;
        private readonly ILogger<BooksController> _logger;
        private readonly UserManager<IdentityUser> _userManager;

        public BooksController(IBookService bookService, ILogger<BooksController> logger, UserManager<IdentityUser> userManager)
        {
            _bookService = bookService;
            _logger = logger;
            _userManager = userManager;
            
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

[HttpPost]
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
}





        public IActionResult MyBooks()
        {
            // Implement logic to display user's borrowed books
            return View();
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
