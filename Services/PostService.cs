using Castle.Core.Internal;
using GlassECommerce.Data;
using GlassECommerce.DTOs;
using GlassECommerce.Models;
using GlassECommerce.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GlassECommerce.Services
{
    public class PostService : ControllerBase, IPostService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly IAuthenticationService _authService;

        public PostService(ApplicationDbContext dbContext, IAuthenticationService authService)
        {
            _dbContext = dbContext;
            _authService = authService;
        }

        public async Task<IActionResult> GetAllPosts(int page)
        {
            var posts = page != 0 ? await _dbContext.Posts.Skip(10 * (page - 1)).Take(10).ToListAsync() : 
                                    await _dbContext.Posts.ToListAsync();
            if (posts.IsNullOrEmpty()) return Ok(new List<Post>());
            return Ok(posts.Select(p => new
            {
                PostId = p.PostId,
                CreatedBy = p.User.ToString(),
                CreationDate = p.CreationDate,
                LastestUpdate = p.LatestUpdate,
                Title = p.Title,
                Content = p.Content,
                Thumbnail = p?.Thumbnail
            }));
        }

        public async Task<IActionResult> GetPostById(int postId)
        {
            var post = await _dbContext.Posts.FindAsync(postId);
            if (post == null) return NotFound(new Response("Error", $"The post with id = {postId} was not found"));
            return Ok(new
            {
                PostId = post.PostId,
                CreatedBy = post.User.ToString(),
                CreationDate = post.CreationDate,
                LastestUpdate = post.LatestUpdate,
                Title = post.Title,
                Content = post.Content,
                Thumbnail = post?.Thumbnail

            });
        }

        public async Task<IActionResult> AddPost(string userId, PostDTO model)
        {
            try
            {
                Post newPost = new Post()
                {
                    UserId = userId,
                    Title = model.Title,
                    Content = model.Description,
                    Thumbnail = model.Thumbnail,
                    CreationDate = DateTime.Now,
                    LatestUpdate = DateTime.Now
                };
                await _dbContext.AddAsync(newPost);
                await _dbContext.SaveChangesAsync();
                return Created("New post created", new
                {
                    PostId = newPost.PostId,
                    CreateBy = newPost.User.ToString(),
                    Thumbnail = newPost.Thumbnail,
                    Title = newPost.Title,
                    Content = newPost.Content,
                    CreationDate = newPost.CreationDate,
                    LatestUpdate = newPost.LatestUpdate
                });
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                   new Response("Error", "An error occurs when adding new post"));
            }
        }

        public async Task<IActionResult> EditPost(Post postExist, PostDTO model)
        {

            try
            {
                postExist.Thumbnail = model.Thumbnail;
                postExist.Title = model.Title;
                postExist.Content = model.Description;
                postExist.LatestUpdate = DateTime.Now;
                _dbContext.Update(postExist);
                await _dbContext.SaveChangesAsync();
                return Ok(new
                {
                    PostId = postExist.PostId,
                    CreatedBy = postExist.User.ToString(),
                    Title = postExist.Title,
                    Content = postExist.Content,
                    CreationDate = postExist.CreationDate,
                    LatestUpdate = postExist.LatestUpdate
                });
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                   new Response("Error", "An error occurs when editing post"));
            }
        }

        public async Task<IActionResult> RemovePost(int postId)
        {
            try
            {
                Post postExist = await _dbContext.Posts.FindAsync(postId);
                if (postExist == null) return NotFound(new Response("Error", $"The post with id = {postId} was not found"));
                _dbContext.Remove(postExist);
                await _dbContext.SaveChangesAsync();
                return NoContent();
            }
            catch
            {
                return StatusCode(StatusCodes.Status500InternalServerError,
                  new Response("Error", "An error occurs when removing post"));
            }
        }

    }
}
