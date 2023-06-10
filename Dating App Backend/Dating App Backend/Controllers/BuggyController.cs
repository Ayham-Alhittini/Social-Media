using Dating_App_Backend.Data;
using Dating_App_Backend.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Text.Json.Serialization;

namespace Dating_App_Backend.Controllers
{
    public class BuggyController : BaseApiController
    {
        private readonly DataContext _context;

        public BuggyController(DataContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet("auth")]
        public ActionResult<string> GetSecret()
        {
            return "secret text";
        }
        [HttpGet("not-found")]
        public ActionResult GetNotFound()
        {
            return NotFound();
        }
        [HttpGet("server-error")]
        public ActionResult GetServerError()
        {
            var thing = _context.Users.Find(-1);
            var thingsToReturn = thing.ToString();
            return Ok(thingsToReturn);
        }
        [HttpGet("bad-request")]
        public ActionResult GetBadRequest()
        {
            return BadRequest("This is a bad request");
        }
        [HttpGet("test")]
        public ActionResult GetTest()
        {
            return Ok(JsonConvert.SerializeObject(24));
        }
    }
}
