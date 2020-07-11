using DominosLocationMap.Core.Entities;
using DominosLocationMap.Entities.ComplexTypes;
using DominosLocationMap.Entities.Entities;
using DominosLocationMap.Entities.Models.Locations;
using DominosLocationMap.Entities.Models.Queue;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace DominosLocationMap.Business.Abstract
{
   public interface ILocationInfoService
    {
        List<LocationReadDataQueue> GetLocationGenerate(long count);
        void Add(LocationInfo locationInfo);

        Task<LocationWriteDataQueue> GetLocationsDistanceAsync(LocationReadDataQueue locationReadDataQueue);
    }
}
