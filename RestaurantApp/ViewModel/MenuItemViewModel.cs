using RestaurantApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantApp.ViewModel
{

 //11
    public class MenuItemViewModel
    {
        // viewmodel: combination of 3 models, i.e accessing 3 models through one view

        public MenuItem MenuItem { get; set; }
        public IEnumerable<CategoryType> CategoryType { get; set; }
        public IEnumerable<FoodType> FoodType { get; set; }
    }
}
