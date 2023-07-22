import { Component, Input, OnInit } from '@angular/core';
import { Message } from 'src/app/Models/message';
import { User } from 'src/app/Models/user';
import { MessagesService } from 'src/app/_services/messages.service';

@Component({
  selector: 'app-message-card',
  templateUrl: './message-card.component.html',
  styleUrls: ['./message-card.component.css']
})
export class MessageCardComponent{
  @Input() user: User;
  @Input()message: Message;

  constructor(public messageService: MessagesService){}
  
}
