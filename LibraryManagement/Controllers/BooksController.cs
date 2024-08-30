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
    public class BooksController : Controller
    {
        private readonly IBookService _bookService;
        private readonly IBookRepository _bookRepository;
        private readonly ILogger<BooksController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IBorrowTransactionService _borrowTransactionService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public BooksController(IWebHostEnvironment webHostEnvironment,ApplicationDbContext context,IBookRepository bookRepository, IBookService bookService, IBorrowTransactionService borrowTransactionService, ILogger<BooksController> logger, UserManager<IdentityUser> userManager)
        {
            _bookService = bookService;
            _context = context;
            _bookRepository = bookRepository;
            _webHostEnvironment = webHostEnvironment;
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
            var model = new BookCreateViewModel();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var book = new Book
                {
                    Name = model.Name,
                    TotalCopies = model.TotalCopies,
                    AvailableCopies = model.TotalCopies
                };

                try
                {
                    if (model.ImageFile != null && model.ImageFile.Length > 0)
                    {
                        var imagesDirectory = Path.Combine(_webHostEnvironment.WebRootPath, "images");

                        if (!Directory.Exists(imagesDirectory))
                        {
                            Directory.CreateDirectory(imagesDirectory);
                        }

                        var fileName = Path.GetFileName(model.ImageFile.FileName);
                        var filePath = Path.Combine(imagesDirectory, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await model.ImageFile.CopyToAsync(stream);
                        }
                        book.ImagePath = $"/images/{fileName}";
                    }

                    await _bookService.AddBookAsync(book);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating book");
                    ModelState.AddModelError("", "Unable to save changes. Please try again later.");
                }
            }

            return View(model);
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
                    var existingBook = await _bookService.GetBookByIdAsync(id);
                    if (existingBook == null)
                    {
                        return NotFound();
                    }

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

                return RedirectToAction(nameof(MyBooks)); 
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error returning the book");
                return StatusCode(500, "Internal server error");
            }
        }

        public async Task<IActionResult> BookDetails(Guid bookId)
        {
            var book = await _bookRepository.GetBookByIdAsync(bookId);
            if (book == null)
            {
                return NotFound(); 
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
                Borrowers = borrowTransactions.Select(bt => new UserBookViewModel
                {
                    UserName = bt.User?.UserName ?? "Unknown User", 
                    CopiesBorrowed = bt.Copies 
                }).ToList()
            };

            return View(viewModel);
        }


    }
}
