import { Injectable } from '@angular/core';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { UserService } from '../_services/User.service';
import { AlertifyServiceService } from '../_services/AlertifyService.service';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { Message } from '../_models/Message';
import { AuthService } from '../_services/auth.service';

@Injectable()
export class MessagesResolver implements Resolve<Message[]> {
    pageNumber = 1;
    pageSize = 5;
    messageContainer = 'Unread';

    constructor(private userServices: UserService,
                private authService: AuthService,
                private router: Router,
                private alertify: AlertifyServiceService) {}

    resolve(router: ActivatedRouteSnapshot): Observable<Message[]> {
        return this.userServices.getMessages(
                    this.authService.decodedToken.nameid,
                    this.pageNumber,
                    this.pageSize,
                    this.messageContainer).pipe(
            catchError(error => {
                this.alertify.error('Problem retrieving messages');
                this.router.navigate(['/home']);
                return of(null);
            })
        );
    }
}
