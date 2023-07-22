import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { Member } from 'src/app/Models/member';
import { MemberService } from 'src/app/_services/member.service';
import { PresenceService } from 'src/app/_services/presence.service';

@Component({
  selector: 'app-member-card',
  templateUrl: './member-card.component.html',
  styleUrls: ['./member-card.component.css']
})
export class MemberCardComponent implements OnInit {

  @Input() member : Member;
  constructor(private toastr: ToastrService, private memberService: MemberService, private router: Router,
    public presenceService: PresenceService) { }
    

  ngOnInit(): void {
  }

  addLike(heart) {
    
    if (heart.style.color == "red") {
      heart.style.color = "white";
    } else {
      heart.style.color = "red";
    }
    this.memberService.addLike(this.member.userName).subscribe({
      next: state => {
        if (state == "added") {
          this.toastr.success("You Liked " + this.member.userName)
        } else {
          this.toastr.success("You Unliked " + this.member.userName)
        }
      }
    });
  }
}
