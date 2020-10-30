import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';
import { NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { NgxGalleryImage } from '@kolkov/ngx-gallery';
import { NgxGalleryAnimation } from '@kolkov/ngx-gallery';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css'],
})
export class MemberDetailComponent implements OnInit {
  member: Member;

  constructor(
    private memberService: MembersService,
    private router: ActivatedRoute
  ) {}

  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];

  ngOnInit(): void {
    this.loadMember();
    this.galleryOptions = [
      {
        width: '500px',
        height: '500px',
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false,
      },
    ];
  }

  getImages(): NgxGalleryImage[] {
    const imagesUrl = [];
    if (!this.member) {
      return [];
    }
    for (const photo of this.member?.photos) {
      imagesUrl.push({
        small: photo?.url,
        medium: photo?.url,
        big: photo?.url,
      });

      return imagesUrl;
    }
  }

  loadMember() {
    const name = this.router.snapshot.paramMap.get('username');

    this.memberService.getMember(name).subscribe((member) => {
      this.member = member;
      this.galleryImages = this.getImages();
    });
  }
}
