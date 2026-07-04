using ApexBetX.Data;
using ApexBetX.Models;
using ApexBetX.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApexBetX.Controllers
{
    public class BettingAccountsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AccountService _accountService;

        public BettingAccountsController(ApplicationDbContext context, AccountService accountService)
        {
            _context = context;
            _accountService = accountService;
        }

        // GET: BettingAccounts/Create
        public async Task<IActionResult> Create(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    TempData["Error"] = "The selected user does not exist.";
                    return RedirectToAction("Index", "Users");
                }

                var account = new BettingAccount
                {
                    UserId = userId,
                    CreatedDate = DateTime.Now,
                    Balance = 0,
                    IsClosed = false
                };

                return View(account);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while loading the page.";
                return RedirectToAction("Index", "Users");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BettingAccount account)
        {
            try
            {
                if (!_context.Users.Any(u => u.UserId == account.UserId))
                {
                    ModelState.AddModelError("", "User does not exist.");
                }

                if (await _accountService.AccountNumberExistsAsync(account.AccountNumber!))
                {
                    ModelState.AddModelError(
                        "AccountNumber",
                        "An account with this account number already exists.");
                }

                if (ModelState.IsValid)
                {
                    account.CreatedDate = DateTime.Now;
                    account.Balance = 0;
                    account.IsClosed = false;

                    _context.BettingAccounts.Add(account);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Betting account created successfully.";

                    return RedirectToAction("Details", "Users",
                        new { id = account.UserId });
                }

                return View(account);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while creating the betting account.";
                return View(account);
            }
        }
    }
}
