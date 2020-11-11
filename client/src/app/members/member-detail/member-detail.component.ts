import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';
import { NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { NgxGalleryImage } from '@kolkov/ngx-gallery';
import { NgxGalleryAnimation } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';

@Component({
    selector: 'app-member-detail',
    templateUrl: './member-detail.component.html',
    styleUrls: ['./member-detail.component.css'],
})
export class MemberDetailComponent implements OnInit {
    member: Member;
    @ViewChild('memberTabs', { static: true }) memberTabs: TabsetComponent;
    galleryOptions: NgxGalleryOptions[];
    galleryImages: NgxGalleryImage[];
    activeTab: TabDirective;
    messages: Message[] = [];
    constructor(
        private memberService: MembersService,
        private router: ActivatedRoute,
        private messageService: MessageService
    ) {}

    ngOnInit(): void {
        this.router.data.subscribe((data) => {
            this.member = data.member;
        });
        this.router.queryParams.subscribe((x) => {
            x.tab ? this.selectTab(x.tab) : this.selectTab(0);
        });
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
        this.galleryImages = this.getImages();
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
        }
        return imagesUrl;
    }

    onTabActivated(data: TabDirective) {
        this.activeTab = data;
        if (
            this.activeTab.heading === 'Messages' &&
            this.messages.length === 0
        ) {
            this.loadMessages();
        }
    }
    loadMessages() {
        this.messageService
            .GetMessageThread(this.member.username)
            .subscribe((message) => {
                this.messages = message;
            });
    }
    selectTab(tabId) {
        this.memberTabs.tabs[tabId].active = true;
    }
}
