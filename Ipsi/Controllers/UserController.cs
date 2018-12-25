using System;
using Ipsi.Services;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Ipsi.Controllers
{
    [Route("api/User")]
    [ApiController]
    public class UserApiController : Controller
    {
        public IUserService UserService { get; }
        public UserApiController(IUserService userService)
        {
            this.UserService = userService;
        }

        [HttpGet("GetUser")]
        public async Task<IActionResult> GetUser(string year, string seatNum)
        {
            try
            {
                var modelUser = await this.UserService.GetUser(year, seatNum);

                return Ok(modelUser);
            }
            catch (Exception ex)
            {
                // logger.Error(ex, "토큰 검증에 실패했습니다.");
                return NotFound(ex.Message);
            }
        }


        [HttpGet("GetUserList")]
        public async Task<IActionResult> GetUserList(string year, string div, string tp, string sbj)
        {
            try
            {
                var modelUserList = await this.UserService.GetUserList(year, div, tp, sbj);

                var userList = modelUserList.Select(x => new
                {
                    x.SUA030_SELECT_DIVNM,
                    x.SUA030_SELECT_TPNM,
                    x.SUA030_SBJNM,
                    x.SUA030_SEAT_NUM,
                    x.SUA030_KOR_NM
                });

                return Ok(userList);
            }
            catch (Exception ex)
            {
                // logger.Error(ex, "토큰 검증에 실패했습니다.");
                return NotFound(ex.Message);
            }
        }


    }
}