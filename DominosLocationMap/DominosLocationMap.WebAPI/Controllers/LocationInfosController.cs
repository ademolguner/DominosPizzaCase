using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DominosLocationMap.Business.Abstract;
using DominosLocationMap.Entities.ComplexTypes;
using DominosLocationMap.Entities.Entities;
using Microsoft.AspNetCore.Mvc;


namespace DominosLocationMap.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationInfosController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ILocationInfoService _locationInfoService;
        public LocationInfosController(IMapper mapper, ILocationInfoService locationInfoService)
        {
            _mapper = mapper;
            _locationInfoService = locationInfoService;
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
        public IEnumerable<string> Get()
        {
            return new string[] { "Adem", "Olguner" };
        }

         
    }
}
