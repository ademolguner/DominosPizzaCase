using DominosLocationMap.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DominosLocationMap.Entities.Entities
{
    public class LocationInfo:IEntity
    {
        public int LocationId { get; set; }
        public double SourceLatitude { get; set; }
        public double SourceLongitude { get; set; }
        public double DestinationLatitude { get; set; }
        public double DestinationLongitude { get; set; }
    }
}
