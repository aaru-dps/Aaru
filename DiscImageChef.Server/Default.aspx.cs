using System;
using System.Web;
using System.Web.UI;
using Velyo.AspNet.Markdown;
using System.IO;

namespace DiscImageChef.Server
{

    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            MarkdownContent mkdown = new MarkdownContent();
            StreamReader sr = new StreamReader("docs/README.md");
            string mdcontent = sr.ReadToEnd();
            sr.Close();

            mkdown.Content = mdcontent.Replace(".md)", ".aspx)");
            body.Controls.Add(mkdown);

            lblVersion.Text = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
        }
    }
}
