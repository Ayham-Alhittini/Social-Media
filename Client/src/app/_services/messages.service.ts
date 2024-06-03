import { HttpClient } from '@angular/common/http';
import { EventEmitter, Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Message } from '../Models/message';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { User } from '../Models/user';
import { BehaviorSubject, take } from 'rxjs';
import { MessageListItem } from '../Models/message-list-item';
import { ChatGroupManagementService } from './chat-group-management.service';

@Injectable({
  providedIn: 'root'
})
export class MessagesService {

  constructor(private http: HttpClient, private chatGroupService: ChatGroupManagementService) { }

  baseUrl = environment.apiBase;
  hubUrl = environment.hubBase;

  private hubConnection: HubConnection;
  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  messageThread$ = this.messageThreadSource.asObservable();
  TotalUnreadCount = 0;
  
  lastMessageUpdate = new EventEmitter<Message>();
  RecipientLastConnection = new EventEmitter<Date>();//in case it's group chat or participant is online then it will be max | if user not have connection before then min | otherwise last connection
  onMessagesComponent = false;


  createHubConnection(user: User, groupName: string) {

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'message?groupName=' + groupName, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();
    
    this.hubConnection.start().catch(error => console.log(error));


    this.hubConnection.on('LastConnection', lastConnection => {
      this.RecipientLastConnection.emit(lastConnection);
    });

    this.hubConnection.on('ReciveMessageThread', messages => {
      this.messageThreadSource.next(messages);
    });

    this.hubConnection.on('NewMessage', newMessage => {
      this.messageThread$.pipe(take(1)).subscribe({
        next: messages => {
          this.messageThreadSource.next([...messages, newMessage]);
        }
      })
      this.lastMessageUpdate.emit(newMessage);
      if (newMessage.isGroupMessage && newMessage.isSystemMessage) {
        this.chatGroupService.groupInfoUpdated.emit();
      }
    });

    this.hubConnection.on('UnreadMessagesCount', unreadCount => {
      this.TotalUnreadCount -= unreadCount;
    });

  }

  stopConnection() {
    if (this.hubConnection)
    {
      this.hubConnection.stop();
    }
  }

  GetMessages() {
    return this.http.get<MessageListItem[]>(this.baseUrl + 'messages/list');
  }

  GetMessagesThread(username: string) {
    return this.http.get<Message[]>(this.baseUrl + 'messages/thread/' + username);
  }
  
  async sendMessage(groupName: string, content: string) {
    return this.hubConnection?.invoke('SendMessage', {'groupName': groupName,'content': content})
      .catch(error => console.log(error));
  }

  deleteMessage(id: number) {
    return this.http.delete(this.baseUrl + 'messages/' + id);
  }

  getUnreadCount() {
    return this.http.get(this.baseUrl + 'messages/get-unread-count');
  }

  getUnreadCountFormUser(senderName: string) {
    return this.http.get(this.baseUrl + 'Messages/get-unread-count/' + senderName);
  }


  //////helper method

  getGroupName(username: string, otherUsername: string) {///only for single chat
    return username < otherUsername ? username + '-' + otherUsername : otherUsername + '-' + username;
  }

  cutLargeMessage(content: string) {
    return content.length < 25 ? content : content.substring(0, 25) + '...';
  }
}
