import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { ToastrService } from 'ngx-toastr';
import { environment } from 'src/environments/environment';
import { User } from '../Models/user';
import { BehaviorSubject, take } from 'rxjs';
import { Router } from '@angular/router';
import { MessagesService } from './messages.service';

@Injectable({
  providedIn: 'root'
})
export class PresenceService {

  hubUrl = environment.hubBase;

  private hubConnection: HubConnection;
  private onlineUsersSource = new BehaviorSubject<string[]>([]);
  onlineUsers$ = this.onlineUsersSource.asObservable();
  constructor(private toaster: ToastrService, private router: Router, private messagesService: MessagesService) { }

  createHubConnection(user : User) {

    this.messagesService.getUnreadCount().subscribe({
      next: result => this.messagesService.TotalUnreadCount = result['result']
    });

    this.hubConnection = new HubConnectionBuilder()
      .withUrl(this.hubUrl + 'presence', {
        accessTokenFactory: () => user.token 
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.start().catch(error => console.log(error));

    this.hubConnection.on('UserIsOnline', username => {

      this.onlineUsers$.pipe(take(1)).subscribe({
        next: onlineUsers => {
          this.onlineUsersSource.next([...onlineUsers, username]);
        }
      })
      
    });

    this.hubConnection.on('UserIsOffline', username => {
      this.onlineUsers$.pipe(take(1)).subscribe({
        next: onlineUsers => {
          this.onlineUsersSource.next(onlineUsers.filter(x => x !== username));
        }
      })
    });


    this.hubConnection.on('GetOnlineUsers', res => {
      this.onlineUsersSource.next(res);
    })


    this.hubConnection.on('NewMessageRecived', ({username, knownAs, message}) => {
      
      /////if on message component show the message on the list
      if (this.messagesService.onMessagesComponent) 
      { 
        this.messagesService.lastMessageUpdate.emit(message);
      } 
      else 
      {
        this.toaster.info(knownAs + ' has sent you a message! Click to see it')
        .onTap
        .pipe(take(1))
        .subscribe({
          next: () => {
            this.router.navigateByUrl('/members/' + username + '?tab=Messages');
          }
        });
      }

      this.messagesService.TotalUnreadCount++;
    });

  }

  stopHubConnection() {
    this.hubConnection.stop().catch(error => console.log(error));
  }
}
