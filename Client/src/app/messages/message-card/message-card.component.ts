import { Component, Input, OnInit } from '@angular/core';
import { MessageListItem } from 'src/app/Models/message-list-item';
import { MessagesService } from 'src/app/_services/messages.service';

@Component({
  selector: 'app-message-card',
  templateUrl: './message-card.component.html',
  styleUrls: ['./message-card.component.css']
})
export class MessageCardComponent{
  @Input()message: MessageListItem;
  constructor(public messageService: MessagesService){}
}
