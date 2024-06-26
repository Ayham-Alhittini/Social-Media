﻿using AutoMapper;
using Dating_App_Backend.DTOs;
using Dating_App_Backend.Entities;
using Dating_App_Backend.Extensions;
using System.Linq;

namespace Dating_App_Backend.Helper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AppUser, MemberDto>()
                .ForMember(dest => dest.PhotoUrl,
                           opt => opt.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url))
                .ForMember(dest => dest.Age,
                           opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));

            CreateMap<Photo, PhotoDto>();

            CreateMap<MemberUpdateDto, AppUser>();

            CreateMap<Message, MessageDto>()
                .ForMember(d => d.SenderPhotoUrl,
                           opt => opt.MapFrom(src => src.Sender.Photos.FirstOrDefault(p => p.IsMain).Url));

            CreateMap<RegisterDto, AppUser>();

            CreateMap<ParticipantGroupInvite, InviteListItemDto>();

            CreateMap<ChatGroupParticipant, GroupParticipantListItemDto>()
                .ForMember(d => d.ParticipantRoleName,
                           opt => opt.MapFrom(src => src.ParticipantRole.RoleName));

            CreateMap<ChatGroup, GroupDto>()
                .ForMember(d => d.GroupParticipants,
                           opt => opt.MapFrom(src => src.GroupParticipants));

            CreateMap<DateTime, DateTime>().ConvertUsing(d => DateTime.SpecifyKind(d, DateTimeKind.Utc));
            CreateMap<DateTime?, DateTime?>().ConvertUsing(d => d.HasValue ? DateTime.SpecifyKind(d.Value, DateTimeKind.Utc) : (DateTime?)null);
        }
    }
}
