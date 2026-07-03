using ApexBetX.Data;
using ApexBetX.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApexBetX.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UsersController(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IActionResult> Index(string? searchTerm)
        {
            try
            {
                ViewBag.SearchTerm = searchTerm;

                var users = _context.Users
                    .Include(u => u.BettingAccounts)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    users = users.Where(u =>
                        u.IDNumber.Contains(searchTerm) ||
                        u.Surname.Contains(searchTerm) ||
                        u.BettingAccounts.Any(a => a.AccountNumber.Contains(searchTerm)));
                }

                var result = await users
                    .OrderBy(u => u.Surname)
                    .ToListAsync();

                return View(result);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while retrieving users. Please try again.";
                return View(new List<User>());
            }
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Users.Add(user);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "User created successfully.";
                    return RedirectToAction(nameof(Index));
                }

                return View(user);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while creating the user. Please try again.";
                return View(user);
            }
        }

        public IActionResult Details(int id)
        {
            return View();
        }

        public IActionResult Edit(int id)
        {
            return View();
        }

        public IActionResult Delete(int id)
        {
            return View();
        }
    }
}