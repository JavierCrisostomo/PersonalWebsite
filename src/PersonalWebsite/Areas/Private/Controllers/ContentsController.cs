using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalWebsite.Repositories;
using PersonalWebsite.ViewModels.Content;

namespace PersonalWebsite.Areas.Private.Controllers
{
    [Authorize]
    [Area(nameof(Private))]
    public class ContentsController : Controller
    {
        private readonly IContentEditorRepository _contentEditorRepository;

        public ContentsController(IContentEditorRepository contentEditorRepository)
        {
            if(contentEditorRepository == null)
            {
                throw new ArgumentNullException(nameof(contentEditorRepository));
            }

            _contentEditorRepository = contentEditorRepository;
        }

        public IActionResult Index()
        {
            ContentIndexViewModel content;
            using(_contentEditorRepository)
            {
                content = _contentEditorRepository.ReadList();
            }
            return View(content);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ContentEditViewModel content)
        {
            if (ModelState.IsValid)
            {
                using (_contentEditorRepository)
                {
                    _contentEditorRepository.Create(content);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(content);
        }

        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ContentEditViewModel content;

            using(_contentEditorRepository)
            {
                content = _contentEditorRepository.Read(id.Value);
            }

            if (content == null)
            {
                return NotFound();
            }
            return View(content);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ContentEditViewModel content)
        {
            if (ModelState.IsValid)
            {
                using(_contentEditorRepository)
                {
                    _contentEditorRepository.Update(content);
                }

                return RedirectToAction(nameof(Edit), new { id = content.Id });
            }
            return View(content);
        }

        [ActionName(nameof(Delete))]
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ContentEditViewModel content;

            using(_contentEditorRepository)
            {
                content = _contentEditorRepository.Read(id.Value);
            }
            
            if (content == null)
            {
                return NotFound();
            }

            return View(content);
        }

        [HttpPost, ActionName(nameof(Delete))]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            using(_contentEditorRepository)
            {
                _contentEditorRepository.Delete(id);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
