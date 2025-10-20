using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PeopleAccountsManager.Models;

namespace PeopleAccountsManager.Controllers
{

    [Authorize]
    public class PersonsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<PersonsController> _logger;
        private const int PageSize = 10; 

        public PersonsController(AppDbContext context, ILogger<PersonsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(string? searchIdNumber, string? searchSurname, string? searchAccountNumber, int page = 1)
        {
            try
            {
                var query = _context.Persons.Include(p => p.Accounts).AsQueryable();

                if (!string.IsNullOrWhiteSpace(searchIdNumber))
                {
                    query = query.Where(p => p.IdNumber.Contains(searchIdNumber));
                    ViewData["SearchIdNumber"] = searchIdNumber;
                }

                if (!string.IsNullOrWhiteSpace(searchSurname))
                {
                    query = query.Where(p => p.Surname.Contains(searchSurname));
                    ViewData["SearchSurname"] = searchSurname;
                }

                if (!string.IsNullOrWhiteSpace(searchAccountNumber))
                {
                    query = query.Where(p => p.Accounts.Any(a => a.AccountNumber.Contains(searchAccountNumber)));
                    ViewData["SearchAccountNumber"] = searchAccountNumber;
                }

                var totalRecords = await query.CountAsync();
                var totalPages = (int)Math.Ceiling(totalRecords / (double)PageSize);

                page = Math.Max(1, Math.Min(page, totalPages > 0 ? totalPages : 1));

                var persons = await query
                    .OrderBy(p => p.Surname)
                    .ThenBy(p => p.Name)
                    .Skip((page - 1) * PageSize)
                    .Take(PageSize)
                    .ToListAsync();

                ViewData["CurrentPage"] = page;
                ViewData["TotalPages"] = totalPages;
                ViewData["TotalRecords"] = totalRecords;

                return View(persons);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving persons list");
                TempData["ErrorMessage"] = "An error occurred while loading the persons list.";
                return View(new List<Person>());
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
                var person = await _context.Persons
                    .Include(p => p.Accounts)
                    .FirstOrDefaultAsync(m => m.Code == id);

                if (person == null)
                {
                    return NotFound();
                }

                return View(person);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving person details for ID {PersonId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading person details.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Surname,IdNumber")] Person person)
        {
            if (await _context.Persons.AnyAsync(p => p.IdNumber == person.IdNumber))
            {
                ModelState.AddModelError("IdNumber", "A person with this ID Number already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Add(person);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Person created: {PersonName} {PersonSurname}", person.Name, person.Surname);
                    TempData["SuccessMessage"] = "Person created successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error creating person");
                    ModelState.AddModelError(string.Empty, "An error occurred while creating the person.");
                }
            }
            return View(person);
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
                var person = await _context.Persons.FindAsync(id);
                if (person == null)
                {
                    return NotFound();
                }
                return View(person);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading person for edit, ID {PersonId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the person.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Code,Name,Surname,IdNumber")] Person person)
        {
            if (id != person.Code)
            {
                return NotFound();
            }

            if (await _context.Persons.AnyAsync(p => p.IdNumber == person.IdNumber && p.Code != person.Code))
            {
                ModelState.AddModelError("IdNumber", "A person with this ID Number already exists.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(person);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Person updated: {PersonName} {PersonSurname}", person.Name, person.Surname);
                    TempData["SuccessMessage"] = "Person updated successfully.";
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException ex)
                {
                    if (!await PersonExists(person.Code))
                    {
                        return NotFound();
                    }
                    else
                    {
                        _logger.LogError(ex, "Concurrency error updating person");
                        ModelState.AddModelError(string.Empty, "The person was updated by another user. Please try again.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error updating person");
                    ModelState.AddModelError(string.Empty, "An error occurred while updating the person.");
                }
            }
            return View(person);
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
                var person = await _context.Persons
                    .Include(p => p.Accounts)
                    .FirstOrDefaultAsync(m => m.Code == id);

                if (person == null)
                {
                    return NotFound();
                }

                return View(person);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading person for delete, ID {PersonId}", id);
                TempData["ErrorMessage"] = "An error occurred while loading the person.";
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var person = await _context.Persons
                    .Include(p => p.Accounts)
                    .FirstOrDefaultAsync(p => p.Code == id);

                if (person == null)
                {
                    return NotFound();
                }

                if (person.Accounts.Any())
                {
                    TempData["ErrorMessage"] = "Cannot delete person with active accounts. Please delete all accounts first.";
                    return RedirectToAction(nameof(Delete), new { id });
                }

                _context.Persons.Remove(person);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Person deleted: {PersonName} {PersonSurname}", person.Name, person.Surname);
                TempData["SuccessMessage"] = "Person deleted successfully.";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting person");
                TempData["ErrorMessage"] = "An error occurred while deleting the person.";
                return RedirectToAction(nameof(Index));
            }
        }

        private async Task<bool> PersonExists(int id)
        {
            return await _context.Persons.AnyAsync(e => e.Code == id);
        }
    }
}
