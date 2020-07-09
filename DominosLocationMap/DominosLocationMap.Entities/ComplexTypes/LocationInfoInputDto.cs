using DominosLocationMap.Core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DominosLocationMap.Entities.ComplexTypes
{
   public class LocationInfoInputDto:IDto
    {
        public double SourceLatitude { get; set; }
        public double SourceLongitude { get; set; }
        public double DestinationLatitude { get; set; }
        public double DestinationLongitude { get; set; }
    }
}
