using System;
using System.Collections.Generic;
using System.Text;

namespace DominosLocationMap.Entities.Models.Locations
{
  public  class LocationOptions
    {
        public double MinLatitude { get; set; } 
        public double MaxLatitude { get; set; }
        public double MinLongitude { get; set; } 
        public double MaxLongitude { get; set; } 
    }
}
