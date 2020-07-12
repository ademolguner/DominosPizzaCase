using DominosLocationMap.Core.Entities;

namespace DominosLocationMap.Entities.Entities
{
    public class DominosLocation : IEntity
    {
        public int Id { get; set; }
        public double SourceLatitude { get; set; }
        public double SourceLongitude { get; set; }
        public double DestinationLatitude { get; set; }
        public double DestinationLongitude { get; set; }
    }
}