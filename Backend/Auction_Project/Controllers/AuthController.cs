﻿using Auction_Project.DataBase;
using Auction_Project.Models.Users;
using Auction_Project.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace Auction_Project.Authenticate
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly AppDbContext _dbContext;

        public AuthController(IUserService userService, AppDbContext dbContext, UserManager<User> userModel, IConfiguration configuration)
        {
            _userService = userService;
            _dbContext = dbContext;
        }

        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(UserChangePasswordDTO user)
        {
            var updated = await _userService.ChangePassword(user);
            if (updated)
            {
                return Ok("Password changed!");
            }
            return BadRequest("Bad request");
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserResponseDTO>> Register(UserRegisterDTO request)
        {
            
            var error = await _userService.VeryfyData(request);
            if (error != null)
            {
                return BadRequest(error);
            }
            var result = await _userService.AddUser(request);
            var role = await _userService.ChangeUserRole(new UserRoleDTO { Id = result.Id, RoleName = "User" });
            _dbContext.SaveChanges();
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserLoginResponseDTO>> Login(UserLoginDTO request)
        {

            if (!await _userService.CheckPassword(request))
            {
                return BadRequest("Invalid Password!");
            }

            if(await _userService.isUserBanned(request.UserName))
            {
                return BadRequest("User banned!");
            }

            var token = await _userService.GenerateToken(request);

            return Ok(new UserLoginResponseDTO{ 
                token = new JwtSecurityTokenHandler().WriteToken(token),
                userResponse = await _userService.GetUserById(request.UserName),
                roles = await _userService.GetUserRolesById(request.UserName)
            });
        }
    }
}