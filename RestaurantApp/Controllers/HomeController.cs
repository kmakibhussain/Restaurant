using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Data;
using RestaurantApp.Models;
using RestaurantApp.Utility;
using RestaurantApp.ViewModel;

namespace RestaurantApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        //[TempData]
        //public string StatusMessage { get; set; }
        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public IndexViewModel IndexVM { get; set; }


        public async Task<IActionResult> Index()
        {
            //retrieve everything and add that to index view model
            IndexVM = new IndexViewModel()
            {
                MenuItems = await _context.MenuItem.Include(m => m.CategoryType).Include(m => m.FoodType).ToListAsync(),
                CategoryTypes = _context.CategoryType.OrderBy(c => c.DisplayOrder)

            };

            return View(IndexVM);
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }


        [BindProperty]
        public ShoppingCart CartObj { get; set; }


        // GET: Details
        [Authorize]
        public IActionResult GetDetails(int id) //id coming from route-id in thumbnailareapartial
        {
            var MenuItemFromDb = _context.MenuItem.Include(m => m.CategoryType).Include(m => m.FoodType)
                .Where(x => x.Id == id).FirstOrDefault(); // retreives just one record


            CartObj = new ShoppingCart()
            {
                MenuItemId = MenuItemFromDb.Id,
                MenuItem = MenuItemFromDb
            };

            return View(CartObj);
        }

        [Authorize]
        [HttpPost, ActionName("GetDetails")]
        [ValidateAntiForgeryToken]
        public IActionResult PostDetails()
        {
            if (ModelState.IsValid)
            {
                // to retreive current user identity of the currently logged user
                var claimsIdentity = (ClaimsIdentity)this.User.Identity;
                
                //finding out claims for this identity user
                var claim = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

                //based on the claim we can find out the user identity
                CartObj.ApplicationUserId = claim.Value;                         //this line stores user id from the logged-in-user

                ShoppingCart cartFromDb = _context.ShoppingCart.Where(c => c.ApplicationUserId == CartObj.ApplicationUserId
                                                                    && c.MenuItemId == CartObj.MenuItemId).FirstOrDefault();

                if (cartFromDb == null)
                {
                    _context.ShoppingCart.Add(CartObj); //user adds item to the cart for the first time
      
                }
                else
                {
                    cartFromDb.Count = cartFromDb.Count + CartObj.Count;
                }
                 _context.SaveChanges();

                //10. add session and increment count
                var count = _context.ShoppingCart.Where(c => c.ApplicationUserId == CartObj.ApplicationUserId).ToList().Count();
                HttpContext.Session.SetInt32("CartCount", count);

                TempData["message"] = "Item Successfully Added To Cart";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var MenuItemFromDb = _context.MenuItem.Include(m => m.CategoryType).Include(m => m.FoodType).FirstOrDefault(); // retreives just one record
                {
                    CartObj = new ShoppingCart()
                    {
                        MenuItemId = MenuItemFromDb.Id,
                        MenuItem = MenuItemFromDb
                    };
                }
                return View(CartObj);
            }
        }
    }
}
