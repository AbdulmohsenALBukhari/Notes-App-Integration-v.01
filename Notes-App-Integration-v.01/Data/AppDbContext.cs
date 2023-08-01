using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Notes_App_Integration_v._01.Model;

namespace Notes_App_Integration_v._01.Data
{
    public class AppDbContext : IdentityDbContext<AccountUserModel,AccountRoleModel,string >
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
        public DbSet<NotesModel> Notes { get; set; }

    }
}
