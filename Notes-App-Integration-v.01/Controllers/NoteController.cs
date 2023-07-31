using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notes_App_Integration_v._01.Data;
using Notes_App_Integration_v._01.Model;

namespace Notes_App_Integration_v._01.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class NoteController : Controller
    {
        private readonly AppDbContext context;

        public NoteController(AppDbContext context) 
        {
            this.context = context;
        }
        //Get All Notes
        [HttpGet]
        
        public async Task<IActionResult> GetAllNotes()
        {
            IEnumerable<NotesModel> NotesList = await context.Notes.ToListAsync();
            return Ok(NotesList);
        }
        //Get single Note
        [HttpGet]
        [Route("{Id:Guid}")]
        [ActionName("GetSingleNote")]
        public async Task<IActionResult> GetSingleNote([FromRoute] Guid Id)
        {
            var itme = await context.Notes.SingleOrDefaultAsync(x => x.Id == Id);
            if (itme != null)
            {
                return Ok(itme);
            }
            return NotFound("Note Not found");
        }
        //Post Add Note
        [HttpPost]
        public async Task<IActionResult> AddNote([FromBody] NotesModel Note)
        {
            if (!ModelState.IsValid || Note.title == "string" && Note.content == "string")
            {
                
                return NotFound("bitc");
            }
            else
            {
                Note.Id = Guid.NewGuid();
                await context.Notes.AddAsync(Note);
                await context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetSingleNote), new { Id = Note.Id }, Note);
            }
        }

        //Post Update Note
        [HttpPut]
        [Route("{Id:Guid}")]
        public async Task<IActionResult> UpdateNote([FromRoute] Guid Id,[FromBody] NotesModel Note)
        {
            var UpdateItem = await context.Notes.FirstOrDefaultAsync(x => x.Id == Id);
            if (UpdateItem != null)
            {
                UpdateItem.title = Note.title;
                UpdateItem.content = Note.content;
                await context.SaveChangesAsync();
                return Ok(UpdateItem);
            }
            return NotFound("Note Not found");
        }

        //Delete Note
        [HttpDelete]
        [Route("{Id:Guid}")]
        public async Task<IActionResult> DeleteNote([FromRoute] Guid Id)
        {
            var DeleteItem = await context.Notes.FirstOrDefaultAsync(x => x.Id == Id);
            if (DeleteItem != null)
            {
                context.Remove(DeleteItem);
                await context.SaveChangesAsync();
                return Ok(DeleteItem);
            }
            return NotFound("Note Not found");
        }
    }
}
