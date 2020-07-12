using DominosLocationMap.Entities.Entities;
using DominosLocationMap.Entities.Models.Queue;
using System.Threading.Tasks;

namespace DominosLocationMap.Business.Abstract
{
    public interface ILocationInfoService
    {
        void GetLocationGenerateOperation(long count);

        Task GetLocationGenerateOperationAsync(long count);

        DominosLocation Add(DominosLocation locationInfoInputDto);

        Task<DominosLocation> AddAsync(DominosLocation locationInfoInputDto);

        Task<LocationWriteDataQueue> GetLocationsDistanceAsync(LocationReadDataQueue locationReadDataQueue);

        Task QueueDatabaseCreatedAfterSendFileProcess(LocationWriteDataQueue locationWriteDataQueue);

        Task<bool> FileReadingOperation(LocationWriteDataQueue locationWriteDataQueue);
    }
}