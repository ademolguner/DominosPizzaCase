using AutoMapper;
using DominosLocationMap.Entities.ComplexTypes;
using DominosLocationMap.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DominosLocationMap.Business.Mappings
{
    public class AutoMapping:Profile
    {
        public AutoMapping()
        {
            CreateMap<LocationInfo, LocationInfoInputDto>();
        }
    }
}
