using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using RestaurantApp.Data;
using RestaurantApp.Models;
using RestaurantApp.Utility;
using RestaurantApp.ViewModel;

namespace RestaurantApp.Controllers
{
    public class OrderController : Controller
    {
        private readonly ApplicationDbContext _context;

        public OrderController(ApplicationDbContext context)
        {
            _context = context;
        }

        //public IActionResult Index()
        //{
        //    return View();
        //}



//......................OrderConfirmation..........................
        [BindProperty]
        public OrderDetailsViewModel OrderDetailViewModel { get; set; }

        //Get: OrderConfirmation
        public IActionResult OrderConfirmation(int id)
        {
                                                    //To prevent access to non-users :to  enter random orderid in the url & see the details
                                                    //Find user id of logged-in user
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);  //FindFirst : finds first userid

            OrderDetailViewModel = new OrderDetailsViewModel()
            {
                                                    //check the user such that , the user has a particular order
                OrderHeader = _context.OrderHeader.Where(o => o.Id == id && o.UserId == claim.Value).FirstOrDefault(), //search for only 1 record
                OrderDetail = _context.OrderDetail.Where(o => o.OrderId == id).ToList()  //order id will have orderheader id
            };

            return View(OrderDetailViewModel);
        }

//...............................OrderHistory...................................................


        [BindProperty]   //we need to display all order history, so use list
        public List<OrderDetailsViewModel> OrderDetailsViewModelList { get; set; }

        //Get:OrderHistory
        public IActionResult OrderHistory(int id = 0) //if id=0 display only 5 past orders else display all the orders
        {
                                                                             //Find user id of logged-in user
            var claimsIdentity = (ClaimsIdentity)this.User.Identity;
            var claim = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);  //FindFirst : finds first userid

                                                                              //complete list of orderheader for a particular user
            OrderDetailsViewModelList = new List<OrderDetailsViewModel>();

            List<OrderHeader> OrderHeaderList = _context.OrderHeader.Where(u => u.UserId == claim.Value).OrderByDescending(u => u.OrderDate).ToList();

                                                                                //search to see if more than 5 orders are made, then take 5 of them
            if (id == 0 && OrderHeaderList.Count > 4)   
            {
                OrderHeaderList = OrderHeaderList.Take(5).ToList();
            }

                                                                                 // assigning all orders to view model
            foreach (OrderHeader item in OrderHeaderList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel();

                                                                                 // assigning order headers and details for each of the item
                individual.OrderHeader = item;
                individual.OrderDetail = _context.OrderDetail.Where(o => o.Id == item.Id).ToList();

                OrderDetailsViewModelList.Add(individual);

            }

            return View(OrderDetailsViewModelList);
        }

 //..............................ManageOrder..............................

        //[BindProperty]
        //public List<OrderDetailsViewModel> OrderDetailsViewModel { get; set; }

        public IActionResult ManageOrder()
        {
            OrderDetailsViewModelList = new List<OrderDetailsViewModel>();

                                                                //find orders which are not ready/completed/canceled
            List<OrderHeader> OrderHeaderList = _context.OrderHeader.Where(u => u.Status != SD.StatusCompleted && u.Status != SD.StatusReady && u.Status != SD.StatusCancelled).OrderByDescending(u => u.PickUpTime).ToList();

                                                                //retreive details and add them to orderdetailsveiwmodel
            foreach (OrderHeader item in OrderHeaderList)
            {
                OrderDetailsViewModel individual = new OrderDetailsViewModel();
                individual.OrderDetail = _context.OrderDetail.Where(o => o.OrderId == item.Id).ToList();
                individual.OrderHeader = item;
                                                                //adding to "individual" orderdetailviewmodel
                OrderDetailsViewModelList.Add(individual);
            }
            return View(OrderDetailsViewModelList);
        }

