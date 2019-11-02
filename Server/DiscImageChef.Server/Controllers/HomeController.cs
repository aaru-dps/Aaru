// /***************************************************************************
// The Disc Image Chef
// ----------------------------------------------------------------------------
//
// Filename       : HomeController.cs
// Author(s)      : Natalia Portillo <claunia@claunia.com>
//
// Component      : DiscImageChef Server.
//
// --[ Description ] ----------------------------------------------------------
//
//     Provides documentation data for razor views.
//
// --[ License ] --------------------------------------------------------------
//
//     This library is free software; you can redistribute it and/or modify
//     it under the terms of the GNU Lesser General Public License as
//     published by the Free Software Foundation; either version 2.1 of the
//     License, or (at your option) any later version.
//
//     This library is distributed in the hope that it will be useful, but
//     WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
//     Lesser General Public License for more details.
//
//     You should have received a copy of the GNU Lesser General Public
//     License along with this library; if not, see <http://www.gnu.org/licenses/>.
//
// ----------------------------------------------------------------------------
// Copyright © 2011-2019 Natalia Portillo
// ****************************************************************************/

using System;
using System.IO;
using System.Reflection;
using System.Web.Hosting;
using System.Web.Mvc;
using Markdig;

namespace DiscImageChef.Server.Controllers
{
    [RoutePrefix("Home")]
    public class HomeController : Controller
    {
        [Route("")]
        [Route("~/")]
        [Route("README")]
        [Route("~/README")]
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

        [Route("Changelog")]
        [Route("~/Changelog")]
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

        [Route("CODE_OF_CONDUCT")]
        [Route("~/CODE_OF_CONDUCT")]
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

        [Route("PULL_REQUEST_TEMPLATE")]
        [Route("~/PULL_REQUEST_TEMPLATE")]
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

        [Route("ISSUE_TEMPLATE")]
        [Route("~/ISSUE_TEMPLATE")]
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

        [Route("CONTRIBUTING")]
        [Route("~/CONTRIBUTING")]
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

        [Route("DONATING")]
        [Route("~/DONATING")]
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

        [Route("TODO")]
        [Route("~/TODO")]
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