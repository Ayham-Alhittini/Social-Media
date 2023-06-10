import { Component, OnInit } from '@angular/core';
import { Member } from '../Models/member';
import { Pagination } from '../Models/pagination';
import { MemberService } from '../_services/member.service';

@Component({
  selector: 'app-lists',
  templateUrl: './lists.component.html',
  styleUrls: ['./lists.component.css']
})
export class ListsComponent implements OnInit {

  members: Member[];
  predicate = 'liked';
  pagination: Pagination;
  pageNumber = 1;
  pageSize = 5;


  constructor(private memberService: MemberService) { }

  ngOnInit(): void {
    this.loadMember();
  }

  loadMember() {
    this.memberService.getLikes(this.predicate, this.pageNumber, this.pageSize).subscribe({
      next: response => {
        this.members = response.result;
        this.pagination = response.pagination;
      }
    });
  }
  pageChanged(event: any) {
    if (this.pageNumber !== event.page) {
      this.pageNumber = event.page;
      this.loadMember();
    }
  }
}
