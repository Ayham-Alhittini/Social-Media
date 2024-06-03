using AutoMapper;
using Dating_App_Backend.Data;
using Dating_App_Backend.DTOs;
using Dating_App_Backend.Entities;
using Dating_App_Backend.Extensions;
using Dating_App_Backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Writers;
using System.Security.Cryptography;
using System.Text;

namespace Dating_App_Backend.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        public AccountController(UserManager<AppUser> userManager, ITokenService tokenService, IMapper mapper)
        {
            _userManager = userManager;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet("get-active-user")]
        public ActionResult ActiveUser()
        {
            return Ok(User.GetUsername());
        }

        [HttpPost("register")]/// POST  : api/account/register
        public async Task<ActionResult<UserDto>> Register(RegisterDto registerDto)
        {
            if (await (UserExist(registerDto.UserName.ToLower())))
            {
                return BadRequest("User Name Is Taken");
            }

            var user = new AppUser();
            _mapper.Map(registerDto, user);

            user.UserName = user.UserName.ToLower();

            var result = await _userManager.CreateAsync(user, registerDto.Password);

            if (!result.Succeeded)
            {
                return BadRequest("Weak Password");
            }

            var roleResult = await _userManager.AddToRoleAsync(user, "Member");

            if (!roleResult.Succeeded)
            {
                return BadRequest(roleResult.Errors);
            }

            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }

            UserDto userDto = new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
            return Ok(userDto);
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.Users.Include(p => p.Photos).FirstOrDefaultAsync(user => user.UserName == loginDto.UserName);

            if (user == null)
            {
                return Unauthorized("invalid username");
            }

            var result = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!result) return Unauthorized("Invalid Password");

            UserDto userDto = new UserDto
            {
                Username = user.UserName,
                Token = await _tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };

            return Ok(userDto);
        }
        private async Task<bool> UserExist(string username)
        {
            return await _userManager.Users.AnyAsync(user => user.UserName == username);
        }
    }
}
