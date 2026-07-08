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
        private readonly EmailService _emailService;

        public UsersController(ApplicationDbContext context, UserService userService, EmailService emailService)
        {
            _context = context;
            _userService = userService;
            _emailService = emailService;

        }
        public async Task<IActionResult> Index(string? searchTerm, int page = 1)
        {
            try
            {
                int pageSize = 10;

                ViewBag.SearchTerm = searchTerm;
                ViewBag.CurrentPage = page;

                var users = _context.Users
                    .Include(u => u.Account)
                    .Include(u => u.BettingAccounts)
                    .Where(u => !u.IsArchived &&
                                u.Account != null &&
                                u.Account.IsEmailVerified)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    users = users.Where(u =>
                        u.IDNumber!.Contains(searchTerm) ||
                        u.Surname!.Contains(searchTerm) ||
                        u.BettingAccounts.Any(a => a.AccountNumber!.Contains(searchTerm)));
                }

                int totalUsers = await users.CountAsync();
                int totalPages = (int)Math.Ceiling(totalUsers / (double)pageSize);

                ViewBag.TotalPages = totalPages;

                var result = await users
                    .OrderBy(u => u.Surname)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
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
                ModelState.Remove("Account");
                ModelState.Remove("BettingAccounts");

                // Check duplicate ID Number
                if (await _userService.IDNumberExistsAsync(user.IDNumber!))
                {
                    ModelState.AddModelError(
                        "IDNumber",
                        "A user with this ID Number already exists.");
                }

                // Check whether the email is already linked to another User
                var existingAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Email == user.Email);

                if (existingAccount != null)
                {
                    var accountAlreadyLinked = await _context.Users
                        .AnyAsync(u => u.AccountId == existingAccount.AccountId);

                    if (accountAlreadyLinked)
                    {
                        ModelState.AddModelError(
                            "Email",
                            "This login account is already linked to another user.");
                    }
                }

                if (!ModelState.IsValid)
                {
                    return View(user);
                }

                Account account;
                string? verificationToken = null;

                // If the Account already exists, link it
                if (existingAccount != null)
                {
                    account = existingAccount;
                }
                else
                {
                    // Generate verification token
                    verificationToken =
                        Random.Shared.Next(100000, 1000000).ToString();

                    // Create login Account
                    account = new Account
                    {
                        Email = user.Email!,
                        Role = "User",

                        IsEmailVerified = false,

                        EmailVerificationToken = verificationToken,

                        EmailVerificationTokenExpiry =
                            DateTime.Now.AddMinutes(10),
                        Password = BCrypt.Net.BCrypt.HashPassword(
                            Guid.NewGuid().ToString())
                    };

                    _context.Accounts.Add(account);

                    // Save first so AccountId is generated
                    await _context.SaveChangesAsync();
                }

                // Link User to Account
                user.AccountId = account.AccountId;

                _context.Users.Add(user);

                await _context.SaveChangesAsync();

                // Only send verification email for newly-created Accounts
                if (verificationToken != null)
                {
                    try
                    {
                        await _emailService.SendEmailAsync(
                            account.Email,
                            "Verify Your ApexBetX Account",
                            $"""
                            An ApexBetX account has been created for you.

                            Your verification code is:

                            {verificationToken}

                            This verification code expires in 10 minutes.

                            Please verify your email to activate your account.
                            """);
                    }
                    catch (Exception)
                    {
                        TempData["Error"] =
                            "The user was created, but the verification email could not be sent.";

                        return RedirectToAction(nameof(Index));
                    }
                }

                TempData["Success"] =
                    "User created successfully. A verification code was sent to the user's email.";

                return RedirectToAction("VerifyToken", "Account", new { email = account.Email });
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "An error occurred while creating the user. Please try again.";

                return View(user);
            }
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.BettingAccounts)
                    .FirstOrDefaultAsync(u => u.UserId == id && !u.IsArchived);

                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                return View(user);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while loading user details.";
                return RedirectToAction(nameof(Index));
            }
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
        public async Task<IActionResult> Archive(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.BettingAccounts)
                    .FirstOrDefaultAsync(u => u.UserId == id);

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

        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.BettingAccounts)
                    .FirstOrDefaultAsync(u => u.UserId == id);

                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction(nameof(Index));
                }

                if (!_userService.CanArchiveUser(user))
                {
                    TempData["Error"] = "This user cannot be archived because they have open betting accounts.";
                    return RedirectToAction(nameof(Index));
                }

                user.IsArchived = true;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                TempData["Success"] = "User archived successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while archiving the user.";
                return RedirectToAction(nameof(Index));
            }
        }
        public async Task<IActionResult> ArchivedUsers()
        {
            try
            {
                var archivedUsers = await _context.Users
                    .Include(u => u.BettingAccounts)
                    .Where(u => u.IsArchived)
                    .OrderBy(u => u.Surname)
                    .ToListAsync();

                return View(archivedUsers);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while loading archived users.";
                return RedirectToAction(nameof(Index));
            }
        }
        public async Task<IActionResult> ArchiveRestore(int id)
        {
            var user = await _context.Users.FindAsync(id);

            if (user == null)
            {
                TempData["Error"] = "User not found.";
                return RedirectToAction(nameof(ArchivedUsers));
            }

            user.IsArchived = false;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            TempData["Success"] = "User restored successfully.";
            return RedirectToAction(nameof(ArchivedUsers));
        }
    }
}