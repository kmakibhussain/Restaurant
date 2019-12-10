using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantApp.Extensions
{
    //1 make it static class, works with static members, first argument should be of extended class precided by this keyword,
    //2nd argument is of selected value

    //Ienumerable into select list, & display in razor page

    public static class IEnumerableExtensions
    {
        public static IEnumerable<SelectListItem> ToSelectListItem<T>(this IEnumerable<T> items, int selectedValue)
        {
            return from item in items                     //for each items in IEnumerable
                   select new SelectListItem
                   {
                       Text = item.GetPropertyValue("Name"),  // call to another extension method
                       Value = item.GetPropertyValue("Id"),
                       Selected = item.GetPropertyValue("Id").Equals(selectedValue.ToString())
                   };
        }
    }   
}
