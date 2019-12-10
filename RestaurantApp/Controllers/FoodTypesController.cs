using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Data;
using RestaurantApp.Models;
using RestaurantApp.Utility;

namespace RestaurantApp.Controllers
{
    [Authorize(Policy = SD.AdminEndUser)]
    public class FoodTypesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FoodTypesController(ApplicationDbContext context)
        {
            _context = context;
        }


//..................Index........................................
        // GET: FoodTypes
        public async Task<IActionResult> Index()
        {
            return View(await _context.FoodType.ToListAsync());
        }
//..........................................................

//...............DETAILS.......................................

        // GET: FoodTypes/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodType = await _context.FoodType
                .SingleOrDefaultAsync(m => m.Id == id);
            if (foodType == null)
            {
                return NotFound();
            }

            return View(foodType);
        }

//............................................................


//...............CREATE.....................................

        // GET: FoodTypes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FoodTypes/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] FoodType foodType)
        {
            if (ModelState.IsValid)
            {
                _context.Add(foodType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(foodType);
        }
//...............................................................


//.................................EDIT............................

        // GET: FoodTypes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodType = await _context.FoodType.SingleOrDefaultAsync(m => m.Id == id);
            if (foodType == null)
            {
                return NotFound();
            }
            return View(foodType);
        }

        // POST: FoodTypes/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] FoodType foodType)
        {
            if (id != foodType.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(foodType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FoodTypeExists(foodType.Id))
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
            return View(foodType);
        }
//..............................................................


//......................DELETE................................


        // GET: FoodTypes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var foodType = await _context.FoodType
                .SingleOrDefaultAsync(m => m.Id == id);
            if (foodType == null)
            {
                return NotFound();
            }

            return View(foodType);
        }

        // POST: FoodTypes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var foodType = await _context.FoodType.SingleOrDefaultAsync(m => m.Id == id);
            _context.FoodType.Remove(foodType);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }


//.............................................................

        private bool FoodTypeExists(int id)
        {
            return _context.FoodType.Any(e => e.Id == id);
        }
    }
}
