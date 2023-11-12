import { ChangeDetectionStrategy, Component, ElementRef, Input, OnInit, ViewChild } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
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
export class MemberMessagesComponent implements OnInit{
  @ViewChild('messageForm') messageForm: NgForm;
  @Input() username: string;
  @Input() groupName: string;
  @Input() chatPhoto: string = null;
  @Input() doesHaveHeader: boolean = false;///when get the message from member details no need to put the header , otherwise put it
  @Input() isGroupMessage: boolean = false;//used to to set where user go when click on header if any

  recipientLastConnection: Date = null;

  user: User;
  messageContent: string;
  loading = false;

  constructor(public messageService: MessagesService,
    private router: Router,
    private accountService: AccountService) { 
    accountService.loadedUser.pipe(take(1)).subscribe({
      next: res => this.user = res
    })
  }

  ngOnInit(): void {
    this.GetRecipientLastConnection();
  }
  
  GetRecipientLastConnection() {
    this.recipientLastConnection = null;
    this.messageService.RecipientLastConnection.subscribe(lastConnection => {
      this.recipientLastConnection = lastConnection;
    });
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

  onHeaderClick() {
    if (!this.isGroupMessage) {
      this.router.navigateByUrl('/members/' + this.username);
    } else {
      this.openNav();
    }
  }


  openNav() {
    document.getElementById("groupInfo").style.width = "450px";
    document.getElementById("chat-messages").style.marginRight = "450px";
  }
  
  closeNav() {
    document.getElementById("groupInfo").style.width = "0";
    document.getElementById("chat-messages").style.marginRight = "0";
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
    this.messageService.sendMessage(this.groupName, this.messageContent).then(() => {
      this.messageForm.reset();
    }).finally(() => this.loading = false);
  }

}