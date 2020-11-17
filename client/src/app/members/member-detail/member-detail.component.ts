import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Member } from 'src/app/_models/member';
import { MembersService } from 'src/app/_services/members.service';
import { NgxGalleryOptions } from '@kolkov/ngx-gallery';
import { NgxGalleryImage } from '@kolkov/ngx-gallery';
import { NgxGalleryAnimation } from '@kolkov/ngx-gallery';
import { TabDirective, TabsetComponent } from 'ngx-bootstrap/tabs';
import { Message } from 'src/app/_models/message';
import { MessageService } from 'src/app/_services/message.service';
import { PresenceService } from 'src/app/_services/presence.service';
import { AccountService } from 'src/app/_services/account.service';
import { User } from 'src/app/_models/user';
import { take } from 'rxjs/operators';

@Component({
    selector: 'app-member-detail',
    templateUrl: './member-detail.component.html',
    styleUrls: ['./member-detail.component.css'],
})
export class MemberDetailComponent implements OnInit, OnDestroy {
    member: Member;
    @ViewChild('memberTabs', { static: true }) memberTabs: TabsetComponent;
    galleryOptions: NgxGalleryOptions[];
    galleryImages: NgxGalleryImage[];
    activeTab: TabDirective;
    messages: Message[] = [];
    user: User;

    constructor(
        public presenceService: PresenceService,
        private router: ActivatedRoute,
        private messageService: MessageService,
        private accountService: AccountService,
        private routere: Router
    ) {
        this.accountService.currentUser$
            .pipe(take(1))
            .subscribe((user) => (this.user = user));
        this.routere.routeReuseStrategy.shouldReuseRoute = () => false;
    }
    ngOnDestroy(): void {
        this.messageService.stopHubConnection();
    }

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
            this.messageService.createHubConnection(
                this.user,
                this.member.username
            );
        } else {
            this.messageService.stopHubConnection();
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
