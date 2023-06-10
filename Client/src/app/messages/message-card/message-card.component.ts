import { Component, Input, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { take, tap } from 'rxjs';
import { Message } from 'src/app/Models/message';
import { AccountService } from 'src/app/_services/account.service';

@Component({
  selector: 'app-message-card',
  templateUrl: './message-card.component.html',
  styleUrls: ['./message-card.component.css']
})
export class MessageCardComponent{

  @Input() message: Message;
  @Input() me : string= null;
  
  getName() {
    if (this.message.senderUsername === this.me) {
      return this.message.recipenetUsername;
    } else {
      return this.message.senderUsername;
    }
  }

  determineSender() {
    if (this.message.senderUsername === this.me) {
      return this.message?.content ?  'You: ' : '';
    } else {
      return '';
    }
  }

  getImage() {
    
    if (this.message.senderUsername === this.me) {
      return this.message.recipenetPhotoUrl;
    } else {
      return this.message.senderPhotoUrl;
    }
  }
}
