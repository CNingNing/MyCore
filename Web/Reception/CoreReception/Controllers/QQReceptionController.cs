using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebCore.Base;
namespace CoreReception.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QQReceptionController:ApiBaseController
    {
        public virtual async Task<IActionResult> Reception()
        {
            await Task.Delay(500);
            return ReturnSuccessResult("success", null);
        }


    }
}
