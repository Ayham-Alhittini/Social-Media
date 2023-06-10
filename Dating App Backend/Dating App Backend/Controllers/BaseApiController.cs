using Dating_App_Backend.Helper;
using Microsoft.AspNetCore.Mvc;

namespace Dating_App_Backend.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [ApiController]
    [Route("api/[controller]")]
    public class BaseApiController : ControllerBase
    {

    }
}
