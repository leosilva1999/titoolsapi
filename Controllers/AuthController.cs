using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using TiTools_backend.DTOs;
using TiTools_backend.Models;
using TiTools_backend.Services;
using System.Linq;
using TiTools_backend.Context;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.Collections.Generic;
using Microsoft.IdentityModel.Tokens;

namespace TiTools_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ITokenService _tokenService;
        private readonly AppDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IConfiguration _config;

        public AuthController(ITokenService tokenService,
            AppDbContext dbContext,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager,
            IConfiguration config)
        {
            _tokenService = tokenService;
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _config = config;
        }

        [HttpGet]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetUsers(int limit, int offset, [FromQuery] UserFilterDTO filter)
        {
            var query = _dbContext.Users.AsQueryable();

            if (!string.IsNullOrEmpty(filter.UserName))
                query = query.Where(u => u.UserName.Contains(filter.UserName));
            if (!string.IsNullOrEmpty(filter.Email))
                query = query.Where(u => u.Email.Contains(filter.Email));
            if (!string.IsNullOrEmpty(filter.Role))
            {
                var usersInRole = await _userManager.GetUsersInRoleAsync(filter.Role);
                var userIds = usersInRole.Select(u => u.Id).ToList();

                query = query.Where(u => userIds.Contains(u.Id));
            }
                

            var users = await query
                .Select(u => new
                {
                    u.Id,
                    u.UserName,
                    u.Email,
                    Roles = _dbContext.UserRoles
                        .Where(ur => ur.UserId == u.Id)
                        .Join(_dbContext.Roles,
                            ur => ur.RoleId,
                            r => r.Id,
                            (ur, r) => new { r.Id, r.Name })
                        .ToList()
                })
                .OrderBy(u => u.UserName)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            var usersCount =  await query.Skip(offset)
                .Take(limit).CountAsync();

            if (users is not null)
            {
                return Ok(new {
                    users,
                    usersCount
                });
            }
            return BadRequest(new { errors = "400", message = "Falha na requisição" });
            
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
                    Expiration = token.ValidTo,
                    Errors = false
                });
            }

            return Unauthorized(new { errors = "401", message = "Dados de acesso inválidos" });
        }

        [HttpPost]
        [Authorize(Policy = "UserOnly")]
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
        [HttpPut]
        [Authorize(Policy = "AdminOnly")]
        [Route("updateUser")]
        public async Task<IActionResult> UpdateUser(string Id, [FromBody] UpdateUserDTO updates)
        {
            var user = await _userManager.FindByIdAsync(Id!);

            if (user == null)
            {
                return StatusCode(
                    StatusCodes.Status400BadRequest,
                    new Response
                    {
                        Status = "Error",
                        Message = "User not found!"
                    });
            }

            if (updates.Username != null)
            {
                user.UserName = updates.Username;
            }
            ;
            if (updates.Email != null)
            {
                user.Email = updates.Email;
            }
            if (updates.Password != null)
            {
                var removePassword = await _userManager.RemovePasswordAsync(user);
                var addPassword = await _userManager.AddPasswordAsync(user, updates.Password);
                if (!addPassword.Succeeded)
                {
                    return StatusCode(
                   StatusCodes.Status400BadRequest,
                   new Response
                   {
                       Status = "Error",
                       Message = "Senha muito fraca, utilize letras maíúsculas e símbolos(@, #, %...)"
                   });
                }
            }


            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return StatusCode(
                    StatusCodes.Status500InternalServerError,
                    new Response
                    {
                        Status = "Error",
                        Message = "User update failed."
                    });
            }
            return Ok(
                new Response
                {
                    Status = "Success",
                    Message = "User update successfully!"
                });
        }

        //REFRESH TOKEN AINDA NÃO É UTILIZADO
        /**
        [HttpPost]
        [Authorize(Policy = "UserOnly")]
        [Route("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenModelDTO tokenModel)
        {
            if (tokenModel == null)
            {
                return BadRequest(new { errors = "400", message = "Invalid client request!" });
            }

            string? accessToken = tokenModel.AccessToken
                ?? throw new ArgumentNullException(nameof(tokenModel));
            string? refreshToken = tokenModel.RefreshToken
                ?? throw new ArgumentException(nameof(tokenModel));

            var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken!, _config);

            if (principal == null)
            {
                return BadRequest(new { errors = "400", message = "Invalid access token/refresh token" });
            }

            string userName = principal.Identity.Name;

            if (userName is null)
            {
                return BadRequest(new { errors = "400", message = "Invalid access token/refresh token" });
            }

            var user = await _userManager.FindByNameAsync(userName);

            if (user == null || user.RefreshToken != refreshToken
                            || user.RefreshTokenExpiryTime <= DateTime.Now)
            {
                return BadRequest(new { errors = "400", message = "Invalid access token/refresh token(user invalid)" });
            }

            var newAccessToken = _tokenService.GenerateAccessToken(
                principal.Claims.ToList(), _config);

            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            return new ObjectResult(new
            {
                accessToken = new JwtSecurityTokenHandler().WriteToken(newAccessToken),
                refreshToken = newRefreshToken,
                errors = "false"
            });
        }
        */

        [HttpPost]
        [Authorize(Policy = "SuperAdminOnly")]
        [Route("revoke/{useremail}")]
        public async Task<IActionResult> Revoke(string useremail)
        {
            var user = await _userManager.FindByEmailAsync(useremail);
            if (user == null)
            {
                return BadRequest(new { errors = "400", message =  "Invalid e-mail!" });
            }

            user.RefreshToken = null;

            await _userManager.UpdateAsync(user);

            return NoContent();

        }

        [Authorize(Policy = "AdminOnly")]
        [HttpGet]
        [Route("getRoles")]
        public async Task<IActionResult> getRoles(int limit, int offset)
        {

            var roles = await _roleManager.Roles.Select(r => new
            {
                r.Id,
                r.Name,
             
            })
                .OrderBy(r => r.Name)
                .Skip(offset)
                .Take(limit)
                .ToListAsync();

            if (!roles.IsNullOrEmpty())
                {
                return Ok(new
                {
                    roles
                });
            }
                else
                {
                    return StatusCode(StatusCodes.Status400BadRequest,
                        new Response { Status = "Error", Message = $"Get Roles failed" });
                }
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
                try
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
                    else
                    {
                        throw new InvalidOperationException("O usuário já está nesta role");
                    }
                }
                catch(Exception ex)
                {
                    return StatusCode(StatusCodes.Status500InternalServerError, new Response { Status = "Error", Message = ex.Message });
                }
            }
            return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "User not found" });
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete]
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
        
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete]
        [Route("RemoveUserFromAllRoles")]
        public async Task<IActionResult> RemoveUserFromAllRoles(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var result = await _userManager.RemoveFromRolesAsync(user, userRoles);
                if (result.Succeeded)
                {
                    return StatusCode(StatusCodes.Status200OK,
                        new Response
                        {
                            Status = "Success",
                            Message = $"User {user.Email} removed from all roles"
                        });
                }
            }
            return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "User not found" });
        }
        
        [Authorize(Policy = "AdminOnly")]
        [HttpDelete]
        [Route("DeleteUser")]
        public async Task<IActionResult> DeleteUser(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                var removeRoles = await _userManager.RemoveFromRolesAsync(user, userRoles);
                if (removeRoles.Succeeded)
                {
                    var result = await _userManager.DeleteAsync(user);
                    if (result.Succeeded)
                    {
                        return StatusCode(StatusCodes.Status200OK,
                            new Response
                            {
                                Status = "Success",
                                Message = $"User {user.Email} removed"
                            });
                    }
                }
            }
            return StatusCode(StatusCodes.Status400BadRequest, new Response { Status = "Error", Message = "User not found" });
        }
    }
}
