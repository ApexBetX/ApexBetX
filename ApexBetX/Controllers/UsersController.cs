using ApexBetX.Data;
using ApexBetX.Models;
using ApexBetX.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApexBetX.Controllers
{
    public class UsersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserService _userService;

        public UsersController(ApplicationDbContext context, UserService userService)
        {
            _context = context;
            _userService = userService;
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User user)
        {
            try
            {
                if (await _userService.IDNumberExistsAsync(user.IDNumber!))
                {
                    ModelState.AddModelError("IDNumber", "A user with this ID Number already exists.");
                }

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

        // GET: Users/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(user);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while loading the user.";
                return RedirectToAction(nameof(Index));
            }
        }
        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, User user)
        {
            try
            {
                if (id != user.UserId)
                {
                    TempData["Error"] = "Invalid user selected.";
                    return RedirectToAction(nameof(Index));
                }

                if (await _userService.DuplicateIDNumberExistsAsync(user.UserId, user.IDNumber!))
                {
                    ModelState.AddModelError("IDNumber",
                        "A user with this ID Number already exists.");
                }

                if (ModelState.IsValid)
                {
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "User updated successfully.";
                    return RedirectToAction(nameof(Index));
                }

                return View(user);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while updating the user.";
                return View(user);
            }
        }
        public IActionResult Delete(int id)
        {
            return View();
        }
    }
}