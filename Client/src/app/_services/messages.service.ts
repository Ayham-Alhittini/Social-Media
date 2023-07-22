import { HttpClient } from '@angular/common/http';
import { EventEmitter, Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Message } from '../Models/message';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { User } from '../Models/user';
import { BehaviorSubject, take } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class MessagesService {

  constructor(private http: HttpClient) { }

  baseUrl = environment.apiBase;
  hubUrl = environment.hubBase;

  private hubConnection: HubConnection;
  private messageThreadSource = new BehaviorSubject<Message[]>([]);
  messageThread$ = this.messageThreadSource.asObservable();
  TotalUnreadCount = 0;
  
  lastMessageUpdate = new EventEmitter<Message>();
  onMessagesComponent = false;


  createHubConnection(user: User, otherUsername: string) {

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'message?user=' + otherUsername, {
        accessTokenFactory: () => user.token
      })
      .withAutomaticReconnect()
      .build();
    
    this.hubConnection.start().catch(error => console.log(error));

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
    return this.http.get<Message[]>(this.baseUrl + 'messages/list');
  }

  GetMessagesThread(username: string) {
    return this.http.get<Message[]>(this.baseUrl + 'messages/thread/' + username);
  }
  
  async sendMessage(username: string, content: string) {
    return this.hubConnection?.invoke('SendMessage', {'recipenetUsername': username,'content': content})
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
  getChatName(user: User, message: Message) {
    return user.username === message.recipenetUsername 
    ? message.senderUsername : message.recipenetUsername;
  }

  cutLargeMessage(content: string) {
    return content.length < 25 ? content : content.substring(0, 25) + '...';
  }
}
