using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Infrastructure.Messaging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using service.api;

namespace webapi.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        readonly IMessageSender<ProcessingCommandBus> _CommandBus;

        public CommonController(IMessageSender<ProcessingCommandBus> CommandBus)
        {
            _CommandBus = CommandBus;
        }

        // GET: api/Common
        [HttpGet]
        public async Task<ActionResult> Get()
        {
            try
            {
                var cmd = new Command()
                {
                    ProjectId = 123,
                    FilesPath = "123"
                };
                await _CommandBus.SendAsync(cmd);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return NoContent();
                throw;
            }
            return Ok();
        }
    }
}
