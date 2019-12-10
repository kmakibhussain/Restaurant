using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Data;
using RestaurantApp.Models;
using RestaurantApp.Utility;
using RestaurantApp.ViewModel;

namespace RestaurantApp.Controllers
{
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CartController(ApplicationDbContext context)
        {
            _context = context;
        }


        [BindProperty]
        public OrderDetailsCart detailCart { get; set; }  //ViewModel

        public IActionResult Index()
        {
            detailCart = new OrderDetailsCart()
            {
                OrderHeader = new OrderHeader()
            };

            
            detailCart.OrderHeader.OrderTotal = 0;                      //make sure order total is zero

            var claimsIdentity = (ClaimsIdentity)this.User.Identity;    //retrieve user id of logged user, to get all items to be purchased by that user
            var claim = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            var cart = _context.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value);
            if (cart != null)
            {
                detailCart.ListCart = cart.ToList();
           
                foreach (var list in detailCart.ListCart)               //order total
                {
                                                                        //retrieving menu item from db to get the price
                    list.MenuItem = _context.MenuItem.FirstOrDefault(m => m.Id == list.MenuItemId);
                    detailCart.OrderHeader.OrderTotal = detailCart.OrderHeader.OrderTotal + (list.MenuItem.Price * list.Count);
                                                                        //price * qty
                                        
                    if (list.MenuItem.Description.Length > 100)         //display description; but only upto 100 characters
                    {
                        list.MenuItem.Description = list.MenuItem.Description.Substring(0, 99) + "...";
                    }

                    detailCart.OrderHeader.PickUpTime = DateTime.Now;
                }
            }
            return View(detailCart);
        }

        [HttpPost, ActionName("Index")]
        [ValidateAntiForgeryToken]
        public IActionResult PostIndex()
        {
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);

            detailCart.ListCart = _context.ShoppingCart.Where(c => c.ApplicationUserId == claim.Value).ToList();

            OrderHeader orderHeader = detailCart.OrderHeader;

            detailCart.OrderHeader.OrderDate = DateTime.Now;

            detailCart.OrderHeader.UserId = claim.Value;

            detailCart.OrderHeader.Status = SD.StatusSubmitted;    //initial status

            _context.OrderHeader.Add(orderHeader);
            _context.SaveChanges();

            foreach (var item in detailCart.ListCart)
            {
                item.MenuItem = _context.MenuItem.FirstOrDefault(m => m.Id == item.MenuItemId);
                OrderDetail orderDetails = new OrderDetail
                {
                    MenuItemId = item.MenuItemId,
                    OrderId = orderHeader.Id,
                    Name = item.MenuItem.Name,
                    Description = item.MenuItem.Description,
                    Price = item.MenuItem.Price,
                    Count = item.Count
                };
                _context.OrderDetail.Add(orderDetails);
            }

            //removing items from db after order
            _context.ShoppingCart.RemoveRange(detailCart.ListCart);  //Remove range: removes everything
            HttpContext.Session.SetInt32("CartCount", 0);
            _context.SaveChanges();

            //return RedirectToAction(nameof(Index));
            return RedirectToAction("OrderConfirmation", "Order", new { id = orderHeader.Id });

        }


        [HttpPost]
        public IActionResult Plus(int cartId) //update count and save changes in db
        {
            var cart = _context.ShoppingCart.Where(c => c.Id == cartId).FirstOrDefault(); //retrieves from db
            cart.Count += 1;
            _context.SaveChanges();
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Minus(int cartId) //remove item & update session 
        {
            var cart = _context.ShoppingCart.Where(c => c.Id == cartId).FirstOrDefault(); //retrieves from db

            if (cart.Count == 1)
            {
                _context.ShoppingCart.Remove(cart);
                //updating the count beside cart icon, after an item is removed
                _context.SaveChanges();

                //update the session for that user:
                HttpContext.Session.SetInt32("CartCount", _context.ShoppingCart.Where(u => u.ApplicationUserId == cart.ApplicationUserId).ToList().Count);
                //CartCount is used everywhere as session name
                //total count of the no of objs inside shopcart where userid matches
            }
            else
            {
                cart.Count -= 1;
                _context.SaveChanges();
            }
            return RedirectToAction("Index");
        }

    }
}