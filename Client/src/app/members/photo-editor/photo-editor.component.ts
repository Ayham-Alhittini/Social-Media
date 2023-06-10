import { Component, Input, OnInit } from '@angular/core';
import { FileUploader } from 'ng2-file-upload';
import { take } from 'rxjs';
import { Member } from 'src/app/Models/member';
import { Photo } from 'src/app/Models/photo';
import { User } from 'src/app/Models/user';
import { AccountService } from 'src/app/_services/account.service';
import { MemberService } from 'src/app/_services/member.service';
import { environment } from 'src/environments/environment';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})
export class PhotoEditorComponent implements OnInit {
  @Input() member : Member;
  uploader: FileUploader;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiBase;
  user : User;

  constructor(private accountService : AccountService, private memberService: MemberService){
    accountService.loadedUser.pipe(take(1)).subscribe({
      next : res => {
        this.user = res;
      }
    });
  }
  ngOnInit(): void {
    this.initializeUploader();
  }

  fileOverBase(e : any) {
    this.hasBaseDropZoneOver = e;
  }

  setMainPhoto(photo: Photo) {
    for(const ph of this.member.photos) {
      ph.isMain = false;
    }
    this.memberService.setMainPhoto(photo.id).subscribe({
      next : () => {
        photo.isMain = true;
        this.member.photoUrl = photo.url;
        this.user.photoUrl = photo.url;
        this.accountService.setCurrentUser(this.user);
      }
    });
    
  }

  removePhoto(photo : Photo) {
    this.memberService.removePhoto(photo.id).subscribe({
      next : () => {
        const toDeletePhoto = this.member.photos.findIndex(x => x.id == photo.id);
        this.member.photos.splice(toDeletePhoto, 1);
        this.member.photos.slice()
        if (photo.isMain) {
          this.member.photoUrl = null;
          this.user.photoUrl = null;
          this.accountService.setCurrentUser(this.user);
        }
      }
    });
    
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/add-photo',
      authToken: 'Bearer ' + this.user?.token,
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true,
      autoUpload: false,
      maxFileSize : 10 * 1024 * 1024
    });

    this.uploader.onAfterAddingFile = (file) => {
      file.withCredentials = false
    };

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if (response) {
        const photo = JSON.parse(response);
        if (this.member.photos.length == 0)
        {
          this.member.photoUrl = photo.url;
          this.user.photoUrl = photo.url;
          this.accountService.setCurrentUser(this.user);
        }
        this.member?.photos.push(photo);
      }
    };

  }

  oninputChnage(files: FileList) {
    console.log(files);
    
  }

  getImgSrc(file) {
    const url = URL.createObjectURL(file);

    return url;
  }

  imageSrc;

  readUrl(event) {
    if(event.target.files && event.target.files[0]) {
      const file = event.target.files[0];

      const reader = new FileReader();
      reader.onload = e => this.imageSrc = reader.result;

      reader.readAsDataURL(file);
    }
  }
}
