﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TiTools_backend.DTOs;
using TiTools_backend.Models;
using TiTools_backend.Services;

namespace TiTools_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

        public AuthController(ITokenService tokenService,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config)
        {
            _tokenService = tokenService;
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
        }

        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginModelDTO model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user is not null && await _userManager.CheckPasswordAsync(user, model.Password))
            {
                var userRoles = await _userManager.GetRolesAsync(user);

                var authClaims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim("id", user.UserName!),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),                  
                };

                foreach (var userRole in userRoles)
                {
                    authClaims.Add(new Claim(ClaimTypes.Role, userRole));
                }

                var token = _tokenService.GenerateAccessToken(authClaims, _config);
                var refreshToken = _tokenService.GenerateRefreshToken();

                _ = int.TryParse(_config["JwtTest:RefreshTokenValidityInMinutes"],
                    out int refreshTokenValidityInMinutes);

                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiryTime = DateTime.Now.AddMinutes(refreshTokenValidityInMinutes);

                await _userManager.UpdateAsync(user);

                return Ok(new
                {
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    RefreshToken = refreshToken,
                    Expiration = token.ValidTo
                });
            }

            return Unauthorized("Dados de acesso inválidos");
        }

        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModelDTO model)
        {
            var userExists = await _userManager.FindByEmailAsync(model.Email!);

            if (userExists != null)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response
                    {
                        Status = "Error",
                        Message = "User already exists!"
                    });
            }

            ApplicationUser user = new()
            {
                Email = model.Email,
                SecurityStamp = Guid.NewGuid().ToString(),
                UserName = model.Username
            };
            var result = await _userManager.CreateAsync(user, model.Password!);

            if (!result.Succeeded)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response
                    {
                        Status = "Error",
                        Message = "User creation failed."
                    });
            }
            return Ok(
                new Response
                {
                    Status = "Success",
                    Message = "User created successfully!"
                });
        }

        [HttpPost]
        [Authorize(Policy = "UserOnly")]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenModelDTO tokenModel)
        {
            if (tokenModel == null)
            {
                return BadRequest("Invalid client request!");
            }

            string? accessToken = tokenModel.AccessToken
                ?? throw new ArgumentNullException(nameof(tokenModel));
            string? refreshToken = tokenModel.RefreshToken
                ?? throw new ArgumentException(nameof(tokenModel));

            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken!, _config);

            if (principal == null)
            {
                return BadRequest("Invalid access token/refresh token");
            }

            string userName = principal.Identity.Name;

            if (userName is null)
            {
                return BadRequest("Invalid access token/refresh token");
            }

            var user = await _userManager.FindByNameAsync(userName);

            if (user == null || user.RefreshToken != refreshToken
                            || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest("Invalid access token/refresh token(user invalid)");
            }

            var newAccessToken = _tokenService.GenerateAccessToken(
                principal.Claims.ToList(), _config);

            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            return new ObjectResult(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                refreshToken = newRefreshToken
            });
        }

        [Authorize(Policy = "SuperAdminOnly")]
        [HttpPost]
        [Route("revoke/{useremail}")]
        public async Task<IActionResult> Revoke(string useremail)
        {
            var user = await _userManager.FindByEmailAsync(useremail);
            if (user == null)
            {
                return BadRequest("Invalid e-mail!");
            }

            user.RefreshToken = null;

            await _userManager.UpdateAsync(user);

            return NoContent();

        }

        [Authorize(Policy = "SuperAdminOnly")]
        [HttpPost]
        [Route("CreateRole")]
        public async Task<IActionResult> CreateRole(string roleName)
        {
            var roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                var roleResult = await _roleManager.CreateAsync(new IdentityRole(roleName));
                if (roleResult.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK,
                        new Response { Status = "Success", Message = $"Role {roleName} added successfully" });
                }
                else
                {
                    return StatusCode(StatusCodes.Status400BadRequest,
                        new Response { Status = "Error", Message = $"Issue adding the new {roleName} role" });
                }
            }
            return StatusCode(StatusCodes.Status400BadRequest,
                new Response { Status = "Error", Message = "Role already exists." });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [Route("AddUserToRole")]
        public async Task<IActionResult> AddUserToRole(string email, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _userManager.AddToRoleAsync(user, roleName);
                if (result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK,
                        new Response
                        {
                            Status = "Success",
                            Message = $"User {user.Email} added to the {roleName} role"
                        });
                }
            }
            return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "User not found" });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        [Route("RemoveUserFromRole")]
        public async Task<IActionResult> RemoveUserFromRole(string email, string roleName)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var result = await _userManager.RemoveFromRoleAsync(user, roleName);
                if (result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK,
                        new Response
                        {
                            Status = "Success",
                            Message = $"User {user.Email} removed from the {roleName} role"
                        });
                }
            }
            return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "User not found" });
        }
    }
}
