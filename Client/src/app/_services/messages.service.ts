import { HttpClient } from '@angular/common/http';
import { EventEmitter, Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Message } from '../Models/message';

@Injectable({
  providedIn: 'root'
})
export class MessagesService {

  constructor(private http: HttpClient) { }

  baseUrl = environment.apiBase;
  lastMessageUpdated = new EventEmitter<Message>();
  GetMessages() {
    return this.http.get<Message[]>(this.baseUrl + 'messages/list');
  }

  GetMessagesThread(username: string) {
    return this.http.get<Message[]>(this.baseUrl + 'messages/thread/' + username);
  }
  
  sendMessage(username: string, content: string) {
    return this.http.post<Message>(this.baseUrl + 'messages', 
    {
      'recipenetUsername': username,
      'content': content
    });
  }

  deleteMessage(id: number) {
    return this.http.delete(this.baseUrl + 'messages/' + id);
  }
}1
