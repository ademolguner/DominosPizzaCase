using DominosLocationMap.Core.Entities;
using DominosLocationMap.Entities.ComplexTypes;
using DominosLocationMap.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DominosLocationMap.Business.Abstract
{
   public interface ILocationInfoService
    {
        void Add(LocationInfo locationInfo);
    }
}
