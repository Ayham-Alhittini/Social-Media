using AutoMapper;
using AutoMapper.QueryableExtensions;
using Dating_App_Backend.Data;
using Dating_App_Backend.DTOs;
using Dating_App_Backend.Entities;
using Dating_App_Backend.Extensions;
using Dating_App_Backend.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Dating_App_Backend.Controllers
{
    [Authorize]
    public class ChatGroupController: BaseApiController
    {
        private readonly DataContext _context;
        private readonly IPhotoService _photoService;
        private readonly IMapper _mapper;
        public ChatGroupController(DataContext context, IPhotoService photoService, IMapper mapper)
        {
            _context = context;
            _photoService = photoService;
            _mapper = mapper;
        }

        [HttpPost("create-chat-group")]
        public async Task<ActionResult> CreateChatGroup([FromForm] CreateChatGroupDto groupDto)
        {
            ///even though it's a photo it's 
            ///not a standar photo that we store the user id for it
            ///so instead we using app-file
            var photo = new AppFile();
            
            if (groupDto.GroupPicture != null && groupDto.GroupPicture.ContentType.Contains("image"))
            {
                var result = await _photoService.AddPhotoAsync(groupDto.GroupPicture);
                if (result.Error != null)
                {
                    return BadRequest(result.Error.Message);
                }
                photo = new AppFile
                {
                    FileUrl = result.Url.AbsoluteUri,
                    PublicId = result.PublicId,
                    FileName = groupDto.GroupPicture.FileName,
                    FileType = groupDto.GroupPicture.ContentType,
                    FileExtension = Path.GetExtension(groupDto.GroupPicture.FileName),
                    Length = groupDto.GroupPicture.Length,
                };
                _context.Files.Add(photo);
            }
            else
            {
                photo.Id = null;
            }



            var chatGroup = new ChatGroup
            {
                GroupCreatorId = User.GetUserId(),
                GroupDescription = groupDto.GroupDescription,
                GroupName = groupDto.GroupName,
                GroupPhotoId = photo.Id,
                GroupPhotoUrl = photo.FileUrl,
            };

            _context.ChatGroups.Add(chatGroup);


            /// and let's add the group creator to the participants

            var participantRoles = await _context.ParticipantRoles
                .ToListAsync();

            var user = await _context.Users
                .Include(x => x.Photos)
                .Where(x => x.Id == User.GetUserId())
                .FirstOrDefaultAsync();

            var userMainPhoto = user.Photos
                .FirstOrDefault(p => p.IsMain);


            var participant = new ChatGroupParticipant
            {
                ChatGroupId = chatGroup.Id,
                ParticipantId = chatGroup.GroupCreatorId,
                ParticipantRoleId = participantRoles
                .FirstOrDefault(x => x.RoleName == ParticipantRolesSrc.GroupCreator).Id,
                ParticipantUserName = User.GetUsername(),
                ParticipantKnownAs = User.GetKnownAs(),
                ParticipantPhotoUrl = userMainPhoto.Url,
            };


            _context.ChatGroupParticipants.Add(participant);

            ///add initial message that you have been create the group

            var message = new Message
            {
                ChatGroupId = chatGroup.Id,
                Content = $"{User.GetKnownAs()} create group {chatGroup.GroupName}.",
                GroupName = chatGroup.Id,
                IsGroupMessage = true,
                IsSystemMessage = true,
                SenderUsername = "System",
                RecipenetUsername = chatGroup.GroupName,
            };

            _context.Messages.Add(message);

            await _context.SaveChangesAsync();

            return Ok();
        }

        ///send invite to participants
        [HttpPost("invite-participants")]
        public async Task<ActionResult> InviteParticipants(InviteParticipantsDto inviteDto)
        {
            //check that this user is have the role to invite other participants
            var inviteBy = await _context.ChatGroupParticipants
                .Where(x => x.ChatGroupId == inviteDto.GroupId && x.ParticipantId == User.GetUserId())
                .FirstOrDefaultAsync();

            var participantsRoles = await _context.ParticipantRoles.ToListAsync();

            var groupCreatorRole = participantsRoles.Where(x => x.RoleName == ParticipantRolesSrc.GroupCreator)
                .FirstOrDefault().Id;
            var adminRole = participantsRoles.Where(x => x.RoleName == ParticipantRolesSrc.GroupAdmin)
                .FirstOrDefault().Id;

            if (inviteBy == null)
            {
                return NotFound("Group id not exist");
            }

            if (inviteBy.ParticipantRoleId != groupCreatorRole && inviteBy.ParticipantRoleId != adminRole)
            {
                return Forbid();
            }

            //check that the user to invite is exist and not on the group

            var toInvite = await _context.Users
                .Where(x => x.UserName == inviteDto.ToInviteUserName)
                .FirstOrDefaultAsync();

            if (toInvite == null)
            {
                return NotFound("User name not found");
            }

            bool isOnGroupAlready = await _context.ChatGroupParticipants.AnyAsync(x => x.ParticipantId == toInvite.Id && x.ChatGroupId == inviteDto.GroupId);
            if (isOnGroupAlready)
            {
                return BadRequest("User already on the group !!!");
            }

            //validation complete, send an invite to the participant

            var group = await _context.ChatGroups.FirstOrDefaultAsync(x => x.Id == inviteDto.GroupId);

            var invite = new ParticipantGroupInvite
            {
                ChatGroupId = inviteDto.GroupId,
                GroupName = group.GroupName,
                GroupDesc = group.GroupDescription,
                InvitedById = User.GetUserId(),
                InviteByKnownAs = User.GetKnownAs(),
                UserId = toInvite.Id
            };

            _context.GroupInvites.Add(invite);

            await _context.SaveChangesAsync();

            return Ok();
        }

        //take action with the invite
        [HttpPost("invite-response")]
        public async Task<ActionResult> TakeActionWithInvite(InviteResponseDto inviteResponse)
        {
            //check if the invite exist
            var invite = await _context.GroupInvites.FindAsync(inviteResponse.InviteId);
            if (invite == null) 
            {
                return NotFound("Invite not exist");
            }

            if (invite.UserId !=  User.GetUserId())
            {
                return Forbid();
            }


            //if response is true add it to the group and delete all invites to that group
            if (inviteResponse.Response)
            {
                var user = await _context.Users
                    .Include(u => u.Photos)
                    .Where(u => u.Id == User.GetUserId())
                    .FirstOrDefaultAsync();

                var participantsRoles = await _context.ParticipantRoles.ToListAsync();

                //add participant to the group
                var participant = new ChatGroupParticipant
                {
                    ChatGroupId = invite.ChatGroupId,
                    ParticipantId = invite.UserId,
                    ParticipantKnownAs = User.GetKnownAs(),
                    ParticipantPhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
                    ParticipantUserName = user.UserName,
                    ParticipantRoleId = participantsRoles.FirstOrDefault(r => r.RoleName == ParticipantRolesSrc.GroupParticipant).Id
                };
                _context.ChatGroupParticipants.Add(participant);

                //delete all invites to this group

                var allInvites = await _context.GroupInvites.Where(i => i.UserId == User.GetUserId() && i.ChatGroupId == invite.ChatGroupId)
                    .ToListAsync();

                _context.GroupInvites.RemoveRange(allInvites);

                ///add system message that the user joinned the group
                var message = new Message
                {
                    ChatGroupId = invite.ChatGroupId,
                    Content = $"{User.GetKnownAs()} joinned the group.",
                    IsGroupMessage = true,
                    IsSystemMessage = true,
                    SenderUsername = "System",
                    RecipenetUsername = invite.GroupName,
                    GroupName = invite.ChatGroupId,
                };

                _context.Messages.Add(message);

                await _context.SaveChangesAsync();
            }
            else
            {
                //delete the invite
                _context.GroupInvites.Remove(invite);
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        //get invite list
        [HttpGet("get-invite-list")]
        public async Task<ActionResult> GetInviteList()
        {
            var inviteList = await _context.GroupInvites
                .Where(x => x.UserId == User.GetUserId())
                .ProjectTo<InviteListItemDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(inviteList);
        }

        ///remove participants
        [HttpDelete("remove-participant")]
        public async Task<ActionResult> RemoveParticipant(int participantId, string groupId)
        {
            //should check that the caller are group creator or an admin to kick up the participant 
            var caller = await _context.ChatGroupParticipants
                .Where(x => x.ChatGroupId == groupId && x.ParticipantId == User.GetUserId())
                .Include(x => x.ParticipantRole)
                .FirstOrDefaultAsync();

            if (caller == null)
            {
                return BadRequest("You either not a participant or the group id is incorrect");
            }

            if (caller.ParticipantRole.RoleName != ParticipantRolesSrc.GroupCreator && caller.ParticipantRole.RoleName != ParticipantRolesSrc.GroupAdmin)
            {
                return Forbid();
            }

            var participant = await _context.ChatGroupParticipants
                .Where(p => p.ParticipantId == participantId && p.ChatGroupId == groupId)
                .Include(p => p.ParticipantRole)
                .FirstOrDefaultAsync();

            if (participant == null)
            {
                return NotFound("Participant id not found");
            }

            if (caller.ParticipantRole.RoleName == ParticipantRolesSrc.GroupAdmin && participant.ParticipantRole.RoleName == ParticipantRolesSrc.GroupAdmin)
            {
                return Forbid();
            }

            if (participant.ParticipantUserName == caller.ParticipantUserName)
            {
                return BadRequest("Exit instead");
            }

            _context.ChatGroupParticipants.Remove(participant);

            await _context.SaveChangesAsync();

            return Ok();
        }

        ///delete group
        [HttpDelete("delete-chat-group/{groupId}")]
        public async Task<ActionResult> DeleteChatGroup(string groupId)
        {
            //only admins
            
            var participant = await _context.ChatGroupParticipants.Where(x => x.ChatGroupId == groupId && x.ParticipantId == User.GetUserId())
                .Include(x => x.ParticipantRole)
                .FirstOrDefaultAsync();

            if (participant == null)
            {
                return Forbid();
            }

            if (participant.ParticipantRole.RoleName != ParticipantRolesSrc.GroupCreator && participant.ParticipantRole.RoleName != ParticipantRolesSrc.GroupAdmin)
            {
                return Forbid();
            }

            var group = await _context.ChatGroups.Where(g => g.Id == groupId)
                .Include(g => g.GroupParticipants)
                .Include(g => g.GroupInvites)
                .Include(g => g.GroupMessages)
                .FirstOrDefaultAsync();

            if (group == null)
            {
                return NotFound("Group not found");
            }


            ///delete users from group
            _context.ChatGroupParticipants.RemoveRange(group.GroupParticipants);

            //delete group invites
            _context.GroupInvites.RemoveRange(group.GroupInvites);

            ///delete the group messages
            _context.Messages.RemoveRange(group.GroupMessages);

            ///delete the group itself
            _context.ChatGroups.Remove(group);

            await _context.SaveChangesAsync();
            
            return Ok();
        }
        ///get group participants list
        [HttpGet("get-group-participants/{groupId}")]
        public async Task<ActionResult> GetGroupParticipants(string groupId)
        {
            ///check if the user on that group to see his participants

            var group = await _context.ChatGroups
                .Where(x => x.Id == groupId)
                .Include(g => g.GroupParticipants)
                .FirstOrDefaultAsync();

            if (!group.GroupParticipants.Any(gp => gp.ParticipantId == User.GetUserId()))
            {
                return Forbid();
            }

            var groupParticipants = await _context.ChatGroupParticipants
                .Where(g => g.ChatGroupId == groupId)
                .ProjectTo<GroupParticipantListItemDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return Ok(groupParticipants);
        }
        ///exit group
        [HttpGet("exit-from-group/{groupId}")]///method not async because, if last two admin participant want to quit the same group at the same time that makes an issue
        public ActionResult ExitFromGroup(string groupId)
        {
            var participant =  _context.ChatGroupParticipants
                .Where(x => x.ChatGroupId == groupId && x.ParticipantId == User.GetUserId())
                .Include(x => x.ParticipantRole)
                .FirstOrDefault();

            if (participant == null)
            {
                return BadRequest("Your not on the group to exit");
            }

            ///used in case the caller is the last participant in the group
            bool ShouldDeleteTheGroup = false;

            if (participant.ParticipantRole.RoleName != ParticipantRolesSrc.GroupParticipant)
            {
                //super participant means, group creator or group admin

                ///we should make sure that there is another group admin or group creator
                ///so the group not stay without higher role

                var superParticipantsCount = _context.ChatGroupParticipants
                    .Include(p => p.ParticipantRole)
                    .Where(p => p.ChatGroupId == groupId && p.ParticipantRole.RoleName != ParticipantRolesSrc.GroupParticipant)
                    .Count();

                if (superParticipantsCount == 1)
                {
                    ///need to promote the oldest participant
                    
                    var oldestParticipant = _context.ChatGroupParticipants
                        .Where(p => p.ChatGroupId == groupId && p.ParticipantId != User.GetUserId())
                        .OrderBy(p => p.JoinnedSince)
                        .FirstOrDefault();

                    if (oldestParticipant == null)
                    {
                        //there is no one in the group but the caller
                        ShouldDeleteTheGroup = true;
                    }
                    else
                    {
                        oldestParticipant.ParticipantRoleId = _context.ParticipantRoles
                            .Where(r => r.RoleName == ParticipantRolesSrc.GroupAdmin)
                            .FirstOrDefault()
                            .Id;
                    }



                }
            }

            _context.ChatGroupParticipants.Remove(participant);

            if (ShouldDeleteTheGroup)
            {
                var chatGroup = _context.ChatGroups.Find(groupId);

                _context.ChatGroups.Remove(chatGroup);
            }

            _context.SaveChanges();

            return Ok();
        }
        ///promote participant
        [HttpPost("promote-participant")]
        public async Task<ActionResult> PromoteParticipant(PromoteParticipantDto promoteDto)
        {
            ///check that the caller is an admin or group creator
            var caller = await _context.ChatGroupParticipants
                .Where(p => p.ChatGroupId == promoteDto.GroupId && p.ParticipantId == User.GetUserId())
                .Include(p => p.ParticipantRole)
                .FirstOrDefaultAsync();

            if (caller.ParticipantRole.RoleName == ParticipantRolesSrc.GroupParticipant)
            {
                return Forbid();
            }

            var toPromoteParticipant = await _context.ChatGroupParticipants
                .Where(p => p.ChatGroupId == promoteDto.GroupId && p.ParticipantId == promoteDto.ParticipantId)
                .Include(p => p.ParticipantRole)
                .FirstOrDefaultAsync();

            if (toPromoteParticipant == null)
            {
                return NotFound("Participant not on the group");
            }

            if (toPromoteParticipant.ParticipantRole.RoleName == ParticipantRolesSrc.GroupCreator)
            {
                return BadRequest("Can not promote any more");
            }

            var adminRole = await _context.ParticipantRoles.Where(r => r.RoleName == ParticipantRolesSrc.GroupAdmin)
                .FirstOrDefaultAsync();

            toPromoteParticipant.ParticipantRoleId = adminRole.Id;

            await _context.SaveChangesAsync();

            return Ok();
        }

        ///edit group information
        [HttpPatch("update-group-information")]
        public async Task<ActionResult> EditGroupInformation(UpdateChatGroupInformationDto groupInfo)
        {
            //check that the is an admin in the group

            var participant = await _context.ChatGroupParticipants
                .Where(p => p.ChatGroupId == groupInfo.GroupId && p.ParticipantId == User.GetUserId())
                .Include(p => p.ParticipantRole)
                .FirstOrDefaultAsync();

            if (participant == null || participant.ParticipantRole.RoleName == ParticipantRolesSrc.GroupParticipant)
            {
                return Forbid();
            }


            //edit the group

            var group = await _context.ChatGroups.FindAsync(groupInfo.GroupId);

            if (group == null)
            {
                return NotFound("Group not found");
            }

            group.GroupName = groupInfo.GroupName;
            group.GroupDescription = groupInfo.GroupDescription;

            await _context.SaveChangesAsync();

            return Ok();
        }
        ///update group photo
        [HttpPatch("update-group-photo/{groupId}")]
        public async Task<ActionResult> UpdateGroupPhoto(string groupId,[Required] IFormFile newPhoto)
        {
            //check file content
            if (!newPhoto.ContentType.Contains("image"))
            {
                return BadRequest("Expect image for the group photo");
            }

            //check participant role
            var participant = await _context.ChatGroupParticipants.Where(p => p.ChatGroupId == groupId && p.ParticipantId == User.GetUserId())
                .Include(p => p.ParticipantRole)
                .FirstOrDefaultAsync();

            if (participant == null || participant.ParticipantRole.RoleName == ParticipantRolesSrc.GroupParticipant)
            {
                return Forbid();
            }


            var group = await _context.ChatGroups.Where(g => g.Id == groupId)
                .Include(g => g.GroupPhoto)
                .FirstOrDefaultAsync();

            if (group == null)
            {
                return NotFound("Group not found");
            }

            ///if there is an image delete it
            if (group.GroupPhoto != null)
            {
                await _photoService.DeletePhotoAsync(group.GroupPhoto.PublicId);

                _context.Files.Remove(group.GroupPhoto);
            }


            var photo = new AppFile(newPhoto);

            var cloudinaryResult = await _photoService.AddPhotoAsync(newPhoto);

            photo.FileUrl = cloudinaryResult.Url.AbsoluteUri;
            photo.PublicId = cloudinaryResult.PublicId;

            group.GroupPhotoId = photo.Id;
            group.GroupPhotoUrl = photo.FileUrl;

            _context.Files.Add(photo);

            await _context.SaveChangesAsync();

            return Ok();
        }
    }
}
