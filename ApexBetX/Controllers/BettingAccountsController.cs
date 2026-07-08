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
        private readonly EmailService _emailService;


        public BettingAccountsController(ApplicationDbContext context, AccountService accountService, EmailService emailService)
        {
            _context = context;
            _accountService = accountService;
            _emailService = emailService;
        }

       
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

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var account = await _context.BettingAccounts
                    .Include(a => a.User)
                    .Include(a => a.Transactions)
                    .FirstOrDefaultAsync(a => a.AccountId == id);

                if (account == null)
                {
                    TempData["Error"] = "Betting account not found.";
                    return RedirectToAction("Index", "Users");
                }

                return View(account);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while loading account details.";
                return RedirectToAction("Index", "Users");
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var account = await _context.BettingAccounts.FindAsync(id);

                if (account == null)
                {
                    TempData["Error"] = "Betting account not found.";
                    return RedirectToAction("Index", "Users");
                }

                return View(account);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while loading the account.";
                return RedirectToAction("Index", "Users");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, BettingAccount model)
        {
            try
            {
                if (id != model.AccountId)
                {
                    return NotFound();
                }

                ModelState.Remove("User");
                ModelState.Remove("Transactions");

                var account = await _context.BettingAccounts
                    .Include(a => a.User)
                        .ThenInclude(u => u.Account)
                    .FirstOrDefaultAsync(a => a.AccountId == id);

                if (account == null)
                {
                    TempData["Error"] = "Betting account not found.";
                    return RedirectToAction("Index", "Users");
                }

                if (account.User == null || account.User.Account == null)
                {
                    TempData["Error"] = "The account owner does not have a linked login account.";
                    return RedirectToAction("Details", new { id = account.AccountId });
                }

                if (model.IsClosed && !_accountService.CanCloseAccount(account))
                {
                    ModelState.AddModelError(
                        "IsClosed",
                        "An account can only be closed when the balance is zero.");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var token = Random.Shared.Next(100000, 1000000).ToString();

                TempData["AccountEditData"] = System.Text.Json.JsonSerializer.Serialize(model);
                TempData["AccountEditToken"] = token;
                TempData["AccountEditTokenExpiry"] = DateTime.Now.AddMinutes(10).ToString("O");

                await _emailService.SendEmailAsync(
                    account.User.Account.Email,
                    "ApexBetX Betting Account Update Verification",
                                $"""
                        A request has been made to update your ApexBetX betting account.

                        Account Number: {account.AccountNumber}

                        Verification code: {token}

                        This code expires in 10 minutes.
                        """);

                TempData["Success"] = "A verification code was sent to the account owner's email.";

                return RedirectToAction("VerifyEdit");
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while preparing the account update.";
                return View(model);
            }
        }
    }
}
