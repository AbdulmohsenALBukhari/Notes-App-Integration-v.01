using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Notes_App_Integration_v._01.Data;
using Notes_App_Integration_v._01.Model;
using Notes_App_Integration_v._01.ModelViews;
using Notes_App_Integration_v._01.Services;
using System.Security.Claims;
using System.Web;

namespace Notes_App_Integration_v._01.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<AccountUserModel> _user;
        private readonly SignInManager<AccountUserModel> signInManager;
        private readonly IEmailSender email;
        private readonly RoleManager<AccountRoleModel> roleManager;

        public AccountController(AppDbContext dbContext, UserManager<AccountUserModel> userManager,
            SignInManager<AccountUserModel> signInManager, IEmailSender email,RoleManager<AccountRoleModel> roleManager)
        {
            this._dbContext = dbContext;
            this._user = userManager;
            this.signInManager = signInManager;
            this.email = email;
            this.roleManager = roleManager;
        }


        //class {GetAllUserAsync}
        [HttpGet]
        [Route("GetAllUserAsync")]
        public async Task<IActionResult> GetAllUserAsync()
        {
            //  get table user and insert value list
            IEnumerable<AccountUserModel> ListUser = await _dbContext.Users.ToListAsync();
            return Ok(ListUser);
        }// end class {GetAllUserAsync} 


        //  class {Rigister}
        [HttpPost]
        [Route("Rigister")]
        public async Task<IActionResult> Rigister(RegisterModel model)
        {
            // check all valid
            if (ModelState.IsValid)
            {
                //  conditions Email
                if (model.Email.IsNullOrEmpty() || !model.Email.Contains("@") || !model.Email.Contains("."))
                {
                    return NotFound();
                }
                //  conditions Email and user name if Existes or not
                if (Existes(model.Email, model.UserName))
                {
                    return BadRequest("Emil or user is Existes");
                }
                //  Create new user in database and insert value 
                var User = new AccountUserModel
                {
                    Email = model.Email,
                    UserName = model.UserName
                };
                // check and save user in database and check give true or false
                var result = await _user.CreateAsync(User, model.PasswordHash);

                // conditions result is true or false
                if (result.Succeeded)
                {
                    // Token
                    var token = await _user.GenerateEmailConfirmationTokenAsync(User); // Create token form User
                    var confirmLink = Url.Action("RegistreationConfirm", "Account", new  // Create Link and can send to email to [EmailConfirmed] 
                    {
                        Id = User.Id,
                        Token = HttpUtility.UrlEncode(token)
                    }, Request.Scheme);

                    // contact Email
                    var text = "Please Confirm Registration at our sute";
                //    var link = "<a href=\""+confirmLink+"\">Confirm</a>\r\n";
                 
                    //  Send Email
                //    await email.SendEmailAsync(User.Email, text, link);
                    return Ok(confirmLink);
                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }

            return BadRequest(ModelState);
        }// end class {Rigister}


        // Metod check email and user name
        private bool Existes(string email, string userName)
        {
            return _dbContext.Users.Any(u => u.Email == email || u.UserName == userName);
        }// end class {Existes}


        /*
         * class {RegistreationConfirm}
         Metod check if user open link and after send 1 in database change [EmailConfirmed] to = 1
        this metod run if user open link [confirmLink] 
         */
        [AllowAnonymous]
        [HttpGet]
        [Route("RegistreationConfirm")]
        public async Task<IActionResult> RegistreationConfirm(string id, string Token)
        {
            // Check id or Token is empty
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(Token))
            {
                return BadRequest();
            }
            //  Find Id 
            var user = await _user.FindByIdAsync(id);
            if (user == null)
            {
                return BadRequest(user);
            }
            //  check the token and decode link and make  in datebase
            var result = await _user.ConfirmEmailAsync(user, HttpUtility.UrlDecode(Token));
            if (result.Succeeded)
            {
                return Ok("Done");
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }// end class {RegistreationConfirm}


        //  Metod {Login}
        [AllowAnonymous]
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            await CreateAdmin();
            await CreateRoles();
            if (model == null)
            {
                return NotFound(model);
            }
            //  Find user by email
            var user = await _user.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return NotFound(user);
            }
            else if (!user.EmailConfirmed)
            {
                return BadRequest("User is not Confirm user Please check you email");
            }
            //  check if user and password is true or fales and if user login more 3 time block user
            var result = await signInManager.PasswordSignInAsync(user, model.PasswordHash, model.RememberMe, true);
            if (result.Succeeded)
            {
                if (await roleManager.RoleExistsAsync("User"))
                {
                    if(! await _user.IsInRoleAsync(user, "User"))
                    {
                        await _user.AddToRoleAsync(user, "User");
                    }
                }
                var roleName = await GetRoleNameByUserId(user.Id);
                if (roleName != null)
                AddCookies(user.UserName, roleName, user.Id, model.RememberMe);
                return Ok();
            }
            else if (result.IsLockedOut)
            {
                return Unauthorized("user account is blocked");
            }

            return BadRequest(result);
        }// end class {Login}

        [AllowAnonymous]
        [HttpGet]
        [Route("Logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok();
        }

        private async Task<string> GetRoleNameByUserId(string id)
        {
            var userRole = await _dbContext.UserRoles.FirstOrDefaultAsync(x => x.UserId == id);
            if(userRole != null)
            {
                return await _dbContext.Roles.Where(x => x.Id == userRole.RoleId).Select(x => x.Name).FirstOrDefaultAsync();
            }
            return null;
        }

        //  get by id
        [HttpGet]
        [Route("get-by-id/{Id}")]
        [ActionName("FindById")]
        public async Task<IActionResult> GetUserById([FromRoute] string Id)
        {
            var itme = await _dbContext.Users.SingleOrDefaultAsync(x => x.Id == Id);
            if (itme != null)
            {
                return Ok(itme.UserName);
            }
            return NotFound("Note Not found");
        }// end metod by id

        //  update user
        [HttpPut]
        [Route("update-user/{Id}")]
        public async Task<IActionResult> UpdateNote([FromRoute] string Id, [FromBody] AccountUserModel userModel)
        {
            var UpdateUser= await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == Id);
            if (UpdateUser != null)
            {
                UpdateUser.UserName = userModel.UserName;
                UpdateUser.Email = userModel.Email;
                UpdateUser.PasswordHash = userModel.PasswordHash;
                await _dbContext.SaveChangesAsync();
                return Ok(UpdateUser);
            }
            return NotFound("Bad requst");
        }// end update user


        // Delete user
        [HttpDelete]
        [Route("Remove-user/{Id}")]
        public async Task<IActionResult> DeleteNote([FromRoute] string Id)
        {
            var DeleteItem = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == Id);
            if (DeleteItem != null)
            {
                _dbContext.Remove(DeleteItem);
                await _dbContext.SaveChangesAsync();
                return Ok("Done Bitch");
            }
            return NotFound("Note Not found");
        }   //end Delete User

        private async Task CreateAdmin()
        {
            var admin = await _user.FindByNameAsync("Admin");
            if (admin == null)
            {
                var User = new AccountUserModel
                {
                    Email = "manager@admin.com",
                    UserName = "Admin",
                    EmailConfirmed = true
                };
                var x = await _user.CreateAsync(User,"@Password125856479");
                if (x.Succeeded)
                {
                    if (await roleManager.RoleExistsAsync("Admin"))
                    {
                        await _user.AddToRoleAsync(User, "Admin");
                    }
                }
            }
        }

        private async Task CreateRoles()
        {
            if (roleManager.Roles.Count() <1)
            {
                var role = new AccountRoleModel
                {
                    Name = "Admin",
                };
                await roleManager.CreateAsync(role);

                role = new AccountRoleModel
                {
                    Name = "User",
                };
                await roleManager.CreateAsync(role);
            }
        }

        private async void AddCookies(string username, string roleName,string userId, bool remember)
        {
            var clim = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Role, roleName)
            };

            var claimIdentifier = new ClaimsIdentity(clim, CookieAuthenticationDefaults.AuthenticationScheme);

            if (remember)
            {
                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = remember,
                    ExpiresUtc = DateTime.UtcNow.AddDays(15),
                };
                await HttpContext.SignInAsync
                    (
                        CookieAuthenticationDefaults.AuthenticationScheme,
                         new ClaimsPrincipal(claimIdentifier),
                         authProperties
                    );
            }
            else
            {
                var authProperties = new AuthenticationProperties
                {
                    AllowRefresh = true,
                    IsPersistent = remember,
                    ExpiresUtc = DateTime.UtcNow.AddMinutes(30),
                };
                await HttpContext.SignInAsync
                    (
                        CookieAuthenticationDefaults.AuthenticationScheme,
                         new ClaimsPrincipal(claimIdentifier),
                         authProperties
                    );

            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("GetRoleName/{email}")]
        public async Task<string> GetRoleName(string email)
        {
            var user = await _user.FindByEmailAsync(email);
            if (user != null)
            {
                var userRole = await _dbContext.UserRoles.FirstOrDefaultAsync(x => x.UserId == user.Id);
                if (userRole != null)
                {
                    return await _dbContext.Roles.Where(x => x.Id == userRole.RoleId).Select(x => x.Name).FirstOrDefaultAsync();
                }
            }
            
            return null;
        }

    }   // end Main Class
}
