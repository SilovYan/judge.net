﻿using System.Security.Authentication;
using System.Web;
using System.Web.Mvc;
using Judge.Application.Interfaces;
using Judge.Application.ViewModels;
using Judge.Application.ViewModels.Submit;
using Judge.Web.Services;
using Microsoft.AspNet.Identity;

namespace Judge.Web.Controllers
{
    public class ProblemsController : Controller
    {
        private readonly IProblemsService _problemsService;
        private readonly ISubmitSolutionService _submitSolutionService;
        private readonly ISessionService _sessionService;
        private int _pageSize = 20;

        public ProblemsController(IProblemsService problemsService, ISubmitSolutionService submitSolutionService, ISessionService sessionService)
        {
            _problemsService = problemsService;
            _submitSolutionService = submitSolutionService;
            _sessionService = sessionService;
        }


        public ActionResult Index(int? page)
        {
            long? userId = null;
            if (User.Identity.IsAuthenticated)
            {
                userId = User.Identity.GetUserId<long>();
            }
            var model = _problemsService.GetProblemsList(page ?? 1, _pageSize, userId, false);
            return View(model);
        }

        public ActionResult Statement(long id)
        {
            var model = _problemsService.GetStatement(id);
            if (model == null)
                return HttpNotFound();

            return View(model);
        }

        [HttpGet]
        public PartialViewResult SubmitSolution(long problemId)
        {
            var languages = _submitSolutionService.GetLanguages();
            var model = new SubmitSolutionViewModel
            {
                Languages = languages,
                ProblemId = problemId,
                SelectedLanguage = _sessionService.GetSelectedLanguage()
            };
            return PartialView("Submits/_SubmitSolution", model);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        [Authorize]
        public ActionResult SubmitSolution(SubmitSolutionViewModel model)
        {
            if (ModelState.IsValid)
            {
                model.Success = true;
                var userId = User.Identity.GetUserId<long>();
                var userHost = Request.UserHostAddress;
                var sessionId = Session.SessionID;
                var userInfo = new UserInfo(userId, userHost, sessionId);
                _submitSolutionService.SubmitSolution(model.ProblemId, model.SelectedLanguage, model.File, userInfo);

                _sessionService.SaveSelectedLanguage(model.SelectedLanguage);

                return Redirect(Request.UrlReferrer.ToString());
            }

            model.Success = false;
            model.Languages = _submitSolutionService.GetLanguages();
            return PartialView("Submits/_SubmitSolution", model);
        }

        [Authorize]
        public ActionResult Solution(long submitResultId)
        {
            var userId = User.Identity.GetUserId<long>();

            try
            {
                var model = _submitSolutionService.GetSolution(submitResultId, userId);
                if (model == null)
                    return HttpNotFound();

                return View(model);
            }
            catch (AuthenticationException)
            {
                throw new HttpException(401, "AuthenticationException");
            }
        }
    }
}