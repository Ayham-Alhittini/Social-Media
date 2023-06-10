using Dating_App_Backend.DTOs;
using Dating_App_Backend.Entities;
using Dating_App_Backend.Extensions;
using Dating_App_Backend.Helper;
using Dating_App_Backend.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dating_App_Backend.Data
{
    public class LikesRepository : ILikesRepository
    {
        private readonly DataContext _context;
        public LikesRepository(DataContext context)
        {
            _context = context;
        }
        public async Task<UserLike> GetUserLike(int sourceId, int targetUserId)
        {
            return await _context.Likes.FindAsync(sourceId, targetUserId);
        }

        public async Task<PagedList<LikeDto>> GetUserLikes(LikeParams likeParams)
        {
            var users = _context.Users.OrderBy(u => u.UserName).AsQueryable();
            var likes = _context.Likes.AsQueryable();

            if (likeParams.Predicate == "liked")
            {
                likes = likes.Where(l => l.SourceUserId == likeParams.UserId);
                users = likes.Select(x => x.TargetUser);
            }
            if (likeParams.Predicate == "likedBy")
            {
                likes = likes.Where(l => l.TargetUserId == likeParams.UserId);
                users = likes.Select(x => x.SourceUser);
            }

            var source =  users.Select(user => new LikeDto
            {
                Id = user.Id,
                UserName = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url
            });

            var result =  await PagedList<LikeDto>.CreateAsync(source, likeParams.PageNumber, likeParams.PageSize);

            foreach(var item in result)
            {
                item.IsLiked = await GetUserLike(likeParams.UserId, item.Id) != null;
            }


            return result;
        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await _context.Users.Include(u => u.LikedUsers).FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
