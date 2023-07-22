import { Component, OnDestroy, OnInit } from '@angular/core';
import { MessagesService } from '../_services/messages.service';
import { Message } from '../Models/message';
import { User } from '../Models/user';
import { AccountService } from '../_services/account.service';
import { take } from 'rxjs';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit, OnDestroy{
  list: Message[] = [];
  user: User;
  selectedChat = -1;
  loading = false;


  constructor(public messageService: MessagesService,
    private accountService:AccountService){
      accountService.loadedUser.pipe(take(1)).subscribe({
        next: res => this.user = res
      })
  }
  
  ngOnInit(): void {
    this.loading = true;
    this.messageService.onMessagesComponent = true;

    ////fetch the messages list
    this.messageService.GetMessages().subscribe({
      next: res => {
        this.list = res;
        this.loading = false;
      }
    });


    ///update last message on message list
    this.messageService.lastMessageUpdate.subscribe({
      next: (msg: Message) => {
        
        ///message i sent
        if (this.user.username === msg.senderUsername) 
        {
          const chatName = msg.recipenetUsername;
          
          const sendedChat = this.list.find(x => this.messageService.getChatName(this.user, x) === chatName);
  
          sendedChat.content = msg.content;
  
          this.list = this.list.filter(x => this.messageService.getChatName(this.user, x) !== chatName);
  
          this.list = [sendedChat, ...this.list];
          this.selectedChat = 0;
        }
        ///message i get
        else
        {
          
          const selectedChatName = this.selectedChat === -1 ? null : this.messageService.getChatName(this.user, this.list[this.selectedChat]);

          ///check if it's on the list
          const chatName = msg.senderUsername;
          const recivedChat = this.list.find(x => this.messageService.getChatName(this.user, x) === chatName);
          
          if (recivedChat)
          {
            ///check if it's the active chat
            if (this.selectedChat !== -1 && selectedChatName === chatName) 
            {
              recivedChat.unreadCount = 0; 
            }
            else 
            {
              recivedChat.unreadCount++;
            }
            
            this.list = this.list.filter(x => this.messageService.getChatName(this.user, x) !== chatName);
            recivedChat.content = msg.content;
            this.list = [recivedChat, ...this.list];

            ///find the new index of the selected chat
            for (let index = 0; index < this.list.length && this.selectedChat !== -1; index++) {
              const element = this.messageService.getChatName(this.user, this.list[index]);
              if (element === selectedChatName)
              { 
                this.selectedChat = index;
                break; 
              }
            }

          }
          else
          {
            ///could not be the active chat
            msg.unreadCount = 1;
            msg.listPhotoUrl = msg.senderPhotoUrl;
            this.list = [msg, ...this.list];
          }
        }

      }
    })
  }


  selectChat(idx: number) {
    this.selectedChat = idx;
    this.list[this.selectedChat].unreadCount = 0;
    this.messageService.stopConnection();
    this.messageService.createHubConnection(this.user, this.messageService.getChatName(this.user, this.list[this.selectedChat]));
  }


  ngOnDestroy(): void {
    this.messageService.onMessagesComponent = false;
    this.messageService.stopConnection()
  }
}
