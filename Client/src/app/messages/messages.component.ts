import { Component, ElementRef, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { MessagesService } from '../_services/messages.service';
import { Message } from '../Models/message';
import { User } from '../Models/user';
import { AccountService } from '../_services/account.service';
import { take } from 'rxjs';
import { MessageListItem } from '../Models/message-list-item';
import { ToastrService } from 'ngx-toastr';
import { ChatGroupManagementService } from '../_services/chat-group-management.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit, OnDestroy{

  @ViewChild('closeCreateGroupChatBtn') closeBtn: ElementRef;

  list: MessageListItem[] = [];
  user: User;
  selectedChat = -1;
  loading = false;

  fileCleaner = null;

  newGroup = {
    groupName: '',
    groupDescription: '',
    groupPicture: null
  };

  invalidFile = false;
  submitted = false;

  constructor(public messageService: MessagesService,
    private chatGroupService: ChatGroupManagementService,
    private toaster: ToastrService,
    accountService:AccountService){
      accountService.loadedUser.pipe(take(1)).subscribe({
        next: res => this.user = res
      })
  }
  
  ngOnInit(): void {
    
    this.messageService.onMessagesComponent = true;
    ////fetch the messages list
    this.loadChats();
    this.syncMessages();
    this.chatGroupService.exitFromGroupEmitter.subscribe(() => {
      this.loadChats();
      this.messageService.stopConnection();
      this.selectedChat = -1;
    });
  }

  loadChats() {
    this.loading = true;
    this.messageService.GetMessages().subscribe({
      next: res => {
        this.list = res;
        this.loading = false;
      },
      error: err => {
        this.loading = false;
        console.log(err);
      }
    });
  }

  onSelectedGroupPictureChange(event: any) {
    const selectedFile = event.target.files[0];
    
    if (selectedFile.type.indexOf('image') === -1) 
    {
      this.invalidFile = true;
      this.newGroup.groupPicture = null;
      this.fileCleaner = null;
      return;
    }
    else
    {
      this.invalidFile = false;
    }

    this.newGroup.groupPicture = selectedFile;
  }

  syncMessages() {
    // /update last message on message list
    this.messageService.lastMessageUpdate.subscribe({
      next: (msg: Message) => {
        
        ///message i sent
        if (this.user.username === msg.senderUsername) 
        {
          
          const sendedChat = this.list.find(x => x.groupName === msg.groupName);
          
          sendedChat.content = msg.content;

          this.list = this.list.filter(x => x.groupName !== sendedChat.groupName);
  
          this.list = [sendedChat, ...this.list];
          this.selectedChat = 0;
        }
        ///message i get
        else
        {
          ///check if it's on the list
          const recivedChat = this.list.find(x => x.groupName === msg.groupName);
          
          if (recivedChat)//on the list
          {
            ///check if it's the active chat
            if (this.selectedChat !== -1 && this.list[this.selectedChat].groupName === msg.groupName) 
            {
              recivedChat.unreadCount = 0; 
            }
            else 
            {
              recivedChat.unreadCount++;
            }
            
            this.list = this.list.filter(x => x.groupName !== msg.groupName);
            recivedChat.content = msg.content;
            this.list = [recivedChat, ...this.list];

            ///find the new index of the selected chat
            for (let index = 0; index < this.list.length && this.selectedChat !== -1; index++) {
              if (this.list[index].groupName === this.list[this.selectedChat].groupName)
              { 
                this.selectedChat = index;
                break; 
              }
            }

          }
          else
          {
            ///could not be the active chat
            var newMessageList : MessageListItem = {
              chatName : msg.senderUsername,
              chatPhoto: msg.senderPhotoUrl,
              content: msg.content,
              groupName: this.messageService.getGroupName(this.user.username, msg.senderUsername),
              isGroupMessage: false,
              isSystemMessage: false,
              messageSent: null,
              senderUsername: msg.senderUsername,
              unreadCount: 1
            };
            //add it to the list
            this.list = [newMessageList, ...this.list];
          }
        }

      }
    })
  }


  selectChat(idx: number) {
    this.selectedChat = idx;

    this.list[this.selectedChat].unreadCount = 0;

    this.chatGroupService.closeGroupInfoEmitter.emit();

    this.messageService.stopConnection();///to stop the previous connection if any

    this.messageService.createHubConnection(this.user,this.list[idx].groupName);
  }

  resetCreateChatGroupModal() {
    this.fileCleaner = null;
    this.invalidFile = false;
    this.submitted = false;
    
    this.newGroup = {
      groupName: '',
      groupDescription: '',
      groupPicture: null
    };
  }

  createChatGroup() {
    this.submitted = true;

    if (this.newGroup.groupName.trim() === '' || this.newGroup.groupDescription.trim() === '' || this.invalidFile)
    {
      this.toaster.error('fill the form correctly');
      return;
    }
    
    const formData = new FormData();
    formData.append('GroupName', this.newGroup.groupName);
    formData.append('GroupDescription', this.newGroup.groupDescription);
    formData.append('GroupPicture', this.newGroup.groupPicture);
    
    
    this.chatGroupService.createGroupChat(formData).subscribe(response => {
      this.toaster.success('Created successfuly');
      this.messageService.TotalUnreadCount += 1;
      this.loadChats();
      this.resetCreateChatGroupModal();
      this.closeBtn.nativeElement.click();
    });
  }

  ngOnDestroy(): void {
    this.messageService.onMessagesComponent = false;
    this.messageService.stopConnection();
    this.chatGroupService.closeGroupInfoEmitter.emit();
  }
}
