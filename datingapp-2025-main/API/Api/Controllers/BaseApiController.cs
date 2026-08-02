using API.Api.Filters;
using Microsoft.AspNetCore.Mvc;

namespace API.Api.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/[controller]")]
    [ApiController]
    public class BaseApiController : ControllerBase
    {
    }
}
