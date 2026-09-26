using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Narendra4News.API.Data;
using Narendra4News.API.Models;
using System.Xml.Linq;
namespace Narendra4News.API.Controllers;
[ApiController]
public class SeoController(AppDbContext db,IConfiguration config):ControllerBase {
 [HttpGet("sitemap.xml")]
 public async Task<IActionResult> Sitemap(CancellationToken ct){var origin=(config["PublicOrigin"]??"https://narendra4news.net").TrimEnd('/');var articles=await db.Articles.AsNoTracking().Where(x=>x.Status==ArticleStatus.Published).Select(x=>new{x.Slug,x.UpdatedDate}).ToListAsync(ct);var movies=await db.Movies.AsNoTracking().Select(x=>x.Slug).ToListAsync(ct);XNamespace ns="http://www.sitemaps.org/schemas/sitemap/0.9";var root=new XElement(ns+"urlset",new XElement(ns+"url",new XElement(ns+"loc",origin+"/")));foreach(var a in articles)root.Add(new XElement(ns+"url",new XElement(ns+"loc",origin+"/article/"+Uri.EscapeDataString(a.Slug)),new XElement(ns+"lastmod",a.UpdatedDate.ToString("yyyy-MM-dd"))));foreach(var m in movies)root.Add(new XElement(ns+"url",new XElement(ns+"loc",origin+"/movie/"+Uri.EscapeDataString(m))));return Content(new XDocument(root).ToString(),"application/xml");}
 [HttpGet("robots.txt")]
 public IActionResult Robots()=>Content($"User-agent: *\nDisallow: /admin\nSitemap: {(config["PublicOrigin"]??"https://narendra4news.net").TrimEnd('/')}/sitemap.xml\n","text/plain");
}
