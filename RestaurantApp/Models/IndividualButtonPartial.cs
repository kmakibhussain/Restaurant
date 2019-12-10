
//6

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantApp.Models
{
    public class IndividualButtonPartial
    {
        public string View { get; set; }
        public string Glyph { get; set; }
        public string ButtonType { get; set; }

        public int? Id { get; set; }  //nullable

        public string ActionParameters
        {
            get
            {
                if (Id != 0 && Id != null)
                {
                    return Id.ToString();
                }
                return null;
            }
        }
    }
}
