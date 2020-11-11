import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { Message } from '../_models/message';
import { getPaginatedResult, getPaginationHeader } from './pagintaionHelper';

@Injectable({
    providedIn: 'root',
})
export class MessageService {
    baseUrl = environment.apiUrl;
    constructor(private http: HttpClient) {}

    getMessages(pageNumber: number, pageSize: number, container) {
        let params = getPaginationHeader(pageNumber, pageSize).append(
            'Container',
            container
        );
        return getPaginatedResult<Message[]>(
            this.baseUrl + 'messages',
            params,
            this.http
        );
    }

    GetMessageThread(username: string) {
        return this.http.get<Message[]>(
            this.baseUrl + 'messages/thread/' + username
        );
    }
    sendMessage(username: string, content: string) {
        return this.http.post<Message>(this.baseUrl + 'messages', {
            recipientUsername: username,
            content,
        });
    }
    deleteMessage(id: number) {
        return this.http.delete<Message>(this.baseUrl + 'messages/' + id);
    }
}
