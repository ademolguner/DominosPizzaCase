using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DominosLocationMap.Business.Abstract;
using DominosLocationMap.Core.RabbitMQ;
using DominosLocationMap.Entities.ComplexTypes;
using DominosLocationMap.Entities.Consts;
using DominosLocationMap.Entities.Entities;
using DominosLocationMap.Entities.Models.Queue;
using Microsoft.AspNetCore.Mvc;


namespace DominosLocationMap.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationInfosController : ControllerBase
    {
        private readonly IPublisherService _publisherService;
        private readonly IMapper _mapper;
        private readonly ILocationInfoService _locationInfoService;
        public LocationInfosController(IMapper mapper, ILocationInfoService locationInfoService, IPublisherService publisherService)
        {
            _mapper = mapper;
            _locationInfoService = locationInfoService;
            _publisherService = publisherService;
        }


        // POST api/<LocationInfosController>
        [HttpPost]
        public void Post(LocationInfoInputDto infoInputDto)
        {
            var locationInfoEntity = _mapper.Map<LocationInfo>(infoInputDto);

            // burada rabbitmq olucak
            _locationInfoService.Add(locationInfoEntity);
        }

        // GET: api/<LocationInfosController>
        [HttpGet]
        public void GetLocationGenerate(long count)
        {
            // 30 milyon adet koordinat noktasının
            //Koordinatların tamamı Türkiye sınırları içinde rastgele olarak set edilmelidir
            //Kaydı yapılmış olan koordinat noktalarının bir Queue aracılığıyla aralarındaki mesafenin kilometre
            // cinsinden hesaplanıp bir txt dosyasına eklenmelidir.

            // 30 milyon koordinat alındı
            
            _publisherService.Enqueue(
                                      PrepareMessages(count),
                                      RabbitMqConsts.RabbitMqConstsList.DominosLocationReadDataQueue.ToString()
                                    ); 
        }

        

        private IEnumerable<LocationReadDataQueue> PrepareMessages(long count)
        {
            var messages = new List<LocationReadDataQueue>();
            var coordinates = _locationInfoService.GetLocationGenerate(count);
            
            foreach (var item in coordinates)
            { 
                messages.Add(_mapper.Map<LocationReadDataQueue>(item));
            }
            return messages;
        }

    }
}
