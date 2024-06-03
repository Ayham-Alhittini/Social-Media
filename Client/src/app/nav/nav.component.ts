import { Component, OnDestroy, OnInit } from '@angular/core';
import { NgForm } from '@angular/forms';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Subscription } from 'rxjs';
import { AccountService } from '../_services/account.service';
import { MessagesService } from '../_services/messages.service';
import { ChatGroupManagementService } from '../_services/chat-group-management.service';

@Component({
  selector: 'app-nav',
  templateUrl: './nav.component.html',
  styleUrls: ['./nav.component.css']
})
export class NavComponent implements OnInit, OnDestroy {

  groupInvites :any = [];

  constructor(
    public accountService: AccountService, 
    private router: Router, 
    private toastr: ToastrService,
    public messageService: MessagesService,
    private chatGroupService: ChatGroupManagementService
  ) { }

  userSub: Subscription;
  toSearchUsername: string = '';

  ngOnInit(): void {
    this.userSub = this.accountService.loadedUser.subscribe({
      next: user => {
        if (user)
           this.loadGroupInvites();
      },
      error: res => {
        console.log(res);
      }
    });

    this.chatGroupService.reciveGroupInviteEmitter.subscribe(() => this.loadGroupInvites());
  }

  loadGroupInvites() {
    this.chatGroupService.getInviteList().subscribe(inviteList => this.groupInvites = inviteList);
  }

  toggleDropdown(event: Event) {
    event.preventDefault();
  }

  acceptInvite(_invite) {
    this.chatGroupService.inviteResponse(_invite.id, true).subscribe(() => this.toastr.success("Joinned successfully"));
    this.groupInvites = this.groupInvites.filter(invite => invite.id !== _invite.id);
  }

  rejectInvite(_invite) {
    this.chatGroupService.inviteResponse(_invite.id, false).subscribe();
    this.groupInvites = this.groupInvites.filter(invite => invite.id !== _invite.id);
  }


  searchUser() {
    this.toSearchUsername = this.toSearchUsername.trim();
    if (this.toSearchUsername === '') return;
    this.router.navigateByUrl('members/' + this.toSearchUsername);
    this.toSearchUsername = '';
  }

  login(authForm: NgForm) {
    this.accountService.login(authForm.value).subscribe({
      next: () => this.router.navigateByUrl("/members"),
      error: errorMsg => {
        console.log(errorMsg);
        for (const element of errorMsg) {
          this.toastr.error(element);
        }
      }
    });
  }

  logout() {
    this.accountService.logout();
    this.router.navigateByUrl("/");
  }

  ngOnDestroy(): void {
    this.userSub.unsubscribe();
  }
}