//Handlers
        [HttpPost]
        public IActionResult OrderPrepare(int orderId)
        {
            OrderHeader orderHeader = _context.OrderHeader.Find(orderId);      //retrieve objects from db based route id
            orderHeader.Status = SD.StausinProcess;
            _context.SaveChanges();
            return RedirectToAction("ManageOrder");
        }


        [HttpPost]
        public IActionResult OrderReady(int orderId)
        {
            OrderHeader orderHeader = _context.OrderHeader.Find(orderId);      //retrieve objects from db based route id
            orderHeader.Status = SD.StatusReady;
            _context.SaveChanges();
            return RedirectToAction("ManageOrder");
        }


        [HttpPost]
        public IActionResult OrderCancel(int orderId)
        {
            OrderHeader orderHeader = _context.OrderHeader.Find(orderId);      //retrieve objects from db based route id
            orderHeader.Status = SD.StatusCancelled;
            _context.SaveChanges();
            return RedirectToAction("ManageOrder");

        }


        //----------------------OrderPickup-------------------------

        //order details view model
        //[BindProperty]
        //public List<OrderDetailsViewModel> OrderDetailsViewModel { get; set; }
        public IActionResult OrderPickup(string option = null, string search = null)
        {
            OrderDetailsViewModelList = new List<OrderDetailsViewModel>();
            if (search != null)
            {
                var user = new ApplicationUser();
                List<OrderHeader> orderHeaderList = new List<OrderHeader>();
                if (option == "order")
                {
                    orderHeaderList = _context.OrderHeader.Where(o => o.Id == Convert.ToInt32(search)).ToList(); //converting string to int
                }
                else
                {
                    if (option == "email")
                    {
                        //find all orders for a user
                        user = _context.Users.Where(u => u.Email.ToLower().Contains(search.ToLower())).FirstOrDefault();  //case sentive. so converting all to lower
                    }
                    else
                    {
                        if (option == "phone")
                        {
                            //find all orders for a user
                            user = _context.Users.Where(u => u.PhoneNumber.ToLower().Contains(search.ToLower())).FirstOrDefault();  //case sentive. so converting all to lower
                        }
                        else
                        {
                            //check first name as well as last name
                            user = _context.Users.Where(u => u.Firstname.ToLower().Contains(search.ToLower()) || u.LastName.ToLower().Contains(search.ToLower())).FirstOrDefault();  //case sentive. so converting all to lower
                        }
                    }
                }

                if (user != null || orderHeaderList.Count > 0)
                {
                    if (orderHeaderList.Count == 0)
                    {
                        orderHeaderList = _context.OrderHeader.Where(o => o.UserId == user.Id).OrderByDescending(o => o.PickUpTime).ToList();
                    }
                    foreach (OrderHeader item in orderHeaderList)
                    {
                        OrderDetailsViewModel individual = new OrderDetailsViewModel();
                        individual.OrderDetail = _context.OrderDetail.Where(o => o.OrderId == item.Id).ToList();
                        individual.OrderHeader = item;

                                                            //adding to "individual" orderdetailviewmodel
                        OrderDetailsViewModelList.Add(individual);
                    }
                }
            }
            else                                     //if there is no search criteria
            {
                                                    //find orders which are not ready/completed/canceled
                List<OrderHeader> OrderHeaderList = _context.OrderHeader.Where(u => u.Status == SD.StatusReady).OrderByDescending(u => u.PickUpTime).ToList();

                                                    //retreive details and add them to orderdetailsveiwmodel
                foreach (OrderHeader item in OrderHeaderList)
                {
                    OrderDetailsViewModel individual = new OrderDetailsViewModel();
                    individual.OrderDetail = _context.OrderDetail.Where(o => o.OrderId == item.Id).ToList();
                    individual.OrderHeader = item;

                                                     //adding to "individual" orderdetailviewmodel
                    OrderDetailsViewModelList.Add(individual);
                }
            }
            return View(OrderDetailsViewModelList);
        }


        //.........................OrderPickupDetails...................

        //[BindProperty]
        //public OrderDetailViewModel OrderDetailViewModel { get; set; }

        public IActionResult OrderPickupDetails(int orderId)  //receive order id as parameter
        {
            OrderDetailViewModel = new OrderDetailsViewModel();

            OrderDetailViewModel.OrderHeader = _context.OrderHeader.Where(o => o.Id == orderId).FirstOrDefault();
            //based on orderheader, we need to assign the application user
            OrderDetailViewModel.OrderHeader.ApplicationUser = _context.Users.Where(u => u.Id == OrderDetailViewModel.OrderHeader.UserId).FirstOrDefault();
            OrderDetailViewModel.OrderDetail = _context.OrderDetail.Where(o => o.OrderId == OrderDetailViewModel.OrderHeader.Id).ToList();

            return View(OrderDetailViewModel);
        }

        [HttpPost, ActionName("OrderPickupDetails")]
        public IActionResult PostOrderPickupDetails(int orderId)
        {//update status and redirect to Manage order page
            OrderHeader orderHeader = _context.OrderHeader.Find(orderId);

            orderHeader.Status = SD.StatusCompleted;
            _context.SaveChanges();
            return RedirectToAction("ManageOrder");
        }

    }
}