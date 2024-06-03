import { EventEmitter, Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class ChatGroupManagementService {

  private baseUrl = environment.apiBase + 'ChatGroup/';
  constructor(private http: HttpClient) { }


  openGroupInfoEmitter = new EventEmitter();
  closeGroupInfoEmitter = new EventEmitter();
  exitFromGroupEmitter = new EventEmitter();
  reciveGroupInviteEmitter = new EventEmitter();
  youKickedOutFromGroup = new EventEmitter();
  groupInfoUpdated = new EventEmitter();


  createGroupChat(data: any) {
    return this.http.post(this.baseUrl + 'create-chat-group', data);
  }

  getGroupInfo(groupId: string) {
    return this.http.get(this.baseUrl + 'get-group-info/' + groupId)
  }


  inviteToGroup(groupId: string, toInviteUserName: string) {
    return this.http.post(this.baseUrl + 'invite-participants', {groupId, toInviteUserName});
  }

  getInviteList() {
    return this.http.get(this.baseUrl + 'get-invite-list');
  }

  inviteResponse(inviteId: number, response: boolean) {
    return this.http.post(this.baseUrl + 'invite-response', {inviteId, response});
  }


  exitFromGroup(groupId: string) {
    return this.http.get(this.baseUrl + 'exit-from-group/' + groupId).pipe(tap( () => {
      this.exitFromGroupEmitter.emit();
    }))
  }

  promoteParticipant(participantId: string, groupId: string) {
    return this.http.post(this.baseUrl + 'promote-participant', {participantId, groupId});
  }

  demoteParticipant(participantId: string, groupId: string) {
    return this.http.post(this.baseUrl + 'demote-participant', {participantId, groupId});
  }

  removeParticipant(participantId: number, groupId: string) {
    return this.http.delete(this.baseUrl + 'remove-participant?participantId=' + participantId + '&groupId=' + groupId);
  }

}
