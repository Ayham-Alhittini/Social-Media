import { Component, Input, OnInit } from '@angular/core';
import { ChatGroupManagementService } from '../_services/chat-group-management.service';
import { AccountService } from '../_services/account.service';
import { PresenceService } from '../_services/presence.service';
import { ToastrService } from 'ngx-toastr';
import { ChangeDetectorRef } from '@angular/core';
import { MessagesService } from '../_services/messages.service';

@Component({
  selector: 'app-group-info',
  templateUrl: './group-info.component.html',
  styleUrls: ['./group-info.component.css']
})
export class GroupInfoComponent implements OnInit {

  @Input() groupId: string;
  groupInfo;
  inviteUserName: string = '';
  currentUserName: string = '';
  currentParticpant;

  constructor(private chatGroupService: ChatGroupManagementService, private accountService: AccountService,
              private toaster: ToastrService, private presenceService: PresenceService,
              private messageService: MessagesService,
              private cdr: ChangeDetectorRef) {}

  ngOnInit(): void {
    this.accountService.loadedUser.subscribe(user => this.currentUserName = user.username);
    this.loadGroupInfo();
    this.chatGroupService.groupInfoUpdated.subscribe(() => this.loadGroupInfo());
    this.chatGroupService.openGroupInfoEmitter.subscribe(() => this.openNav());
    this.chatGroupService.closeGroupInfoEmitter.subscribe(() => this.closeNav());
  }

  loadGroupInfo() {
    this.chatGroupService.getGroupInfo(this.groupId)
      .subscribe(groupInfo => {
        this.groupInfo = groupInfo;
        this.groupInfo.groupParticipants.forEach(p => {
          p.particpantRoleWeight = this.particpantRoleWeight(p.participantRoleName);
        });
        this.currentParticpant = this.groupInfo.groupParticipants.find(p => p.participantUserName === this.currentUserName);
        this.cdr.detectChanges();
      });
  }

  openNav() {
    const groupInfo = document.getElementById("groupInfo") as HTMLElement;
    groupInfo.style.width = "450px";
    document.getElementById("chat-messages").style.marginRight = "450px";
    const sidenav = document.querySelector("div#groupInfo.sidenav") as HTMLElement;
    sidenav.style.display = "block";
  }

  closeNav() {
    document.getElementById("groupInfo").style.width = "0";
    document.getElementById("chat-messages").style.marginRight = "0";
  }

  inviteMember() {
    this.chatGroupService.inviteToGroup(this.groupId, this.inviteUserName).subscribe(() => {
      this.presenceService.sendGroupInvite(this.inviteUserName);
      this.toaster.success("Invite sended to " + this.inviteUserName);
      this.inviteUserName = '';
      this.cdr.detectChanges();
    });
  }

  exitGroup() {
    this.chatGroupService.exitFromGroup(this.groupId).subscribe();
  }

  isAdmin() {
    return this.currentParticpant.participantRoleName === 'Group Admin' || this.currentParticpant.participantRoleName === 'Group Creator';
  }

  promoteParticipant(participant) {
    this.chatGroupService.promoteParticipant(participant.participantId, this.groupId).subscribe();
  }

  demoteParticipant(participant) {
    this.chatGroupService.demoteParticipant(participant.participantId, this.groupId).subscribe();
  }

  removeParticipant(participant) {
    this.chatGroupService.removeParticipant(participant.participantId, this.groupId).subscribe(() => {
      this.groupInfo.groupParticipants = this.groupInfo.groupParticipants
        .filter(p => p.participantId !== participant.participantId);
      this.cdr.detectChanges();
    })
  }

  particpantRoleWeight(roleName) {
    switch(roleName) {
      case "Group Participant": return 0;
      case "Group Admin": return 1;
      case "Group Creator": return 2;
    }
  }
}
