using Microsoft.AspNetCore.Mvc;
using PackCheck.Contracts;

namespace PackCheck.Api.Controllers;

[ApiController]
[Route("api/templates")]
public class TemplatesController : ControllerBase
{
    [HttpGet]
    public IReadOnlyList<PackingTemplateDto> Get() => PackingTemplates.All;
}
