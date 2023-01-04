using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CloudNotes.Models;
using CloudNotes.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace CloudNotes.Controllers
{
     [Authorize]
    public class NotesController : Controller
    {
        private INoteStorageService _noteStorageService;
        private IEventPublisher _eventPublisher;
        private IMemoryCache _cache;
        private readonly ILogger<NotesController> _logger;

        public NotesController(
            INoteStorageService noteStorageService,
            IEventPublisher eventPublisher,
            IMemoryCache memoryCache,
            ILogger<NotesController> logger)
        {
            _noteStorageService = noteStorageService;
            _eventPublisher = eventPublisher;
            _cache = memoryCache;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult New()
        {
            _logger.LogInformation("Creating new note");

            ViewData["Title"] = "Create a New Note";

            return View("Edit");
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            Note note = await _noteStorageService.GetNote(User.Identity.Name, id);

            if (note == null)
            {
                _logger.LogWarning($"Coulnd't find note with ID '{id}' for user '{User.Identity.Name}'");
                return NotFound();
            }

            _logger.LogInformation($"Editing note with ID '{id}' for user '{User.Identity.Name}'");

            await _eventPublisher.PublishEvent(new Event
            {
                EventType = EventType.NoteViewed,
                TimeStamp = DateTime.UtcNow,
                Username = User.Identity.Name,
                NoteId = note.NoteId.Value
            });

            ViewData["Title"] = $"Edit Note - {note.Title}";

            return View("Edit", note);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Note note)
        {
            EventType eventType = EventType.NoteEdited;

            if (!note.NoteId.HasValue)
            {
                note.NoteId = Guid.NewGuid();
                note.CreatedAt = DateTime.UtcNow;
                eventType = EventType.NoteCreated;

                _logger.LogInformation($"Creating new note with ID '{note.NoteId.Value}' for user '{User.Identity.Name}'");
            }
            else
            {
                // check the owner of the note
                Note originalNote = await _noteStorageService.GetNote(User.Identity.Name, note.NoteId.Value);

                if (originalNote == null)
                {
                    _logger.LogWarning($"Coulnd't find note with ID '{note.NoteId.Value}' for user '{User.Identity.Name}'");
                    return NotFound();
                }

                _logger.LogInformation($"Saving changes to existing note with ID '{note.NoteId.Value}' for user '{User.Identity.Name}'");
            }

            // reset the user name as we were not displaying it on the page on purpose
            note.UserId = User.Identity.Name;

            await _noteStorageService.SaveNote(note);

            await _eventPublisher.PublishEvent(new Event
            {
                EventType = eventType,
                TimeStamp = DateTime.UtcNow,
                Username = User.Identity.Name,
                NoteId = note.NoteId.Value
            });

            return RedirectToAction("List");
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            ViewData["Title"] = "Your Notes";

            _logger.LogWarning($"Getting note list for user '{User.Identity.Name}'");

            List<NoteSummary> notes = await _noteStorageService.GetNoteList(User.Identity.Name);

            return View(notes);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            // check the owner of the note
            Note originalNote = await _noteStorageService.GetNote(User.Identity.Name, id); //

            if (originalNote == null)
            {
                //_logger.LogWarning($"Coulnd't find note with ID '{id}' for user '{User.Identity.Name}'");
                return NotFound();
            }

            await _noteStorageService.DeleteNote(User.Identity.Name, id);

            //_logger.LogInformation($"Deleting note with ID '{id}' for user '{User.Identity.Name}'");

            await _eventPublisher.PublishEvent(new Event
            {
                EventType = EventType.NoteDeleted,
                TimeStamp = DateTime.UtcNow,
                Username = User.Identity.Name,
                NoteId = id
            });

            return RedirectToAction("List");
        }
    }
}
