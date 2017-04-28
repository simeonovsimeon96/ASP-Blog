using BlogSystem.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace BlogSystem.Controllers
{
    public class ArticleController : Controller
    {
        // GET: Article
        public ActionResult Index()
        {
            return RedirectToAction("List");
        }

        public ActionResult List()
        {
            using (var database = new BlogDbContext())
            {
                var articles = database.Articles
                    .Include(a => a.Author)
                    .ToList();

                return View(articles);
            }
        }

        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            using (var database = new BlogDbContext())
            {

                var article = database.Articles
                    .Where(a => a.Id == id)
                    .Include(a => a.Author)
                    .First();

                if (article == null)
                {
                    return HttpNotFound();
                }
                return View(article);
            }
        }

        
        [Authorize]
        [HttpGet]
        public ActionResult Create()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public ActionResult Create(Article article, HttpPostedFileBase image)
        {

            if (ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    var authorId = database.Users
                        .Where(u => u.UserName == this.User.Identity.Name)
                        .First()
                        .Id;

                   

                    article.AuthorId = authorId;

                    if (image != null)
                    {
                        var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png" };

                        if (allowedContentTypes.Contains(image.ContentType))
                        {
                            var imagePath = "/Content/Images/";

                            var filename = image.FileName;

                            var uploadPath = imagePath + filename;

                            var physicalPath = Server.MapPath(uploadPath);

                            image.SaveAs(physicalPath);

                            article.ImagePath = uploadPath;

                        }
                    }

                    database.Articles.Add(article);
                    database.SaveChanges();

                    return RedirectToAction("Index");

                }
                
            }

            return View(article);
        }

        [Authorize]
        [HttpGet]
        public ActionResult Delete(int id)
        {
            using (var database = new BlogDbContext())
            {
                var article = database.Articles
                    .Where(a => a.Id == id)
                    .FirstOrDefault();
                if (article == null || !IsAuthorised(article))
                {
                    return HttpNotFound();
                }
                return View(article);

            }
        }

        [Authorize]
        [ActionName("Delete")]
        [HttpPost]
        public ActionResult ConfirmDelete(int id)
        {
            using (var database = new BlogDbContext())
            {
                var article = database.Articles
                    .Where(a => a.Id == id)
                    .FirstOrDefault();
                if (article == null || !IsAuthorised(article))
                {
                    return HttpNotFound();
                }

                database.Articles.Remove(article);
                database.SaveChanges();

                return RedirectToAction("Index");
            }
        }

        [Authorize]
        [HttpGet]
        public ActionResult Edit(int id)
        {
            using (var database = new BlogDbContext())
            {
                var article = database.Articles.Find(id);

                if (article == null || !IsAuthorised(article))
                {
                    return HttpNotFound();
                }

                var articleViewModel = new ArticleViewModel();

                articleViewModel.Id = article.Id;
                articleViewModel.Title = article.Title;
                articleViewModel.Content = article.Content;
                articleViewModel.AuthorId = article.AuthorId;
                

            return View(articleViewModel);
            }
        }

        [Authorize]
        [HttpPost]
        public ActionResult Edit(ArticleViewModel model)
        {
            if (ModelState.IsValid)
            {
                using (var database = new BlogDbContext())
                {
                    var article = database.Articles.Find(model.Id);

                    if (article == null || !IsAuthorised(article))
                    {
                        return HttpNotFound();
                    }

                    article.Title = model.Title;
                    article.Content = model.Content;
                    database.SaveChanges();
                }

                return RedirectToAction("Details", new { id = model.Id });
            }
            return View(model);
        }

        private bool IsAuthorised(Article article)
        {
            var isAdmin = this.User.IsInRole("Admin");
            var isAuthor = article.IsAuthor(this.User.Identity.GetUserId());

            return isAdmin || isAuthor;
        }

    }
}