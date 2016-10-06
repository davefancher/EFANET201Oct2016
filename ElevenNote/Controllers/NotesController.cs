using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ElevenNote.Models;
using ElevenNote.Services;
using Microsoft.AspNet.Identity;

namespace ElevenNote.Controllers
{
    [Authorize]
    public class NotesController : Controller
    {
        private readonly Lazy<NoteService> _svc;

        public NotesController()
        {
            _svc =
                new Lazy<NoteService>(
                    () =>
                    {
                        var userId = Guid.Parse(User.Identity.GetUserId());
                        return new NoteService(userId);
                    }
                );
        }

        public ActionResult Index()
        {
            var notes = _svc.Value.GetNotes();

            return View(notes);
        }

        public ActionResult Create()
        {
            var model = new NoteCreateModel();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(NoteCreateModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if(!_svc.Value.CreateNote(model))
            {
                ModelState.AddModelError("", "Unable to create note");
                return View(model);
            }

            TempData["SaveResult"] = "Your note was created!";

            return RedirectToAction("Index");
        }

        public ActionResult Details(int id)
        {
            var note = _svc.Value.GetNoteById(id);

            return View(note);
        }

        public ActionResult Edit(int id)
        {
            var details = _svc.Value.GetNoteById(id);
            var note =
                NoteService.CopyProperties<NoteDetailModel, NoteEditModel>(details);

            return View(note);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, NoteEditModel model)
        {
            if (!ModelState.IsValid) return View(model);

            if (id != model.NoteId)
            {
                ModelState.AddModelError("", "C'mon man");
                return View(model);
            }

            if(!_svc.Value.UpdateNote(model))
            {
                ModelState.AddModelError("", "Unable to update note");
                return View(model);
            }

            TempData["SaveResult"] = "Your note was saved";

            return RedirectToAction("Index");
        }

        [HttpGet]
        [ActionName("Delete")]
        public ActionResult DeleteGet(int id)
        {
            var detail = _svc.Value.GetNoteById(id);

            return View(detail);
        }

        [HttpPost]
        [ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePost(int id, int noteId)
        {
            _svc.Value.DeleteNote(id);

            TempData["SaveResult"] = "Your note was deleted";

            return RedirectToAction("Index");
        }
    }
}
