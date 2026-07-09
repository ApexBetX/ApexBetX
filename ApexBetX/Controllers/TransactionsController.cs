using ApexBetX.Data;
using ApexBetX.Models;
using ApexBetX.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApexBetX.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TransactionService _transactionService;

        public TransactionsController(ApplicationDbContext context, TransactionService transactionService)
        {
            _context = context;
            _transactionService = transactionService;
        }

        public async Task<IActionResult> Create(int accountId)
        {
            try
            {
                var account = await _context.BettingAccounts.FindAsync(accountId);

                if (account == null)
                {
                    TempData["Error"] = "Betting account not found.";
                    return RedirectToAction("Index", "Users");
                }

                if (!_transactionService.CanAddTransaction(account))
                {
                    TempData["Error"] =
                        "Transactions cannot be added to a closed account.";

                    return RedirectToAction(
                        "Details",
                        "BettingAccounts",
                        new { id = accountId });
                }

                var transaction = new Transaction
                {
                    AccountId = accountId,
                    TransactionDate = DateTime.Today
                };

                return View(transaction);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "An error occurred while loading the transaction form.";

                return RedirectToAction("Index", "Users");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Transaction transaction)
        {
            try
            {
                ModelState.Remove("BettingAccount");

                var role = HttpContext.Session.GetString("Role");
                var loginAccountId = HttpContext.Session.GetInt32("AccountId");

                if (loginAccountId == null)
                {
                    TempData["Error"] = "Please login to add a transaction.";

                    return RedirectToAction("Login", "Account");
                }

                var account = await _context.BettingAccounts
                    .Include(b => b.User)
                    .FirstOrDefaultAsync(b =>
                        b.AccountId == transaction.AccountId);

                if (account == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Betting account not found.");
                }
                else
                {
                    if (role == "Admin" && transaction.TransactionType == "Withdrawal")
                    {
                        ModelState.AddModelError(
                            "TransactionType",
                            "Admin users cannot make withdrawals from client accounts.");
                    }


                    if (role == "User")
                    {
                        if (account.User == null ||
                            account.User.AccountId != loginAccountId)
                        {
                            TempData["Error"] =
                                "You are not allowed to add transactions to this account.";

                            return RedirectToAction(
                                "Index",
                                "Dashboard");
                        }
                    }

                    if (!_transactionService.CanAddTransaction(account))
                    {
                        ModelState.AddModelError(
                            "",
                            "Transactions cannot be added to a closed account.");
                    }

                    if (!_transactionService.IsValidTransactionDate(
                        transaction.TransactionDate))
                    {
                        ModelState.AddModelError(
                            "TransactionDate",
                            "Transaction date cannot be in the future.");
                    }

                    if (!_transactionService.IsValidAmount(
                        transaction.Amount))
                    {
                        ModelState.AddModelError(
                            "Amount",
                            "Transaction amount cannot be zero.");
                    }

                    if (!_transactionService.HasEnoughBalance(account, transaction))
                    {
                        ModelState.AddModelError(
                            "Amount",
                            "Withdrawal amount cannot be greater than the available balance.");
                    }
                }

                if (ModelState.IsValid && account != null)
                {
                    _transactionService.SetCaptureDate(transaction);

                    account.Balance =
                        _transactionService.CalculateNewBalance(
                            account.Balance,
                            transaction);

                    _context.Transactions.Add(transaction);

                    await _context.SaveChangesAsync();

                    TempData["Success"] =
                        "Transaction added successfully.";

                    if (role == "User")
                    {
                        return RedirectToAction(
                            "Transactions",
                            "Dashboard",
                            new { id = transaction.AccountId });
                    }


                    return RedirectToAction(
                        "Details",
                        "BettingAccounts",
                        new { id = transaction.AccountId });
                }

                return View(transaction);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "An error occurred while saving the transaction.";

                return View(transaction);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var transaction = await _context.Transactions.FindAsync(id);


                if (transaction == null)
                {
                    TempData["Error"] = "Transaction not found.";

                    return RedirectToAction("Index", "Users");
                }

                var account =
                    await _context.BettingAccounts
                        .FindAsync(transaction.AccountId);

                if (account == null)
                {
                    TempData["Error"] =
                        "Betting account not found.";

                    return RedirectToAction("Index", "Users");
                }

                if (!_transactionService.CanAddTransaction(account))
                {
                    TempData["Error"] =
                        "Transactions on a closed account cannot be edited.";

                    return RedirectToAction(
                        "Details",
                        "BettingAccounts",
                        new { id = account.AccountId });
                }


                return View(transaction);
            }
            catch (Exception)
            {
                TempData["Error"] =
                    "An error occurred while loading the transaction.";

                return RedirectToAction("Index", "Users");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Transaction model)
        {
            try
            {
                ModelState.Remove("BettingAccount");

                if (id != model.TransactionId)
                    return NotFound();

                var transaction = await _context.Transactions.FindAsync(id);

                if (transaction == null)
                {
                    TempData["Error"] = "Transaction not found.";
                    return RedirectToAction("Index", "Users");
                }

                var account = await _context.BettingAccounts
                    .FindAsync(transaction.AccountId);

                if (account == null)
                {
                    TempData["Error"] = "Betting account not found.";
                    return RedirectToAction("Index", "Users");
                }

                if (!_transactionService.CanAddTransaction(account))
                {
                    TempData["Error"] = "Transactions on a closed account cannot be edited.";
                    return RedirectToAction("Details", "BettingAccounts",
                        new { id = account.AccountId });
                }

                if (!_transactionService.IsValidTransactionDate(model.TransactionDate))
                {
                    ModelState.AddModelError("TransactionDate",
                        "Transaction date cannot be in the future.");
                }

                if (!_transactionService.IsValidAmount(model.Amount))
                {
                    ModelState.AddModelError("Amount",
                        "Transaction amount cannot be zero.");
                }

                var balanceAfterReverse = _transactionService.ReverseTransaction(
                    account.Balance,
                    transaction);

                if (model.TransactionType == "Withdrawal" &&
                    model.Amount > balanceAfterReverse)
                {
                    ModelState.AddModelError(
                        "Amount",
                        "Withdrawal amount cannot be greater than the available balance.");
                }

                if (ModelState.IsValid)
                {
                    var history = new TransactionHistory
                    {
                        TransactionId = transaction.TransactionId,
                        AccountId = transaction.AccountId,
                        OldTransactionDate = transaction.TransactionDate,
                        OldAmount = transaction.Amount,
                        OldTransactionType = transaction.TransactionType,
                        OldDescription = transaction.Description,
                        EditedDate = DateTime.Now
                    };

                    _context.TransactionHistories.Add(history);

                    account.Balance = balanceAfterReverse;

                    transaction.TransactionDate = model.TransactionDate;
                    transaction.Amount = model.Amount;
                    transaction.TransactionType = model.TransactionType;
                    transaction.Description = model.Description;

                    _transactionService.SetCaptureDate(transaction);

                    account.Balance = _transactionService.ApplyTransaction(
                        account.Balance,
                        transaction);

                    await _context.SaveChangesAsync();

                    TempData["Success"] =
                        "Transaction updated successfully. Previous transaction values were saved in history.";

                    return RedirectToAction("Details", "BettingAccounts",
                        new { id = transaction.AccountId });
                }

                return View(model);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while updating the transaction.";
                return View(model);
            }
        }
    }
}