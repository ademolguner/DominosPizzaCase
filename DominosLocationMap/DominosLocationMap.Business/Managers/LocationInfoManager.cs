using DominosLocationMap.Business.Abstract;
using DominosLocationMap.DataAccess.Abstract;
using DominosLocationMap.Entities.ComplexTypes;
using DominosLocationMap.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DominosLocationMap.Business.Managers
{
    public class LocationInfoManager : ILocationInfoService
    {
        private readonly ILocationInfoDal _locationInfoDal;
        public LocationInfoManager(ILocationInfoDal locationInfoDal)
        {
            _locationInfoDal = locationInfoDal;
        }

        public void Add(LocationInfo locationInfo)
        {
            _locationInfoDal.Add(locationInfo);
        }
    }
}
