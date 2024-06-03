using Dapper;
using Dating_App_Backend.Data;
using Dating_App_Backend.DTOs;
using Dating_App_Backend.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Data;
using System.Text.Json.Serialization;

namespace Dating_App_Backend.Controllers
{
    public class BuggyController : BaseApiController
    {
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public BuggyController(DataContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
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

        [HttpGet("get-test")]
        public async Task<ActionResult> GetTest()
        {
            string connectionString = _configuration.GetConnectionString("defaultConection");
            using IDbConnection connection = new SqlConnection(connectionString);
            string query = $"SELECT * FROM Messages where groupName like '%{"lisa"}%'";

            var result = await connection.QueryAsync<Message>(query);

            return Ok();
        }

        [HttpPost("post-test")]
        public ActionResult PostTest(IFormFile file)
        {
            return Ok(file);
        }
    }
}
