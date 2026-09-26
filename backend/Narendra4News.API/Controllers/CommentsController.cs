using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Narendra4News.API.Data;
using Narendra4News.API.Models;
using System.Security.Claims;
namespace Narendra4News.API.Controllers;
[ApiController][Route("api")]
public class CommentsController(AppDbContext db):ControllerBase{
 public record CommentInput(string Body);
 [HttpGet("articles/{articleId:int}/comments")]public async Task<IActionResult> List(int articleId,CancellationToken ct)=>Ok(new{success=true,data=await db.Comments.AsNoTracking().Where(x=>x.ArticleId==articleId&&x.Status==ModerationStatus.Approved).OrderByDescending(x=>x.CreatedAt).Take(100).Select(x=>new{x.Id,x.Body,x.CreatedAt}).ToListAsync(ct)});
 [Authorize][HttpPost("articles/{articleId:int}/comments")]public async Task<IActionResult> Add(int articleId,CommentInput input,CancellationToken ct){if(string.IsNullOrWhiteSpace(input.Body)||input.Body.Length>2000)return BadRequest(new{success=false,message="Comment must contain 1–2000 characters"});if(!await db.Articles.AnyAsync(x=>x.Id==articleId&&x.Status==ArticleStatus.Published,ct))return NotFound();var x=new Comment{ArticleId=articleId,UserId=User.FindFirstValue(ClaimTypes.NameIdentifier)??"",Body=input.Body.Trim()};db.Comments.Add(x);await db.SaveChangesAsync(ct);return Ok(new{success=true,message="Comment is awaiting approval",data=x.Id});}
 [Authorize(Roles="Admin")][HttpGet("admin/comments")]public async Task<IActionResult> Pending(CancellationToken ct)=>Ok(new{success=true,data=await db.Comments.AsNoTracking().Where(x=>x.Status==ModerationStatus.Pending).OrderByDescending(x=>x.CreatedAt).Take(100).ToListAsync(ct)});
 [Authorize(Roles="Admin")][HttpPut("comments/{id:int}/status")]public async Task<IActionResult> Moderate(int id,ModerationStatus status,CancellationToken ct){var x=await db.Comments.FindAsync([id],ct);if(x is null)return NotFound();x.Status=status;await db.SaveChangesAsync(ct);return Ok(new{success=true});}
 [Authorize(Roles="Admin")][HttpDelete("comments/{id:int}")]public async Task<IActionResult> Delete(int id,CancellationToken ct){var x=await db.Comments.FindAsync([id],ct);if(x is null)return NotFound();db.Comments.Remove(x);await db.SaveChangesAsync(ct);return Ok(new{success=true});}
}
