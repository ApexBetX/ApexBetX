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
            catch (Exception ex)
            {
                // Log the error (optional)
                // _logger.LogError(ex, "An error occurred while retrieving users.");

                TempData["Error"] = "An error occurred while retrieving users. Please try again.";

                return View(new List<User>());
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
