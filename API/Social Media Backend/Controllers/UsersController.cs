using AutoMapper;
using Dating_App_Backend.Data;
using Dating_App_Backend.DTOs;
using Dating_App_Backend.Entities;
using Dating_App_Backend.Extensions;
using Dating_App_Backend.Helper;
using Dating_App_Backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dating_App_Backend.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {

        private readonly DataContext _context;

        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IPhotoService _photoService;
        private readonly ILikesRepository _likesRepository;
        public UsersController(IUserRepository userRepository, IMapper mapper, IPhotoService photoService, ILikesRepository likesRepository, DataContext context)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _photoService = photoService;
            _likesRepository = likesRepository;

            _context = context;
        }
        [HttpGet]
        public async Task<ActionResult<PagedList<MemberDto>>> GetUsers([FromQuery]UserParams userParams)
        {
            var currentUser = await _userRepository.GetMemberAsync(User.GetUsername());

            userParams.CurrentUserName = currentUser.UserName;
            userParams.UserId = currentUser.Id;

            if (string.IsNullOrEmpty(userParams.Gender))
            {
                userParams.Gender = currentUser.Gender == "male" ? "female" : "male";
            }

            var users = await _userRepository.GetMembersAsync(userParams);


            Response.AddPaginationHeader(
                new PaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages)
            );

            return Ok(users);
        }


        [HttpGet("{username}")]
        public async Task<ActionResult<MemberDto>> GetUser(string username)
        {
            var result =  await _userRepository.GetMemberAsync(username);

            if (result == null)
                return NotFound("Member Not Exist");
            result.IsLiked = await _likesRepository.GetUserLike(User.GetUserId(), result.Id) != null;

            return Ok(result);
        }


        [HttpPut]
        public async Task<ActionResult>UpdateMember(MemberUpdateDto memberUpdateDto)
        {
            var user = await _userRepository.GetUserByNameAsync(User.GetUsername());

            if (user == null)
            {
                return NotFound();
            }

            _mapper.Map(memberUpdateDto, user);

            if (await _userRepository.SaveAllAsync())
            {
                return NoContent();
            }

            return BadRequest("Failed to update user");
        }


        [HttpPost("{add-photo}")]
        public async Task<ActionResult<PhotoDto>>AddPhoto(IFormFile file)
        {
            var user = await _userRepository.GetUserByNameAsync(User.GetUsername());
            if (user == null)
            {
                return NotFound();
            }
            var result = await _photoService.AddPhotoAsync(file);
            
            if (result.Error != null)
            {
                return BadRequest(result.Error.Message);
            }
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };
            if (user.Photos.Count == 0) 
            {
                photo.IsMain = true;
            }
            user.Photos.Add(photo);

            if (await _userRepository.SaveAllAsync())
            {
                return CreatedAtAction(nameof(GetUser), 
                    new { username = user.UserName }, _mapper.Map<PhotoDto>(photo));
            }
            return BadRequest("Can't Add Photo");
        }

        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await _userRepository.GetUserByNameAsync(User.GetUsername());

            if (user == null)
            {
                return NotFound();
            }

            var currentMain = user.Photos.FirstOrDefault(photo => photo.IsMain);

            if (currentMain != null)
                currentMain.IsMain = false;

            var photo = user.Photos.Find(photo => photo.Id == photoId);

            if (photo == null)
            {
                return NotFound();
            }

            if (photo.IsMain)
            {
                return BadRequest("Photo is already main");
            }

            photo.IsMain = true;

            if (await _userRepository.SaveAllAsync())
            {
                return NoContent();
            }
            return BadRequest("set main photo went wrong");
        }

        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult<PhotoDto>> DeletePhoto(int photoId)
        {
            var user = await _userRepository.GetUserByNameAsync(User.GetUsername());

            var photo = user.Photos.FirstOrDefault(x => x.Id == photoId);

            if (photo == null)
            {
                return NotFound();
            }
            if (photo.PublicId != null)
            {
                var result = await _photoService.DeletePhotoAsync(photo.PublicId);

                if (result.Error != null)
                {
                    return BadRequest(result.Error.Message);
                }
            }

            user.Photos.Remove(photo);

            if (await _userRepository.SaveAllAsync())
            {
                return Ok();
            }

            return BadRequest("delete photo went wrong");
        }
    }
}
