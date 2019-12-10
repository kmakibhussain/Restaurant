using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RestaurantApp.Data;
using RestaurantApp.Models;
using RestaurantApp.Utility;
using RestaurantApp.ViewModel;

namespace RestaurantApp.Controllers
{
    public class MenuItemsController : Controller
    {
        private readonly ApplicationDbContext _context;

      //for image
        private readonly IHostingEnvironment _hostingEnvironment;

        public MenuItemsController(ApplicationDbContext context, IHostingEnvironment hostingEnvironment)
        {
            _context = context;
            _hostingEnvironment = hostingEnvironment;
        }

//........................INDEX...............................

        // GET: MenuItems
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.MenuItem.Include(m => m.CategoryType).Include(m => m.FoodType);
            return View(await applicationDbContext.ToListAsync());
        }

//.............................................................        


//..........................Details........................................

        // GET: MenuItems/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            //eager loading
            var menuItem = await _context.MenuItem
                .Include(m => m.CategoryType)
                .Include(m => m.FoodType)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (menuItem == null)
            {
                return NotFound();
            }

            return View(menuItem);
        }
        //..........................CREATE........................................
        // GET: MenuItems/Create

        [BindProperty]
        public MenuItemViewModel MenuItemVM { get; set; }
        public IActionResult Create()
        {

            MenuItemVM = new MenuItemViewModel {
                MenuItem = new MenuItem(),
                FoodType = _context.FoodType.ToList(), 
                CategoryType = _context.CategoryType.ToList()
            };

            return View(MenuItemVM);
        }

        // POST: MenuItems/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,Image,Spicyness,Price,CategoryId,FoodTypeId")] MenuItem menuItem)

        {
            if (!ModelState.IsValid)
            {
                return RedirectToAction(nameof(Index));
            }
            _context.Add(menuItem);
            await _context.SaveChangesAsync();

            //Image being saved

            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;

    
            var menuItemFromDb = _context.MenuItem.Find(menuItem.Id);

            //        Checking if user uploaded any image or not 

            if (files[0] != null && files[0].Length > 0) //image
            {
                var uploads = Path.Combine(webRootPath, "images"); //upload in the webRootPath

                var extension = files[0].FileName.Substring(files[0].FileName.LastIndexOf("."), files[0].FileName.Length - files[0].FileName.LastIndexOf("."));

                // Open fileStream    
                                            //uploads= image folder,//folderpath+file_extension+filename
                using (var fileStream = new FileStream(Path.Combine(uploads, menuItem.Id + extension), FileMode.Create))         
                {//Copy in server 
                    files[0].CopyTo(fileStream);
                }


                //change path inside database              
                menuItemFromDb.Image = @"\images\" + menuItem.Id + extension;

            }
            else  //if user doesnot provide an image, setting up a default image
            {
                var uploads = Path.Combine(webRootPath, @"images\" + SD.DefaultFoodImage);

                System.IO.File.Copy(uploads, webRootPath + @"\images\" + menuItem.Id + ".png");

                menuItemFromDb.Image = @"\images\" + menuItem.Id + ".png";
            }

            await _context.SaveChangesAsync();

            //return View(menuItem);
            return RedirectToAction(nameof(Index));
        }
//......................................................

//........................EDIT...............................

        // GET: MenuItems/Edit/5
        //public async Task<IActionResult> Edit(int? id)
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM = new MenuItemViewModel()
            {
                MenuItem = _context.MenuItem.Include(m => m.CategoryType).Include(m => m.FoodType).SingleOrDefault(m => m.Id == id),
                CategoryType = _context.CategoryType.ToList(),
                FoodType = _context.FoodType.ToList()
            };

            if (MenuItemVM.MenuItem == null)
            {   
                return NotFound();
            }


            return View(MenuItemVM);
        }

        // POST: MenuItems/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,Image,Spicyness,Price,CategoryId,FoodTypeId")] MenuItem menuItem)
        {

            if (!ModelState.IsValid)
            {
                return View();
            }

            string webRootPath = _hostingEnvironment.WebRootPath;  // find webrootpath

            var files = HttpContext.Request.Form.Files; //find all files that has been posted

            var MenuItemFromDb = _context.MenuItem.Where(m => m.Id == MenuItemVM.MenuItem.Id).FirstOrDefault();

            if (files[0] != null && files[0].Length > 0) // i.e user has suppiled a file
            {
                var uploads = Path.Combine(webRootPath, "images");  //webroot,images folder

                                                                             //find extension of the file
                var extension = MenuItemFromDb.Image.Substring(MenuItemFromDb.Image.LastIndexOf("."), MenuItemFromDb.Image.Length - MenuItemFromDb.Image.LastIndexOf("."));

                                                                             //before copying it check to see if there is already a file with that extension and name
                                                                             //if so, then delete the file & then copy the new file
                if (System.IO.File.Exists(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extension)))    //Path.Combine(Uploads) gives path of the folder ,filename
                {
                                                                               //if exists then removing the file
                    System.IO.File.Delete(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extension));
                }

                extension = files[0].FileName.Substring(files[0].FileName.LastIndexOf("."), files[0].FileName.Length - files[0].FileName.LastIndexOf("."));
                                                                                //create new file that has been uploaded by user
                using (var fileStream = new FileStream(Path.Combine(uploads, MenuItemVM.MenuItem.Id + extension), FileMode.Create))                         //uploads= image folder,//folderpath+file_extension+filename
                {
                    files[0].CopyTo(fileStream);
                }
                                                                                //updating the path again
                MenuItemVM.MenuItem.Image = @"\images\" + MenuItemVM.MenuItem.Id + extension;
            }
            

            if (MenuItemVM.MenuItem.Image != null)
            {
                MenuItemFromDb.Image = MenuItemVM.MenuItem.Image;
            }
            MenuItemFromDb.Name = MenuItemVM.MenuItem.Name;
            MenuItemFromDb.Description = MenuItemVM.MenuItem.Description;
            MenuItemFromDb.Price = MenuItemVM.MenuItem.Price;
            MenuItemFromDb.Spicyness = MenuItemVM.MenuItem.Spicyness;
            MenuItemFromDb.FoodType = MenuItemVM.MenuItem.FoodType;
            MenuItemFromDb.CategoryId = MenuItemVM.MenuItem.CategoryId;

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));

        }
