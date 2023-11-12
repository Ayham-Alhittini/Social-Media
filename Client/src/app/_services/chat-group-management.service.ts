import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { MessagesService } from './messages.service';
import { HttpClient } from '@angular/common/http';

@Injectable({
  providedIn: 'root'
})
export class ChatGroupManagementService {

  baseUrl = environment.apiBase + 'ChatGroup/';
  hubUrl = environment.hubBase;
  constructor(private http: HttpClient, private messageService: MessagesService) { }

  createGroupChat(data: any) {
    return this.http.post(this.baseUrl + 'create-chat-group', data);
  }

}
