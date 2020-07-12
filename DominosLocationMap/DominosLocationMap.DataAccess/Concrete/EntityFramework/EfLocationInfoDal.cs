using DominosLocationMap.Core.DataAccess.EntityFramework;
using DominosLocationMap.DataAccess.Abstract;
using DominosLocationMap.DataAccess.Concrete.EntityFramework.DatbaseContext;
using DominosLocationMap.Entities.Entities;

namespace DominosLocationMap.DataAccess.Concrete.EntityFramework
{
    public class EfLocationInfoDal : EfEntityRepositoryBase<DominosLocation, DominosLocationMapDbContext>, ILocationInfoDal
    {
    }
}