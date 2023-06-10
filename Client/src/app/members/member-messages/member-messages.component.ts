import { AfterViewChecked, AfterViewInit, Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Message } from 'src/app/Models/message';
import { MessagesService } from 'src/app/_services/messages.service';

@Component({
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent implements OnInit, AfterViewChecked{
  
  @ViewChild('scrollMe') private myScrollContainer: ElementRef;

   

  scrollToBottom(): void {
      try {
          this.myScrollContainer.nativeElement.scrollTop = this.myScrollContainer.nativeElement.scrollHeight;
      } catch(err) { }                 
  }

  @Input() username: string;
  @Input() messages: Message[] = [];

  content = '';
  constructor(private messageService: MessagesService){}
  ngAfterViewChecked(): void {
    this.scrollToBottom();
  }
  ngOnInit(): void {
    this.scrollToBottom();
  }
  
  sendMessage(messaegForm: NgForm) {
    if (this.username && this.content)
    {
      this.messageService.sendMessage(this.username, this.content).subscribe({
        next: message => {
          this.messages.push(message);
          this.messageService.lastMessageUpdated.emit(message);
          messaegForm.reset();
        }
      });
    }
  }
  
}

