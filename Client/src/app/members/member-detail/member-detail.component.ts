import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { NgxGalleryAnimation, NgxGalleryImage, NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { ToastrService } from 'ngx-toastr';
import { take } from 'rxjs';
import { Member } from 'src/app/Models/member';
import { Message } from 'src/app/Models/message';
import { User } from 'src/app/Models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MemberService } from 'src/app/_services/member.service';
import { MessagesService } from 'src/app/_services/messages.service';
import { PresenceService } from 'src/app/_services/presence.service';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit, OnDestroy{

  @ViewChild('memberTabs', {static: true})memberTabs: TabsetComponent;
  state = ''
  member : Member;
  activeTab: TabDirective;
  user: User;

  constructor(private route : ActivatedRoute, private memberService : MemberService,
    private toastr: ToastrService, private messagesService: MessagesService, private router: Router,
    public presenceService: PresenceService, private accountService: AccountService) {
      this.router.routeReuseStrategy.shouldReuseRoute = () => false;
    }

  ngOnDestroy(): void {
    this.messagesService.stopConnection();
  }
 

  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];

  getImages() {
    if (!this.member) {
      return [];
    }
    const imagesUrls = [];
    for (const photo of this.member.photos) {
      imagesUrls.push({
        small : photo.url,
        medium : photo.url,
        big : photo.url
      });
    }
    return imagesUrls;
  }
  ngOnInit(): void {
    this.route.data.subscribe({
      next: data => {
        this.member = data['member'];
        this.user = data['user']; 
      }
    });
    this.route.queryParams.subscribe({
      next: params => {
        if (params['tab']) {
          this.selectTab(params['tab']);
        }
      }
    });

    this.state = this.member.isLiked ? "Unlike" : "Like";
    this.galleryImages = this.getImages();
    this.galleryOptions = [
      {
        width: '500px',
        height: '500px',
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false
      }
    ];
  }
  addLike() {
    
    this.memberService.addLike(this.member.userName).subscribe({
      next: state => {
        if (state == "added") {
          this.toastr.success("You Liked " + this.member.userName)

        } else {
          this.toastr.success("You Unliked " + this.member.userName)
        }
        if (this.state == "Unlike") {
          this.state = "Like";
        } else {
          this.state = "Unlike";
        }
      }
    });
  }

  onTabActivaited(data: TabDirective) {
    this.activeTab = data;


    if (this.activeTab.heading === 'Messages' && this.user) {
        this.messagesService.createHubConnection(this.user, this.member.userName);
    } else {
      this.messagesService.stopConnection();
    }
    
  }

  selectTab(heading: string) {
    if (this.memberTabs) {
      this.memberTabs.tabs.find(x => x.heading === heading).active = true;
    }
  }
}