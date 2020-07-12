using DominosLocationMap.Business.Abstract;
using Microsoft.AspNetCore.Mvc;
using System;

namespace DominosLocationMap.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationInfosController : ControllerBase
    {
        private readonly ILocationInfoService _locationInfoService;

        public LocationInfosController(ILocationInfoService locationInfoService)
        {
            _locationInfoService = locationInfoService;
        }

        // GET: api/<LocationInfosController>
        [HttpGet]
        public void GetLocationGenerate(long count)
        {
            long stepCount = 1000;
            long packagesCount = Convert.ToInt64(count / stepCount);
            long remainingCount = Convert.ToInt64(count % stepCount);
            if (remainingCount != 0)
                packagesCount += 1;

            for (int i = 0; i < packagesCount; i++)
            {
                if (i == packagesCount - 1)
                {
                    _locationInfoService.GetLocationGenerateOperationAsync(remainingCount);
                }
                else
                {
                    _locationInfoService.GetLocationGenerateOperationAsync(stepCount);
                }
            }
        }
    }
}