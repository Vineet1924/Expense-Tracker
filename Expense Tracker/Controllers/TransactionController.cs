using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Expense_Tracker.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;

namespace Expense_Tracker.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TransactionController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Statement()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Access");
        }

        // GET: Transaction
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Transactions.Include(t => t.Category);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Transaction/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Transactions == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // GET: Transaction/Create
        public IActionResult Create()
        {
            string expenseValue = "Expense";
            int totalSum = _context.Transactions.Where(t => t.Category.Type == expenseValue)
                                    .Sum(t => t.Amount);

            DateTime thirtyDaysAgo = DateTime.Now.AddDays(-30);

            int lastMonthSum = _context.Transactions
                                    .Where(t => t.Date >= thirtyDaysAgo && t.Category.Type == expenseValue)
                                    .Sum(t => t.Amount);

            ViewBag.TotalSum = totalSum;
            ViewBag.LastMonthSum = lastMonthSum;

            var transaction = new Transaction
            {
                Date = DateTime.Now
            };
            PopulateCategories();
            return View(transaction);
        }

        // POST: Transaction/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("TransactionId,CategoryId,Amount,Note,Date")] Transaction transaction)
        {
            PopulateCategories();

            int totalSum = _context.Transactions.Sum(t => t.Amount);

            DateTime thirtyDaysAgo = DateTime.Now.AddDays(-30);

            string expenseValue = "Expense";

            int lastMonthSum = _context.Transactions
                                    .Where(t => t.Date >= thirtyDaysAgo && t.Category.Type == expenseValue)
                                    .Sum(t => t.Amount);

            ViewBag.TotalSum = totalSum;
            ViewBag.LastMonthSum = lastMonthSum;

            var transaction1 = new Transaction
            {
                Date = DateTime.Now
            };

            if (transaction.CategoryId <= 0)
            {
                ModelState.AddModelError("CategoryId", "Please select a category.");
                return View(transaction1);
            }

            if (transaction.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Please enter a valid amount.");
                return View(transaction1);
            }

            if (transaction.Date == DateTime.MinValue)
            {
                ModelState.AddModelError("Date", "Please enter a Date.");
                return View(transaction1);
            }
            _context.Add(transaction);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: Transaction/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {

            if (id == null || _context.Transactions == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }
            PopulateCategories();
            return View(transaction);
        }

        // POST: Transaction/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("TransactionId,CategoryId,Amount,Note,Date")] Transaction transaction)
        {
            PopulateCategories();
            if (id != transaction.TransactionId)
            {
                return NotFound();
            }

            if (transaction.CategoryId <= 0)
            {
                ModelState.AddModelError("CategoryId", "Please select a category.");
                return View(transaction);
            }

            if (transaction.Amount <= 0)
            {
                ModelState.AddModelError("Amount", "Please enter a valid amount.");
                return View(transaction);
            }

            if (transaction.Date == DateTime.MinValue)
            {
                ModelState.AddModelError("Date", "Please enter a Date.");
                return View(transaction);
            }

            try
                {
                    _context.Update(transaction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TransactionExists(transaction.TransactionId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
        }

        // GET: Transaction/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Transactions == null)
            {
                return NotFound();
            }

            var transaction = await _context.Transactions
                .Include(t => t.Category)
                .FirstOrDefaultAsync(m => m.TransactionId == id);
            if (transaction == null)
            {
                return NotFound();
            }

            return View(transaction);
        }

        // POST: Transaction/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Transactions == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Transactions'  is null.");
            }
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction != null)
            {
                _context.Transactions.Remove(transaction);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TransactionExists(int id)
        {
          return (_context.Transactions?.Any(e => e.TransactionId == id)).GetValueOrDefault();
        }

        [NonAction]
        public void PopulateCategories()
        {
            var CategoryCollection = _context.Categories.ToList();
            Category DefaultCategory = new Category() { CategoryId = 0, Title = "Choose a Category" };
            CategoryCollection.Insert(0, DefaultCategory);
            ViewBag.Categories = CategoryCollection;
        }

        [Route("Transaction/Search")]
        public IActionResult Search(DateTime fromDate, DateTime toDate)
        {
            var transactions = _context.Transactions
                .Where(t => t.Date >= fromDate && t.Date <= toDate)
                .ToList();

            return Json(transactions);
        }
    }
}
