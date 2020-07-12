using AutoMapper;
using DominosLocationMap.Business.Abstract;
using DominosLocationMap.Business.Queues.Redis;
using DominosLocationMap.Core.CrossCutting.Caching;
using DominosLocationMap.Core.RabbitMQ;
using DominosLocationMap.Core.Utilities.Helpers.DataConvertHelper;
using DominosLocationMap.DataAccess.Abstract;
using DominosLocationMap.Entities.ComplexTypes;
using DominosLocationMap.Entities.Consts;
using DominosLocationMap.Entities.Entities;
using DominosLocationMap.Entities.Models.Locations;
using DominosLocationMap.Entities.Models.Queue;
using Microsoft.Extensions.Caching.Distributed;
using ServiceStack;
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
        private readonly IOptions<LocationOptions> _options;
        private readonly IOptions<RedisOptions> _redisOption;
        private readonly ICacheManager _cacheManager;
        private readonly IObjectDataConverter _objectDataConverter;
        private readonly IDistributedCache _distributedCache;
        private readonly IPublisherService _publisherService;
        private readonly IMapper _mapper;

        public LocationInfoManager(IPublisherService publisherService, IMapper mapper, ILocationInfoDal locationInfoDal, ICacheManager cacheManager, IObjectDataConverter objectDataConverter, IDistributedCache distributedCache/*, IOptions<LocationOptions> options*/)
        {
            _locationInfoDal = locationInfoDal;
            _mapper = mapper;
            _publisherService = publisherService;
            _cacheManager = cacheManager;
            //_options = options;
            _objectDataConverter = objectDataConverter;
            _distributedCache = distributedCache;
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

        private List<LocationWriteDataQueue> locationWriteDataQueues = new List<LocationWriteDataQueue>();

        public async Task<LocationWriteDataQueue> GetLocationsDistanceAsync(LocationReadDataQueue locationReadDataQueue)
        {
            // database e yazacak işlemler

            Console.WriteLine($"RabbitMQProcessor GetLocationsDistance method  => Calısma zamanı: {DateTime.Now.ToShortTimeString()}");

            LocationWriteDataQueue locationWriteDataQueue = new LocationWriteDataQueue();
            locationWriteDataQueue.LocationReadDataQueue = locationReadDataQueue;
            locationWriteDataQueue.Distance = distance(locationReadDataQueue.LocationInfoInputDto.SourceLatitude,
                locationReadDataQueue.LocationInfoInputDto.SourceLongitude,
                locationReadDataQueue.LocationInfoInputDto.DestinationLatitude,
                locationReadDataQueue.LocationInfoInputDto.DestinationLongitude,
                'K'
                );

            //gereksiz olabilir
            locationWriteDataQueues.Add(locationWriteDataQueue);

            var a = await _locationInfoDal.AddAsync(new DominosLocation
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
            await dosyayazAsync(data);
            return data;
        }

        public async Task QueueDatabaseCreatedAfterSendFileProcess(LocationWriteDataQueue locationWriteDataQueue)
        {
            List<LocationWriteDataQueue> locationWriteDataQueues = new List<LocationWriteDataQueue>();
            locationWriteDataQueues.Add(locationWriteDataQueue);
            _publisherService.Enqueue(
                                locationWriteDataQueues,
                                RabbitMqConsts.RabbitMqConstsList.DominosLocationFileOptionQueue.ToString()
                              );
        }

        private Coordinate GetLocation()
        {
            double minLon = 26;
            double maxLon = 45;
            double minLat = 36;
            double maxLat = 42;
            Random r = new Random();
            Coordinate point = new Coordinate();
            point.Latitude = r.NextDouble() * (maxLat - minLat) + minLat;
            point.Longitude = r.NextDouble() * (maxLon - minLon) + minLon;
            return point;
        }

        private double distance(double lat1, double lon1, double lat2, double lon2, char unit)
        {
            if ((lat1 == lat2) && (lon1 == lon2))
            {
                return 0;
            }
            else
            {
                double theta = lon1 - lon2;
                double dist = Math.Sin(deg2rad(lat1)) * Math.Sin(deg2rad(lat2)) + Math.Cos(deg2rad(lat1)) * Math.Cos(deg2rad(lat2)) * Math.Cos(deg2rad(theta));
                dist = Math.Acos(dist);
                dist = rad2deg(dist);
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

        private double deg2rad(double deg)
        {
            return (deg * Math.PI / 180.0);
        }

        private double rad2deg(double rad)
        {
            return (rad / Math.PI * 180.0);
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
                else
                {
                }
            }
            return result;
        }

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

        public async Task<bool> FileReadingOperation(LocationWriteDataQueue locationWriteDataQueue)
        {
            Console.WriteLine($"RabbitMQProcessor FileReadingOperation method  => Calısma zamanı: {DateTime.Now.ToShortTimeString()}");
            var fileRootAndName = GetFileOutputOperation();
            ProcessWrite(locationWriteDataQueue, fileRootAndName).Wait();
            var data = await Task.FromResult(true);
            return data;
        }

        private string GetFileOutputOperation()
        {
            string folderRootName = @"C:\DominosCoordinates";
            bool exists = Directory.Exists(folderRootName);

            if (!exists)
                Directory.CreateDirectory(folderRootName);

            string fileRootAndName = @"C:\DominosCoordinates\Output.txt";
            if (!File.Exists(fileRootAndName))
            { FileInfo fi = new FileInfo(fileRootAndName); }

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

        public async Task dosyayazAsync(LocationWriteDataQueue locationWriteDataQueue)
        {
            //var cacheKey = "DominosRedis";
            //var existingTime = _distributedCache.GetString(cacheKey);
            //if (!string.IsNullOrEmpty(existingTime))
            //{
            //    var dede = _distributedCache.GetString(cacheKey);
            //}
            //else
            //{
            //    existingTime = DateTime.UtcNow.ToString();
            //    _distributedCache.SetString(cacheKey, _objectDataConverter.ObjectToJson<LocationWriteDataQueue>(locationWriteDataQueue));

            //}

            //string path = @"c:\temp\MyTest.txt";

            //// This text is added only once to the file.
            //if (!File.Exists(path))
            //{
            //    // Create a file to write to.
            //    using (StreamWriter sw = File.CreateText(path))
            //    {
            //        sw.WriteLine("Hello");
            //        sw.WriteLine("And");
            //        sw.WriteLine("Welcome");
            //    }
            //}

            //// This text is always added, making the file longer over time
            //// if it is not deleted.
            //using (StreamWriter sw = File.AppendText(path))
            //{
            //    sw.WriteLine("This");
            //    sw.WriteLine("is Extra");
            //    sw.WriteLine("Text");
            //}

            // Open the file to read from.
            //using (StreamReader sr = File.OpenText(path))
            //{
            //    string s = "";
            //    while ((s = sr.ReadLine()) != null)
            //    {
            //        Console.WriteLine(s);
            //    }
            //}
            //string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //string text = "First line" + Environment.NewLine;

            //    File.WriteAllText(Path.Combine(docPath, "WriteFile.txt"), locationWriteDataQueue.Distance.ToString());

            //_cacheManager.Add("OutputtxtCache", locationWriteDataQueue);

            //// Set a variable to the Documents path.

            //// Write the text to a new file named "WriteFile.txt".
            //File.WriteAllText(Path.Combine(docPath, "WriteFile.txt"), text);

            //// Create a string array with the additional lines of text
            //string[] lines = { "New line 1", "New line 2" };

            //// Append new lines of text to the file
            //File.AppendAllLines(Path.Combine(docPath, "WriteFile.txt"), lines);
        }
    }
}