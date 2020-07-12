using AutoMapper;
using DominosLocationMap.Business.Abstract;
using DominosLocationMap.Business.Queues.Redis;
using DominosLocationMap.Core.CrossCutting.Caching;
using DominosLocationMap.Core.Entities.Options;
using DominosLocationMap.Core.RabbitMQ;
using DominosLocationMap.Core.Utilities.Helpers.DataConvertHelper;
using DominosLocationMap.DataAccess.Abstract;
using DominosLocationMap.Entities.ComplexTypes;
using DominosLocationMap.Entities.Consts;
using DominosLocationMap.Entities.Entities;
using DominosLocationMap.Entities.Models.Locations;
using DominosLocationMap.Entities.Models.Queue;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DominosLocationMap.Business.Managers
{
    public class LocationInfoManager : ILocationInfoService
    {
        private readonly ILocationInfoDal _locationInfoDal;
        private readonly LocationOptions _locationOptions;
        private readonly RedisOptions _redisOptions;
        private readonly ICacheManager _cacheManager;
        private readonly IObjectDataConverter _objectDataConverter;
        private readonly IDistributedCache _distributedCache;
        private readonly IPublisherService _publisherService;
        private readonly IMapper _mapper;

        public LocationInfoManager(IPublisherService publisherService,
            IMapper mapper,
            ILocationInfoDal locationInfoDal,
            ICacheManager cacheManager,
            IObjectDataConverter objectDataConverter,
            IDistributedCache distributedCache,
            IOptions<LocationOptions> locationOptions,
            IOptions<RedisOptions> redisOptions)
        {
            _locationInfoDal = locationInfoDal;
            _mapper = mapper;
            _publisherService = publisherService;
            _cacheManager = cacheManager;
            _objectDataConverter = objectDataConverter;
            _distributedCache = distributedCache;
            _locationOptions = locationOptions.Value;
            _redisOptions = redisOptions.Value;
        }

        #region Public metotolar

        public void GetLocationGenerateOperation(long count)
        {
            _publisherService.Enqueue(
                                     PrepareMessages(count),
                                     RabbitMqConsts.RabbitMqConstsList.DominosLocationDatabaseQueue.ToString()
                                   );
        }

        public async Task GetLocationGenerateOperationAsync(long count)
        {
            await _publisherService.EnqueueAsync(
                                      PrepareMessages(count),
                                      RabbitMqConsts.RabbitMqConstsList.DominosLocationDatabaseQueue.ToString()
                                    );
        }

        public async Task<LocationWriteDataQueue> GetLocationsDistanceAsync(LocationReadDataQueue locationReadDataQueue)
        {
            Console.WriteLine($"RabbitMQProcessor DataBaseConsumerManager GetLocationsDistance method  => Calısma zamanı: {DateTime.Now.ToShortTimeString()}");

            LocationWriteDataQueue locationWriteDataQueue = new LocationWriteDataQueue();
            locationWriteDataQueue.LocationReadDataQueue = locationReadDataQueue;
            locationWriteDataQueue.Distance = DistanceCalculate(locationReadDataQueue.LocationInfoInputDto.SourceLatitude,
                                                        locationReadDataQueue.LocationInfoInputDto.SourceLongitude,
                                                        locationReadDataQueue.LocationInfoInputDto.DestinationLatitude,
                                                        locationReadDataQueue.LocationInfoInputDto.DestinationLongitude,
                                                        'K'
                                                       );

            var a = await AddAsync(new DominosLocation
            {
                DestinationLatitude = locationWriteDataQueue.LocationReadDataQueue.LocationInfoInputDto.DestinationLatitude,
                DestinationLongitude = locationWriteDataQueue.LocationReadDataQueue.LocationInfoInputDto.DestinationLongitude,
                SourceLatitude = locationWriteDataQueue.LocationReadDataQueue.LocationInfoInputDto.SourceLatitude,
                SourceLongitude = locationWriteDataQueue.LocationReadDataQueue.LocationInfoInputDto.SourceLongitude
            });

            if (a.Id != 0)
            {
                await QueueDatabaseCreatedAfterSendFileProcess(locationWriteDataQueue);
            }
            var data = await Task.FromResult(locationWriteDataQueue);
            return data;
        }

        public DominosLocation Add(DominosLocation dominosLocation)
        {
            _locationInfoDal.Add(dominosLocation);
            return dominosLocation;
        }

        public async Task<DominosLocation> AddAsync(DominosLocation dominosLocation)
        {
            var result = await _locationInfoDal.AddAsync(dominosLocation);
            return result;
        }

        public async Task QueueDatabaseCreatedAfterSendFileProcess(LocationWriteDataQueue locationWriteDataQueue)
        {
            List<LocationWriteDataQueue> locationWriteDataQueues = new List<LocationWriteDataQueue>();
            locationWriteDataQueues.Add(locationWriteDataQueue);
            await _publisherService.EnqueueAsync(
                                 locationWriteDataQueues,
                                 RabbitMqConsts.RabbitMqConstsList.DominosLocationFileOptionQueue.ToString()
                               );
        }

        public async Task<bool> FileWritingOperation(LocationWriteDataQueue locationWriteDataQueue)
        {
            Console.WriteLine($"RabbitMQProcessor FileOperationConsumerManager FileWritingOperation  method  => Calısma zamanı: {DateTime.Now.ToShortTimeString()}");
            var fileRootAndName = GetFileOutputOperation();
            ProcessWrite(locationWriteDataQueue, fileRootAndName).Wait();
            var data = await Task.FromResult(true);
            return data;
        }

        #endregion Public metotolar

        #region Private - yardımcı metotlar

        private IEnumerable<LocationReadDataQueue> PrepareMessages(long count)
        {
            var messages = new List<LocationReadDataQueue>();
            var coordinates = GetLocationGenerate(count);

            foreach (var item in coordinates)
            {
                messages.Add(_mapper.Map<LocationReadDataQueue>(item));
            }
            return messages;
        }

        public List<LocationReadDataQueue> GetLocationGenerate(long count)
        {
            List<LocationReadDataQueue> result = new List<LocationReadDataQueue>();
            for (int i = 0; i < count;)
            {
                var destinationPoint = GetLocation();
                var sourcePoint = GetLocation();

                LocationReadDataQueue point = new LocationReadDataQueue();
                point.LocationInfoInputDto = new LocationInfoInputDto()
                {
                    DestinationLatitude = destinationPoint.Latitude,
                    DestinationLongitude = destinationPoint.Longitude,
                    SourceLatitude = sourcePoint.Latitude,
                    SourceLongitude = sourcePoint.Longitude
                };
                if (result.Where(c => c.LocationInfoInputDto.DestinationLatitude == point.LocationInfoInputDto.DestinationLatitude
                                  && c.LocationInfoInputDto.DestinationLongitude == point.LocationInfoInputDto.DestinationLongitude
                                  && c.LocationInfoInputDto.SourceLatitude == point.LocationInfoInputDto.SourceLatitude
                                  && c.LocationInfoInputDto.SourceLongitude == point.LocationInfoInputDto.SourceLongitude).Count() == 0)
                {
                    i++;
                    result.Add(point);
                }
            }
            return result;
        }

        private Coordinate GetLocation()
        {
            double minLon = _locationOptions.MinLongitude;
            double maxLon = _locationOptions.MaxLongitude;
            double minLat = _locationOptions.MinLatitude;
            double maxLat = _locationOptions.MaxLatitude;
            Random r = new Random();
            Coordinate point = new Coordinate();
            point.Latitude = r.NextDouble() * (maxLat - minLat) + minLat;
            point.Longitude = r.NextDouble() * (maxLon - minLon) + minLon;
            return point;
        }

        private double DistanceCalculate(double lat1, double lon1, double lat2, double lon2, char unit)
        {
            if ((lat1 == lat2) && (lon1 == lon2))
            {
                return 0;
            }
            else
            {
                double theta = lon1 - lon2;
                double dist = Math.Sin(Deg2Rad(lat1)) * Math.Sin(Deg2Rad(lat2)) + Math.Cos(Deg2Rad(lat1)) * Math.Cos(Deg2Rad(lat2)) * Math.Cos(Deg2Rad(theta));
                dist = Math.Acos(dist);
                dist = Rad2Deg(dist);
                dist = dist * 60 * 1.1515;
                if (unit == 'K')
                {
                    dist = dist * 1.609344;
                }
                else if (unit == 'N')
                {
                    dist = dist * 0.8684;
                }
                return (dist);
            }
        }

        private double Deg2Rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        private double Rad2Deg(double rad)
        {
            return (rad / Math.PI * 180.0);
        }

        private string GetFileOutputOperation()
        {
            string folderRootName = @"C:\DominosCoordinates";
            bool exists = Directory.Exists(folderRootName);

            if (!exists)
                Directory.CreateDirectory(folderRootName);

            string fileRootAndName = @"C:\DominosCoordinates\Output.txt";
            if (!File.Exists(fileRootAndName))
            {
                FileInfo fi = new FileInfo(fileRootAndName);
            }

            return fileRootAndName;
        }

        private Task ProcessWrite(LocationWriteDataQueue locationWriteDataQueue, string filePath)
        {
            var fileTextData = Math.Round(locationWriteDataQueue.LocationReadDataQueue.LocationInfoInputDto.SourceLatitude, 10).ToString() + "\t" +
                               Math.Round(locationWriteDataQueue.LocationReadDataQueue.LocationInfoInputDto.SourceLongitude, 10).ToString() + "\t" +
                               Math.Round(locationWriteDataQueue.LocationReadDataQueue.LocationInfoInputDto.DestinationLatitude, 10).ToString() + "\t" +
                               Math.Round(locationWriteDataQueue.LocationReadDataQueue.LocationInfoInputDto.DestinationLongitude, 10).ToString() + "\t" +
                               Math.Round(locationWriteDataQueue.Distance, 3).ToString() + "\r\n";
            return WriteTextAsync(filePath, fileTextData);
        }

        private async Task WriteTextAsync(string filePath, string text)
        {
            byte[] encodedText = Encoding.Unicode.GetBytes(text);

            using (FileStream sourceStream = new FileStream(filePath,
                FileMode.Append, FileAccess.Write, FileShare.None,
                bufferSize: 4096, useAsync: true))
            {
                await sourceStream.WriteAsync(encodedText, 0, encodedText.Length);
            };
        }

        #endregion Private - yardımcı metotlar
    }
}