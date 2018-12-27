using System;
using System.IO;
using System.Reflection;
using System.Web.Hosting;
using System.Web.Mvc;
using Markdig;

namespace DiscImageChef.Server.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            StreamReader sr =
                new StreamReader(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(),
                                              "docs", "README.md"));
            string mdcontent = sr.ReadToEnd();
            sr.Close();

            mdcontent = mdcontent.Replace(".md)", ")");

            ViewBag.Markdown = Markdown.ToHtml(mdcontent);

            ViewBag.lblVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            return View();
        }

        public ActionResult Changelog()
        {
            StreamReader sr =
                new StreamReader(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(),
                                              "docs", "Changelog.md"));
            string mdcontent = sr.ReadToEnd();
            sr.Close();

            mdcontent = mdcontent.Replace(".md)", ")");

            ViewBag.Markdown = Markdown.ToHtml(mdcontent);

            ViewBag.lblVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            return View();
        }

        public ActionResult DONATING()
        {
            StreamReader sr =
                new StreamReader(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(),
                                              "docs", "DONATING.md"));
            string mdcontent = sr.ReadToEnd();
            sr.Close();

            mdcontent = mdcontent.Replace(".md)", ")");

            ViewBag.Markdown = Markdown.ToHtml(mdcontent);

            ViewBag.lblVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            return View();
        }

        public ActionResult TODO()
        {
            StreamReader sr =
                new StreamReader(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(),
                                              "docs", "TODO.md"));
            string mdcontent = sr.ReadToEnd();
            sr.Close();

            mdcontent = mdcontent.Replace(".md)", ")");

            ViewBag.Markdown = Markdown.ToHtml(mdcontent);

            ViewBag.lblVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            return View();
        }
    }
}