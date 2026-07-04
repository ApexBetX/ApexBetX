using ApexBetX.Data;
using ApexBetX.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ApexBetX.Controllers
{
    public class TransactionsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Transactions/Create
        public async Task<IActionResult> Create(int accountId)
        {
            var account = await _context.BettingAccounts.FindAsync(accountId);

            if (account == null)
            {
                TempData["Error"] = "Account not found.";
                return RedirectToAction("Index", "Users");
            }

            var transaction = new Transaction
            {
                AccountId = accountId,
                TransactionDate = DateTime.Today
            };

            return View(transaction);
        }

        // POST: Transactions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Transaction transaction)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    transaction.CaptureDate = DateTime.Now;

                    _context.Transactions.Add(transaction);
                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Transaction added successfully.";

                    return RedirectToAction("Details", "BettingAccounts",
                        new { id = transaction.AccountId });
                }

                return View(transaction);
            }
            catch (Exception)
            {
                TempData["Error"] = "An error occurred while saving the transaction.";
                return View(transaction);
            }
        }

        // GET: Transactions/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);

            if (transaction == null)
            {
                TempData["Error"] = "Transaction not found.";
                return RedirectToAction("Index", "Users");
            }

            return View(transaction);
        }

        // POST: Transactions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Transaction model)
        {
            try
            {
                if (id != model.TransactionId)
                {
                    return NotFound();
                }

                if (ModelState.IsValid)
                {
                    var transaction = await _context.Transactions.FindAsync(id);

                    if (transaction == null)
                    {
                        TempData["Error"] = "Transaction not found.";
                        return RedirectToAction("Index", "Users");
                    }

                    transaction.TransactionDate = model.TransactionDate;
                    transaction.Amount = model.Amount;
                    transaction.TransactionType = model.TransactionType;
                    transaction.Description = model.Description;

                    // Capture date is system-generated
                    transaction.CaptureDate = DateTime.Now;

                    await _context.SaveChangesAsync();

                    TempData["Success"] = "Transaction updated successfully.";

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