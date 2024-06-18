using Microsoft.AspNetCore.Mvc;

namespace Shared.Common;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseController : ControllerBase
{
}