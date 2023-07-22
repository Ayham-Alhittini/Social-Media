import { ChangeDetectionStrategy, Component, ElementRef, Input, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { take } from 'rxjs';
import { User } from 'src/app/Models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MessagesService } from 'src/app/_services/messages.service';

@Component({
  changeDetection: ChangeDetectionStrategy.OnPush,
  selector: 'app-member-messages',
  templateUrl: './member-messages.component.html',
  styleUrls: ['./member-messages.component.css']
})
export class MemberMessagesComponent{
  @ViewChild('messageForm') messageForm: NgForm;
  @Input() username: string;
  
  user: User;
  messageContent: string;
  loading = false;

  constructor(public messageService: MessagesService, private accountService: AccountService) { 
    accountService.loadedUser.pipe(take(1)).subscribe({
      next: res => this.user = res
    })
  }


  @ViewChild('scrollMe') private myScrollContainer: ElementRef;
  disableScrollDown = false;



  ngAfterViewChecked() {
      this.scrollToBottom();
  }


  onScroll() {
    let element = this.myScrollContainer.nativeElement;
    let atBottom = element.scrollHeight - element.scrollTop === element.clientHeight;
    if (!atBottom) {
        this.disableScrollDown = true
    } else {
        this.disableScrollDown = false
    }
}


  private scrollToBottom(): void {
    if (this.disableScrollDown) {
        return
    }
    try {
        this.myScrollContainer.nativeElement.scrollTop = this.myScrollContainer.nativeElement.scrollHeight;
    } catch(err) { }
  }




  sendMessage() {
    this.loading = true;
    this.messageService.sendMessage(this.username, this.messageContent).then(() => {
      this.messageForm.reset();
    }).finally(() => this.loading = false);
  }

}