//......................................................

//.................Delete................................


        // GET: MenuItems/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //before deleting to just display what is the categorytype, foodtype in a text box  
            var menuItem = await _context.MenuItem
                .Include(m => m.CategoryType)
                .Include(m => m.FoodType)
                .SingleOrDefaultAsync(m => m.Id == id);
            if (menuItem == null)
            {
                return NotFound();
            }

            return View(menuItem);
        }

        // POST: MenuItems/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var menuItem = await _context.MenuItem.SingleOrDefaultAsync(m => m.Id == id);

            //before remove the object from the table,we should remove the image from the server as well
            string webRootPath = _hostingEnvironment.WebRootPath;

            //Finding MenuItem obj from the db based on the id
            menuItem = await _context.MenuItem.FindAsync(id);

            if (menuItem != null)
            {
                //remove the image from the server
                var uploads = Path.Combine(webRootPath, "images");
                var extension = menuItem.Image.Substring(menuItem.Image.LastIndexOf("."), menuItem.Image.Length - menuItem.Image.LastIndexOf("."));

                //image path
                var ImagePath = Path.Combine(uploads, menuItem.Id + extension);

                if (System.IO.File.Exists(ImagePath))
                {
                    System.IO.File.Delete(ImagePath);
                }
                _context.MenuItem.Remove(menuItem);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        

//......................................................................


        private bool MenuItemExists(int id)
        {
            return _context.MenuItem.Any(e => e.Id == id);
        }
    }
}
