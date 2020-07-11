using System;
using System.Collections.Generic;
using System.Text;

namespace DominosLocationMap.Entities.Models.Queue
{
  public   class LocationWriteDataQueue
    {
        public LocationReadDataQueue LocationReadDataQueue { get; set; }
        public double Distance { get; set; }
    }
}
