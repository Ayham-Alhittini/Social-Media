﻿using System.ComponentModel.DataAnnotations.Schema;

namespace Dating_App_Backend.Entities
{
    public class Message
    {
        public int Id { get; set; }
        public int? SenderId { get; set; }
        public string SenderUsername { get; set; }
        public AppUser Sender { get; set; }
        [ForeignKey("Recipenet")]
        public int? RecipenetId { get; set; }///it would be null if it's a group message and user id otherwise
        public string RecipenetUsername { get; set; }
        public AppUser Recipenet { get; set; }

        public string ChatGroupId { get; set; }//in case IsGroupMessage is true then this one would be the same as group name, otherwise it would be null
        public ChatGroup ChatGroup { get; set; }

        public string Content { get; set; } 
        public DateTime MessageSent { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// GroupName is a special property
        /// in case this is a group message then it would be the id of that group
        /// else it would be as default (first name - second name) where first name is the one 
        /// that lexogicaly smaller than the second name, and those name represent the user name which is unique
        /// </summary>
        public string GroupName { get; set; }
        public bool IsGroupMessage { get; set; }
        public bool IsSystemMessage { get; set; } ///in case it's a group message some message generated by system for example when group created, or something edit or someone joinned

    }
}
