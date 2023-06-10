import { Component, HostListener, OnInit, ViewChild } from '@angular/core';
import { Member } from 'src/app/Models/member';
import { MemberService } from 'src/app/_services/member.service';
import { AccountService } from 'src/app/_services/account.service';
import { take } from 'rxjs';
import { NgForm } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {

  @ViewChild('editForm')editForm : NgForm;
  @HostListener('window:beforeunload', ['$event']) unloadNotification($event : any) {
    if (this.editForm?.dirty) {
      $event.returnValue = true;
    }
  }
  member : Member;
  constructor(private memberService : MemberService, private accountService : AccountService,
    private toastr: ToastrService) { }
  ngOnInit(): void {
    this.accountService.loadedUser.pipe(take(1)).subscribe({
      next : user => {
        this.memberService.getMember(user.username).subscribe({
          next : res => {
            this.member = res;
          },
          error : res => {
            console.log(res);
          }
        });
      }
    });
  }
  updateMember() {
    this.memberService.updateMember(this.editForm.value).subscribe({
      next : () => {
        this.toastr.success("Profile updated successfuly");
        this.editForm.reset(this.member);
      },
      error : error => {
        this.toastr.error(error.error);
      }
    });
  }

}
