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
                var user = await _context.Users
                    .Include(u => u.Account)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    TempData["Error"] = "User not found.";
                    return RedirectToAction("Index", "Users");
                }

                if (user.Account == null || !user.Account.IsEmailVerified)
                {
                    TempData["Error"] = "The user must verify their account before a betting account can be created.";
                    
        return RedirectToAction(
            "VerifyToken",
            "Account",
            new { email = user.Email });
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
                var user = await _context.Users
                    .Include(u => u.Account)
                    .FirstOrDefaultAsync(u => u.UserId == account.UserId);

                if (user == null)
                {
                    ModelState.AddModelError("", "User not found.");
                }
                else if (user.Account == null || !user.Account.IsEmailVerified)
                {
                    ModelState.AddModelError("", "The user must verify their account before a betting account can be created.");
                }

                if (await _accountService.AccountNumberExistsAsync(account.AccountNumber!))
                {
                    ModelState.AddModelError("AccountNumber", "An account with this account number already exists.");
                }

                if (!ModelState.IsValid)
                {
                    return View(account);
                }

                account.CreatedDate = DateTime.Now;
                account.Balance = 0;
                account.IsClosed = false;

                var token = Random.Shared.Next(100000, 1000000).ToString();

                TempData["PendingBettingAccount"] =
                    System.Text.Json.JsonSerializer.Serialize(account);

                TempData["BettingAccountCreateToken"] = token;

                TempData["BettingAccountCreateTokenExpiry"] =
                    DateTime.Now.AddMinutes(10).ToString("O");

                await _emailService.SendEmailAsync(
                    user.Account.Email,
                    "ApexBetX Betting Account Creation Verification",
                    $"""
                    A request has been made to create a betting account for you.

                    Account Number: {account.AccountNumber}

                    Verification code: {token}

                    This code expires in 10 minutes.
                    """);

                TempData["Success"] = "A verification code was sent to the user's email.";

                return RedirectToAction("VerifyCreate");
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while preparing the betting account creation.";
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
        public IActionResult VerifyCreate()
        {
            TempData.Keep("PendingBettingAccount");
            TempData.Keep("BettingAccountCreateToken");
            TempData.Keep("BettingAccountCreateTokenExpiry");

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyCreate(string token)
        {
            TempData.Keep("PendingBettingAccount");
            TempData.Keep("BettingAccountCreateToken");
            TempData.Keep("BettingAccountCreateTokenExpiry");

            var savedData = TempData["PendingBettingAccount"]?.ToString();
            var savedToken = TempData["BettingAccountCreateToken"]?.ToString();
            var expiryText = TempData["BettingAccountCreateTokenExpiry"]?.ToString();

            if (savedData == null || savedToken == null || expiryText == null)
            {
                TempData["Error"] = "Verification session expired.";
                return RedirectToAction("Index", "Users");
            }

            if (DateTime.Parse(expiryText) < DateTime.Now)
            {
                TempData["Error"] = "Verification code has expired.";
                return RedirectToAction("Index", "Users");
            }

            if (token != savedToken)
            {
                ModelState.AddModelError("token", "Invalid verification code.");
                return View();
            }

            var account = System.Text.Json.JsonSerializer
                .Deserialize<BettingAccount>(savedData);

            if (account == null)
            {
                TempData["Error"] = "Pending betting account data could not be loaded.";
                return RedirectToAction("Index", "Users");
            }

            _context.BettingAccounts.Add(account);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Betting account created successfully after verification.";

            return RedirectToAction("Details", "Users", new { id = account.UserId });
        }
    }
}
