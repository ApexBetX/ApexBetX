using ApexBetX.Data;
using ApexBetX.Models;
using ApexBetX.Services;
using Microsoft.AspNetCore.Mvc;

namespace ApexBetX.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly TransactionService _transactionService;

        public TransactionsController(
            ApplicationDbContext context,
            TransactionService transactionService)
        {
            _context = context;
            _transactionService = transactionService;
        }

        // GET: Transactions/Create
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

        // POST: Transactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Transaction transaction)
        {
            try
            {
                var account = await _context.BettingAccounts
                    .FindAsync(transaction.AccountId);

                if (account == null)
                {
                    ModelState.AddModelError(
                        "",
                        "Betting account not found.");
                }
                else
                {
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

                    if (!_transactionService.IsValidAmount(transaction.Amount))
                    {
                        ModelState.AddModelError(
                            "Amount",
                            "Transaction amount cannot be zero.");
                    }
                }

                if (ModelState.IsValid && account != null)
                {
                    // CaptureDate is controlled by the system
                    _transactionService.SetCaptureDate(transaction);

                    // Credit increases balance
                    // Debit decreases balance
                    account.Balance =
                        _transactionService.CalculateNewBalance(
                            account.Balance,
                            transaction);

                    _context.Transactions.Add(transaction);

                    await _context.SaveChangesAsync();

                    TempData["Success"] =
                        "Transaction added successfully.";

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

        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var transaction =
                    await _context.Transactions.FindAsync(id);

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

                    account.Balance = _transactionService.ReverseTransaction(
                        account.Balance,
                        transaction);

                    transaction.TransactionDate = model.TransactionDate;
                    transaction.Amount = model.Amount;
                    transaction.TransactionType = model.TransactionType;
                    transaction.Description = model.Description;

                    _transactionService.SetCaptureDate(transaction);

                    account.Balance = _transactionService.ApplyTransaction(
                        account.Balance,
                        transaction);

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Transaction updated successfully. Previous transaction values were saved in history.";

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