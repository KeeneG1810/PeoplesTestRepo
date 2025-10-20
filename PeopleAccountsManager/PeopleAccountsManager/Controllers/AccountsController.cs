using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PeopleAccountsManager.Models;

namespace PeopleAccountsManager.Controllers
{

    [Authorize]
    public class AccountsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AccountsController> _logger;

        public AccountsController(AppDbContext context, ILogger<AccountsController> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<IActionResult> Index(int? personId)
        {
            if (personId == null)
            {
                return NotFound();
            }

            try
            {
                var person = await _context.Persons.FindAsync(personId);
                if (person == null)
                {
                    return NotFound();
                }

                ViewData["PersonName"] = person.FullName;
                ViewData["PersonId"] = personId;

                var accounts = await _context.Accounts
                    .Include(a => a.Person)
                    .Where(a => a.PersonCode == personId)
                    .OrderBy(a => a.AccountNumber)
                    .ToListAsync();

                return View(accounts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving accounts for person {PersonId}", personId);
                TempData["ErrorMessage"] = "An error occurred while loading the accounts.";
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
                var account = await _context.Accounts
                    .Include(a => a.Person)
                    .Include(a => a.Transactions)
                    .FirstOrDefaultAsync(m => m.Code == id);

                if (account == null)
                {
                    return NotFound();
                }

                return View(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving account details for ID {AccountId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading account details.";
                return RedirectToAction("Index", "Persons");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Create(int? personId)
        {
            if (personId == null)
            {
                return NotFound();
            }

            try
            {
                var person = await _context.Persons.FindAsync(personId);
                if (person == null)
                {
                    return NotFound();
                }

                ViewData["PersonName"] = person.FullName;
                ViewData["PersonId"] = personId;

                var account = new Account
                {
                    PersonCode = personId.Value,
                    OutstandingBalance = 0 
                };

                return View(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading create account form");
                TempData["ErrorMessage"] = "An error occurred while loading the form.";
                return RedirectToAction("Index", "Persons");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PersonCode,AccountNumber,IsClosed")] Account account)
        {
            if (await _context.Accounts.AnyAsync(a => a.AccountNumber == account.AccountNumber))
            {
                ModelState.AddModelError("AccountNumber", "An account with this number already exists.");
            }

            if (!await _context.Persons.AnyAsync(p => p.Code == account.PersonCode))
            {
                ModelState.AddModelError("PersonCode", "Invalid person selected.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    account.OutstandingBalance = 0;

                    _context.Add(account);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Account created: {AccountNumber}", account.AccountNumber);
                    TempData["SuccessMessage"] = "Account created successfully.";
                    return RedirectToAction(nameof(Index), new { personId = account.PersonCode });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating account");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the account.");
                }
            }

            var person = await _context.Persons.FindAsync(account.PersonCode);
            ViewData["PersonName"] = person?.FullName ?? "Unknown";
            ViewData["PersonId"] = account.PersonCode;
            return View(account);
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
                var account = await _context.Accounts
                    .Include(a => a.Person)
                    .FirstOrDefaultAsync(a => a.Code == id);

                if (account == null)
                {
                    return NotFound();
                }

                ViewData["PersonName"] = account.Person?.FullName ?? "Unknown";
                return View(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading account for edit, ID {AccountId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the account.";
                return RedirectToAction("Index", "Persons");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Code,PersonCode,AccountNumber,IsClosed,OutstandingBalance")] Account account)
        {
            if (id != account.Code)
            {
                return NotFound();
            }

            if (await _context.Accounts.AnyAsync(a => a.AccountNumber == account.AccountNumber && a.Code != account.Code))
            {
                ModelState.AddModelError("AccountNumber", "An account with this number already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(account);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Account updated: {AccountNumber}", account.AccountNumber);
                    TempData["SuccessMessage"] = "Account updated successfully.";
                    return RedirectToAction(nameof(Index), new { personId = account.PersonCode });
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!await AccountExists(account.Code))
                    {
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency error updating account");
                        ModelState.AddModelError(string.Empty, "The account was updated by another user. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating account");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the account.");
                }
            }

            var person = await _context.Persons.FindAsync(account.PersonCode);
            ViewData["PersonName"] = person?.FullName ?? "Unknown";
            return View(account);
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
                var account = await _context.Accounts
                    .Include(a => a.Person)
                    .Include(a => a.Transactions)
                    .FirstOrDefaultAsync(m => m.Code == id);

                if (account == null)
                {
                    return NotFound();
                }

                return View(account);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading account for delete, ID {AccountId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the account.";
                return RedirectToAction("Index", "Persons");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var account = await _context.Accounts
                    .Include(a => a.Transactions)
                    .FirstOrDefaultAsync(a => a.Code == id);

                if (account == null)
                {
                    return NotFound();
                }

                var personId = account.PersonCode;

                if (account.Transactions.Any())
                {
                    _context.Transactions.RemoveRange(account.Transactions);
                }

                _context.Accounts.Remove(account);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Account deleted: {AccountNumber}", account.AccountNumber);
                TempData["SuccessMessage"] = "Account deleted successfully.";
                return RedirectToAction(nameof(Index), new { personId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting account");
                TempData["ErrorMessage"] = "An error occurred while deleting the account.";
                return RedirectToAction("Index", "Persons");
            }
        }

        private async Task<bool> AccountExists(int id)
        {
            return await _context.Accounts.AnyAsync(e => e.Code == id);
        }
    }
}
