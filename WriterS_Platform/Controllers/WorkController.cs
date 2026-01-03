// File: Controllers/WorkController.cs

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using WriterS_Platform.Services;
using WriterS_Platform.Models;
using WriterS_Platform.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

[Authorize]
public class WorkController : Controller
{
    private readonly IWorkService _workService;
    private readonly IUser _userService; // Нужен для получения имени пользователя для комментариев

    public WorkController(IWorkService workService, IUser userService)
    {
        _workService = workService;
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string searchTerm, string genre, string sortBy, int pageNumber = 1)
    {
        ViewData["Title"] = "Все Произведения";
        int pageSize = 10; // Количество произведений на странице

        var totalWorksCount = await _workService.GetTotalWorksCountAsync(searchTerm, genre);
        var works = await _workService.SearchWorksAsync(searchTerm, genre, sortBy, pageNumber, pageSize);

        ViewData["CurrentSort"] = sortBy;
        ViewData["CurrentFilter"] = searchTerm;
        ViewData["CurrentGenre"] = genre;

        var pagedWorks = new PagedResult<WorkViewModel>
        {
            Items = works,
            TotalCount = totalWorksCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };

        return View(pagedWorks);
    }

    // GET: /Work/Create (Отображает форму создания)
    [HttpGet]
    public IActionResult Create()
    {
        ViewData["Title"] = "Создать Произведение";
        return View();
    }

    // POST: /Work/Create (Принимает форму создания)
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkViewModel model)
    {
        if (ModelState.IsValid)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int authorId))
            {
                ModelState.AddModelError(string.Empty, "Не удалось определить автора произведения.");
                return View(model);
            }

            var work = new Work
            {
                Title = model.Title,
                Content = model.Content,
                Genre = model.Genre,
                PublicationDate = DateTime.UtcNow,
                AvgRating = 0,
                AuthorID = authorId
            };

            var newWorkId = await _workService.CreateWorkAsync(work);
            if (newWorkId > 0)
            {
                return RedirectToAction("Details", new { id = newWorkId });
            }
            ModelState.AddModelError(string.Empty, "Не удалось опубликовать произведение.");
        }
        return View(model);
    }

    // GET: /Work/Details/{id} (Просмотр одной работы)
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        ViewData["Title"] = "Детали Произведения";
        var work = await _workService.GetWorkByIdAsync(id);
        if (work == null)
        {
            return NotFound();
        }

        var comments = await _workService.GetCommentsByWorkIdAsync(id);

        int userRating = -1;
        if (User.Identity.IsAuthenticated)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(userIdString, out int userId))
            {
                userRating = await _workService.GetUserRatingForWorkAsync(work.WorkID, userId);
            }
        }

        var detailsViewModel = new WorkDetailsViewModel
        {
            Work = work,
            Comments = comments,
            CurrentUserRating = userRating,
            NewComment = new CommentViewModel { WorkID = work.WorkID },
            NewRating = new RatingViewModel { WorkID = work.WorkID, Value = userRating == -1 ? 0 : userRating }
        };

        return View(detailsViewModel);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(WorkDetailsViewModel model)
    {
        if (ModelState.IsValid)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                ModelState.AddModelError(string.Empty, "Не удалось определить пользователя.");
                return RedirectToAction("Details", new { id = model.NewComment.WorkID });
            }

            var comment = new Comment
            {
                WorkID = model.NewComment.WorkID,
                UserId = userId,
                Content = model.NewComment.Content,
                CommentDate = DateTime.UtcNow
            };

            var newCommentId = await _workService.AddCommentAsync(comment);
            if (newCommentId > 0)
            {
                return RedirectToAction("Details", new { id = model.NewComment.WorkID });
            }
            ModelState.AddModelError(string.Empty, "Не удалось добавить комментарий.");
        }
        // Если модель невалидна, перенаправляем обратно на страницу деталей
        return RedirectToAction("Details", new { id = model.NewComment.WorkID });
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddRating(WorkDetailsViewModel model)
    {
        if (ModelState.IsValid)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                ModelState.AddModelError(string.Empty, "Не удалось определить пользователя.");
                return RedirectToAction("Details", new { id = model.NewRating.WorkID });
            }

            var rating = new Rating
            {
                WorkID = model.NewRating.WorkID,
                UserId = userId,
                Value = model.NewRating.Value
            };

            var ratingAdded = await _workService.AddRatingAsync(rating);
            if (ratingAdded)
            {
                await _workService.UpdateWorkAvgRatingAsync(model.NewRating.WorkID);
                return RedirectToAction("Details", new { id = model.NewRating.WorkID });
            }
            ModelState.AddModelError(string.Empty, "Не удалось добавить оценку.");
        }
        return RedirectToAction("Details", new { id = model.NewRating.WorkID });
    }
}