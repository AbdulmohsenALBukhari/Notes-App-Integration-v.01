using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Notes_App_Integration_v._01.Data;
using Notes_App_Integration_v._01.Model;
using Notes_App_Integration_v._01.ModelViews;
using System.Web;

namespace Notes_App_Integration_v._01.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<AccountUserModel> _user;
        private readonly SignInManager<AccountUserModel> signInManager;

        public AccountController(AppDbContext dbContext,UserManager<AccountUserModel> userManager,SignInManager<AccountUserModel> signInManager) 
        {
            this._dbContext = dbContext;
            this._user = userManager;
            this.signInManager = signInManager;
        }
        //Get All user
        [HttpGet]
        public async Task<IActionResult> GetAllUserAsync ()
        {
            IEnumerable<AccountUserModel> NotesList = await _dbContext.Users.ToListAsync();
            return Ok(NotesList);
        }
        //Create Register
        [HttpPost]
        [Route("Rigister")]
        public async Task<IActionResult> Rigister(RegisterModel model)
        {
            // check all valid
            if(ModelState.IsValid)
            {
                //  conditions Email
                if (model.Email.IsNullOrEmpty() || !model.Email.Contains("@") || !model.Email.Contains("."))
                {
                    return NotFound();
                }
                //  conditions Email and user name if Existes or not
                if (Existes(model.Email,model.UserName))
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
                var result = await _user.CreateAsync(User,model.PasswordHash);
                // conditions result is true or false
                if (result.Succeeded)
                {
                    var token = await _user.GenerateEmailConfirmationTokenAsync(User); // Create token form User
                    var confirmLink = Url.Action("RegistreationConfirm", "Account", new  // Create Link and can send to email to [EmailConfirmed] 
                    {
                        Id = User.Id, Token = HttpUtility.UrlEncode(token)
                    },
                        Request.Scheme);
                    return Ok(confirmLink); // link [EmailConfirmed] here
                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }
            return BadRequest("Bad Bitch");
        }
        // Metod check email and user name
        public bool Existes(string email, string userName)
        {
           return _dbContext.Users.Any(u => u.Email == email || u.UserName == userName);
        }
        //  Metod check if user open link and after send 1 in database change [EmailConfirmed] to = 1
        //  this metod run if user open link [confirmLink] 
        [HttpGet]
        [Route("RegistreationConfirm")]
       public async Task<IActionResult> RegistreationConfirm(string id,string Token)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(Token))
            {
                return BadRequest();
            }
            var user = await _user.FindByIdAsync(id);
            if (user == null)
            {
                return BadRequest();
            }
            var result = await _user.ConfirmEmailAsync(user, HttpUtility.UrlDecode(Token));
            if (result.Succeeded)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result.Errors);
            }
        }

    }

}
