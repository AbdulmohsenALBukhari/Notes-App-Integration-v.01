using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes_App_Integration_v._01.Data;
using Notes_App_Integration_v._01.Model;
using Notes_App_Integration_v._01.ModelViews;

namespace Notes_App_Integration_v._01.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<AccountUserModel> _user;

        public AccountController(AppDbContext dbContext,UserManager<AccountUserModel> user) 
        {
            this._dbContext = dbContext;
            this._user = user;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllUserAsync ()
        {
            IEnumerable<AccountUserModel> NotesList = await _dbContext.Users.ToListAsync();
            return Ok(NotesList);
        }
        [HttpPost]
        [Route("Rigister")]
        public async Task<IActionResult> Rigister(RegisterModel model)
        {
            if(ModelState.IsValid)
            {
                if (model == null)
                {
                    return NotFound();
                }
                if (Existes(model.Email,model.UserName))
                {
                    return BadRequest("Emil or user is Existes");
                }
                var User = new AccountUserModel
                {
                    Email = model.Email,
                    UserName = model.UserName,
                    PasswordHash = model.PasswordHash,
                };
                if(model.PasswordHash == null)
                {
                    return BadRequest("Eroor");
                }
                var result = await _user.CreateAsync(User);
                if (result.Succeeded)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result.Errors);
                }
            }
            return BadRequest("Bad Bitch");
        }
        public bool Existes(string email, string userName)
        {
           return _dbContext.Users.Any(u => u.Email == email || u.UserName == userName);
        }

    }
}
