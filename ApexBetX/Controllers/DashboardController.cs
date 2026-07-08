using ApexBetX.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace ApexBetX.Controllers
{
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");
            var role = HttpContext.Session.GetString("Role");

            if (accountId == null)
            {
                TempData["Error"] = "Please login to access your account.";

                return RedirectToAction("Login", "Account");
            }

            if (role == "Admin")
            {
                return RedirectToAction("Index", "Users");
            }

            var account = await _context.Accounts
                .Include(a => a.User)
                    .ThenInclude(u => u!.BettingAccounts)
                        .ThenInclude(b => b.Transactions)
                .FirstOrDefaultAsync(a => a.AccountId == accountId);

            if (account == null)
            {
                HttpContext.Session.Clear();

                TempData["Error"] = "Account not found.";

                return RedirectToAction("Login", "Account");
            }

            if (account.User == null)
            {
                TempData["Error"] =
                    "Your account is not linked to a user profile.";

                return RedirectToAction("Index", "Home");
            }

            return View(account);
        }

        public async Task<IActionResult> Transactions(int id)
        {
            var accountId = HttpContext.Session.GetInt32("AccountId");

            if (accountId == null)
            {
                TempData["Error"] = "Please login to view transactions.";
                return RedirectToAction("Login", "Account");
            }

            var bettingAccount = await _context.BettingAccounts
                .Include(b => b.Transactions)
                .Include(b => b.User)
                .ThenInclude(u => u!.Account)
                .FirstOrDefaultAsync(b =>
                    b.AccountId == id &&
                    b.User != null &&
                    b.User.AccountId == accountId);

            if (bettingAccount == null)
            {
                TempData["Error"] = "You are not allowed to view this account.";
                return RedirectToAction("Index");
            }

            return View(bettingAccount);
        }
    }
}
