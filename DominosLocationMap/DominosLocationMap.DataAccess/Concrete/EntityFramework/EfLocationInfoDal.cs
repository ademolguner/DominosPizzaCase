using DominosLocationMap.Core.DataAccess.EntityFramework;
using DominosLocationMap.DataAccess.Abstract;
using DominosLocationMap.DataAccess.Concrete.EntityFramework.DatbaseContext;
using DominosLocationMap.Entities.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DominosLocationMap.DataAccess.Concrete.EntityFramework
{
    public class EfLocationInfoDal : EfEntityRepositoryBase<LocationInfo, DominosLocationMapDbContext>, ILocationInfoDal
    {
    }
}
