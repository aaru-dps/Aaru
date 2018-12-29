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

        public ActionResult CODE_OF_CONDUCT()
        {
            StreamReader sr =
                new StreamReader(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(),
                                              "docs", "CODE_OF_CONDUCT.md"));
            string mdcontent = sr.ReadToEnd();
            sr.Close();

            mdcontent = mdcontent.Replace(".md)", ")").Replace("(.github/", "(");

            ViewBag.Markdown = Markdown.ToHtml(mdcontent);

            ViewBag.lblVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            return View();
        }

        public ActionResult PULL_REQUEST_TEMPLATE()
        {
            StreamReader sr =
                new StreamReader(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(),
                                              "docs", "PULL_REQUEST_TEMPLATE.md"));
            string mdcontent = sr.ReadToEnd();
            sr.Close();

            mdcontent = mdcontent.Replace(".md)", ")").Replace("(.github/", "(");

            ViewBag.Markdown = Markdown.ToHtml(mdcontent);

            ViewBag.lblVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            return View();
        }

        public ActionResult ISSUE_TEMPLATE()
        {
            StreamReader sr =
                new StreamReader(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(),
                                              "docs", "ISSUE_TEMPLATE.md"));
            string mdcontent = sr.ReadToEnd();
            sr.Close();

            mdcontent = mdcontent.Replace(".md)", ")").Replace("(.github/", "(");

            ViewBag.Markdown = Markdown.ToHtml(mdcontent);

            ViewBag.lblVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            return View();
        }

        public ActionResult CONTRIBUTING()
        {
            StreamReader sr =
                new StreamReader(Path.Combine(HostingEnvironment.MapPath("~") ?? throw new InvalidOperationException(),
                                              "docs", "CONTRIBUTING.md"));
            string mdcontent = sr.ReadToEnd();
            sr.Close();

            mdcontent = mdcontent.Replace(".md)", ")").Replace("(.github/", "(");

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