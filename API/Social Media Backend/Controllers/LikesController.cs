using Azure;
using Dating_App_Backend.DTOs;
using Dating_App_Backend.Entities;
using Dating_App_Backend.Extensions;
using Dating_App_Backend.Helper;
using Dating_App_Backend.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Text.Json;

namespace Dating_App_Backend.Controllers
{
    public class LikesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikesRepository _likesRepository;
        public LikesController(ILikesRepository likesRepository, IUserRepository userRepository)
        {

            _likesRepository = likesRepository;
            _userRepository = userRepository;
        }



        [HttpPost("{username}")]
        public async Task<ActionResult<string>> AddLike(string username)
        {
            var sourceUserId = User.GetUserId();
            var sourceUser = await _likesRepository.GetUserWithLikes(sourceUserId);


            var targetUser = await _userRepository.GetUserByNameAsync(username);

            if (targetUser == null)
            {
                return NotFound();
            }

            if (sourceUser.UserName == username)
            {
                return BadRequest("You cannot like yourself");
            }

            ///check if it's already like that user
            
            var userLike = await _likesRepository.GetUserLike(sourceUserId, targetUser.Id);

            if (userLike != null)
            {
                ///remove like
                sourceUser.LikedUsers.Remove(userLike);
                if (await _userRepository.SaveAllAsync())
                {
                    return Ok(JsonConvert.SerializeObject("removed"));
                }
                return BadRequest("Failde to remove like for this user !!!");
            }

            /// add user process

            userLike = new UserLike
            {
                SourceUserId = sourceUserId,
                TargetUserId = targetUser.Id
            };

            sourceUser.LikedUsers.Add(userLike);

            if (await _userRepository.SaveAllAsync())
            {
                return Ok(JsonConvert.SerializeObject("added"));
            }


            return BadRequest("Failed to like user!!");
        }



        [HttpGet]
        public async Task<ActionResult<PagedList<LikeDto>>> GetLikesUser([FromQuery]LikeParams likeParams)
        {
            likeParams.UserId = User.GetUserId();

            var users = await _likesRepository.GetUserLikes(likeParams);

            Response.AddPaginationHeader(new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages));

            return Ok(users);
        }
    }
}
