using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes_App_Integration_v._01.Data;
using Notes_App_Integration_v._01.Model;

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
        public async Task<IActionResult> Rigister(AccountUserModel model)
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
                    UserName = model.UserName,
                    Email = model.Email,
                    password = model.password,
                    ConfirmPassword = model.ConfirmPassword,
                };
                var result = await _user.CreateAsync(User);
                if (result.Succeeded)
                {
                    return Ok(result);
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
