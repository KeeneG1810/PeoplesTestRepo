using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeopleAccountsManager.Models;

namespace PeopleAccountsManager.Controllers
{

    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<TransactionsController> _logger;

        public TransactionsController(AppDbContext context, ILogger<TransactionsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? accountId)
        {
            if (accountId == null)
            {
                return NotFound();
            }

            try
            {
                var account = await _context.Accounts
                    .Include(a => a.Person)
                    .FirstOrDefaultAsync(a => a.Code == accountId);

                if (account == null)
                {
                    return NotFound();
                }

                ViewData["AccountNumber"] = account.AccountNumber;
                ViewData["PersonName"] = account.Person?.FullName ?? "Unknown";
                ViewData["AccountId"] = accountId;
                ViewData["OutstandingBalance"] = account.OutstandingBalance;

                var transactions = await _context.Transactions
                    .Include(t => t.Account)
                    .Where(t => t.AccountCode == accountId)
                    .OrderByDescending(t => t.TransactionDate)
                    .ThenByDescending(t => t.CaptureDate)
                    .ToListAsync();

                return View(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transactions for account {AccountId}", accountId);
                TempData["ErrorMessage"] = "An error occurred while loading the transactions.";
                return RedirectToAction("Index", "Persons");
            }
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var transaction = await _context.Transactions
                    .Include(t => t.Account)
                    .ThenInclude(a => a!.Person)
                    .FirstOrDefaultAsync(m => m.Code == id);

                if (transaction == null)
                {
                    return NotFound();
                }

                return View(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving transaction details for ID {TransactionId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading transaction details.";
                return RedirectToAction("Index", "Persons");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? accountId)
        {
            if (accountId == null)
            {
                return NotFound();
            }

            try
            {
                var account = await _context.Accounts
                    .Include(a => a.Person)
                    .FirstOrDefaultAsync(a => a.Code == accountId);

                if (account == null)
                {
                    return NotFound();
                }

                if (account.IsClosed)
                {
                    TempData["ErrorMessage"] = "Cannot create transactions on a closed account.";
                    return RedirectToAction(nameof(Index), new { accountId });
                }

                ViewData["AccountNumber"] = account.AccountNumber;
                ViewData["PersonName"] = account.Person?.FullName ?? "Unknown";
                ViewData["AccountId"] = accountId;

                var transaction = new Transaction
                {
                    AccountCode = accountId.Value,
                    TransactionDate = DateTime.Today 
                };

                return View(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create transaction form");
                TempData["ErrorMessage"] = "An error occurred while loading the form.";
                return RedirectToAction("Index", "Persons");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("AccountCode,TransactionDate,Amount,Description")] Transaction transaction)
        {
            if (transaction.Amount == 0)
            {
                ModelState.AddModelError("Amount", "Amount cannot be zero.");
            }

            if (transaction.TransactionDate > DateTime.Today)
            {
                ModelState.AddModelError("TransactionDate", "Transaction date cannot be in the future.");
            }

            var account = await _context.Accounts.FindAsync(transaction.AccountCode);
            if (account == null)
            {
                ModelState.AddModelError("AccountCode", "Invalid account selected.");
            }
            else if (account.IsClosed)
            {
                ModelState.AddModelError(string.Empty, "Cannot create transactions on a closed account.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    transaction.CaptureDate = DateTime.Now;

                    _context.Add(transaction);

                    account!.OutstandingBalance += transaction.Amount;
                    _context.Update(account);

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Transaction created for account {AccountCode}, Amount: {Amount}", transaction.AccountCode, transaction.Amount);
                    TempData["SuccessMessage"] = "Transaction created successfully.";
                    return RedirectToAction(nameof(Index), new { accountId = transaction.AccountCode });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating transaction");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the transaction.");
                }
            }

            var acc = await _context.Accounts.Include(a => a.Person).FirstOrDefaultAsync(a => a.Code == transaction.AccountCode);
            ViewData["AccountNumber"] = acc?.AccountNumber ?? "Unknown";
            ViewData["PersonName"] = acc?.Person?.FullName ?? "Unknown";
            ViewData["AccountId"] = transaction.AccountCode;
            return View(transaction);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var transaction = await _context.Transactions
                    .Include(t => t.Account)
                    .ThenInclude(a => a!.Person)
                    .FirstOrDefaultAsync(t => t.Code == id);

                if (transaction == null)
                {
                    return NotFound();
                }

                if (transaction.Account?.IsClosed == true)
                {
                    TempData["ErrorMessage"] = "Cannot edit transactions on a closed account.";
                    return RedirectToAction(nameof(Index), new { accountId = transaction.AccountCode });
                }

                ViewData["AccountNumber"] = transaction.Account?.AccountNumber ?? "Unknown";
                ViewData["PersonName"] = transaction.Account?.Person?.FullName ?? "Unknown";
                ViewData["OriginalAmount"] = transaction.Amount; 
                return View(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading transaction for edit, ID {TransactionId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the transaction.";
                return RedirectToAction("Index", "Persons");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Code,AccountCode,TransactionDate,Amount,Description")] Transaction transaction)
        {
            if (id != transaction.Code)
            {
                return NotFound();
            }

            if (transaction.Amount == 0)
            {
                ModelState.AddModelError("Amount", "Amount cannot be zero.");
            }

            if (transaction.TransactionDate > DateTime.Today)
            {
                ModelState.AddModelError("TransactionDate", "Transaction date cannot be in the future.");
            }

            var originalTransaction = await _context.Transactions
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Code == id);

            if (originalTransaction == null)
            {
                return NotFound();
            }

            var account = await _context.Accounts.FindAsync(transaction.AccountCode);
            if (account == null)
            {
                ModelState.AddModelError("AccountCode", "Invalid account selected.");
            }
            else if (account.IsClosed)
            {
                ModelState.AddModelError(string.Empty, "Cannot edit transactions on a closed account.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    transaction.CaptureDate = DateTime.Now;

                    _context.Update(transaction);

                    decimal balanceAdjustment = transaction.Amount - originalTransaction.Amount;
                    account!.OutstandingBalance += balanceAdjustment;
                    _context.Update(account);

                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Transaction updated: {TransactionCode}, Balance adjustment: {Adjustment}", transaction.Code, balanceAdjustment);
                    TempData["SuccessMessage"] = "Transaction updated successfully.";
                    return RedirectToAction(nameof(Index), new { accountId = transaction.AccountCode });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!await TransactionExists(transaction.Code))
                    {
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency error updating transaction");
                        ModelState.AddModelError(string.Empty, "The transaction was updated by another user. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating transaction");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the transaction.");
                }
            }

            var acc = await _context.Accounts.Include(a => a.Person).FirstOrDefaultAsync(a => a.Code == transaction.AccountCode);
            ViewData["AccountNumber"] = acc?.AccountNumber ?? "Unknown";
            ViewData["PersonName"] = acc?.Person?.FullName ?? "Unknown";
            ViewData["OriginalAmount"] = originalTransaction?.Amount ?? 0;
            return View(transaction);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            try
            {
                var transaction = await _context.Transactions
                    .Include(t => t.Account)
                    .ThenInclude(a => a!.Person)
                    .FirstOrDefaultAsync(m => m.Code == id);

                if (transaction == null)
                {
                    return NotFound();
                }

                return View(transaction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading transaction for delete, ID {TransactionId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the transaction.";
                return RedirectToAction("Index", "Persons");
            }
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var transaction = await _context.Transactions
                    .Include(t => t.Account)
                    .FirstOrDefaultAsync(t => t.Code == id);

                if (transaction == null)
                {
                    return NotFound();
                }

                var accountId = transaction.AccountCode;
                var account = transaction.Account;

                if (account != null)
                {
                    account.OutstandingBalance -= transaction.Amount;
                    _context.Update(account);
                }

                _context.Transactions.Remove(transaction);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Transaction deleted: {TransactionCode}, Amount reversed: {Amount}", transaction.Code, transaction.Amount);
                TempData["SuccessMessage"] = "Transaction deleted successfully.";
                return RedirectToAction(nameof(Index), new { accountId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting transaction");
                TempData["ErrorMessage"] = "An error occurred while deleting the transaction.";
                return RedirectToAction("Index", "Persons");
            }
        }

        private async Task<bool> TransactionExists(int id)
        {
            return await _context.Transactions.AnyAsync(e => e.Code == id);
        }
    }
}
