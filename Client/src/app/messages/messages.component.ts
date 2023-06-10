import { Component, OnDestroy, OnInit } from '@angular/core';
import { Message } from '../Models/message';
import { Pagination } from '../Models/pagination';
import { MessagesService } from '../_services/messages.service';
import { User } from '../Models/user';
import { AccountService } from '../_services/account.service';
import { first, take, tap } from 'rxjs';
import { ActivatedRoute, Router } from '@angular/router';
import { MemberService } from '../_services/member.service';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.css']
})
export class MessagesComponent implements OnInit{

  timerId: any;
  activeChat = 0;
  messages : Message[] = [];


  chatMessageThread: Message[] = [];
  pagination: Pagination;
  container = 'unread';
  pageNumber = 1;
  pageSize = 5;
  isLoading = true;

  user: User;
  constructor(private messageService: MessagesService,
    private accountService: AccountService,
    private route: ActivatedRoute,
    private memberService: MemberService,
    private router: Router) { }


  loadMessageList() {
    this.messageService.GetMessages().subscribe({
      next: res => {
        this.messages = res;
        this.loadFromParams();
      }
    });
  }
  ngOnInit(): void {
    
    this.loadMessageList();

    this.messageService.lastMessageUpdated.subscribe({
      next: res => {
        ///find which one from them are and replace it with the new value
        ///remove him and put him in the top of the page
        const otherRes = this.otherPerson(res);
        this.messages = this.messages.filter(msg => this.otherPerson(msg) != otherRes);
        this.messages.unshift(res);
        this.activeChat = 0;
      }
    });
    this.accountService.loadedUser.pipe(take(1), tap(res => {
      this.user = res;
    })).subscribe();

    this.isLoading = false;

  }

  loadFromParams() {
    this.route.queryParams.subscribe({
      next: res => {
        const friedName = res['user'];
        if (friedName) {
          ///if exist  on chat mark it as active and load it for him

          let found = false;
          for (let index = 0; index < this.messages.length; index++) {
            const element = this.messages[index];
            
            if (element.senderUsername === friedName || element.recipenetUsername === friedName) {
              this.activeChat = index;
              this.onChatLoad(this.activeChat);
              found = true;
              break;
            }
          }

          if (!found) {
            ///search if this user is exit on database (if not send error)
            this.memberService.getMember(friedName).subscribe({
              next: res => {
                ///add it and mark it active

                ///simulate message is exist

                let message: Message = {
                  id: -1,
                  senderId: '',
                  recipenetId: '',
                  dateRead: null,
                  messageSent: null,
                  senderUsername: this.user.username,
                  senderPhotoUrl: this.user.photoUrl,
                  recipenetUsername: friedName,
                  recipenetPhotoUrl: res.photoUrl,
                  unreadCount: 0,
                  content: ''
                }

                this.messages.push(message);
                ///now mark it as active without load it's messages since he have nothing
                this.activeChat = this.messages.length - 1;
                
              }
            });
          }

        }
        else {
          this.getChatThread();
        }
      }
    });
  }

  getChatThread() {
    if (this.messages.length > 0) {
      this.messageService.GetMessagesThread(this.otherPerson(this.messages[this.activeChat])).subscribe({
        next: res => {
          this.chatMessageThread = res;
          this.messages[this.activeChat] = this.chatMessageThread[this.chatMessageThread.length - 1];
        }
      })
    }
  }

  otherPerson(message: Message): string {
    return message.senderUsername != this.user.username ? message.senderUsername : message.recipenetUsername;
  }

  OtherPersonActive() {
    return this.messages[this.activeChat].senderUsername != this.user.username ? this.messages[this.activeChat].senderUsername : this.messages[this.activeChat].recipenetUsername;
  }

  onChatLoadRoute(msg: Message) {
    msg.unreadCount = 0;
    this.router.navigateByUrl('/messages?user=' + this.otherPerson(msg));
  }
  
  onChatLoad(chatIndex: number) {
    this.activeChat = chatIndex;
    this.messages[this.activeChat].unreadCount = 0;
    this.getChatThread();
  }
}

