import { Component, OnInit } from '@angular/core';
import { User } from '../../_models/User';
import { AlertifyServiceService } from '../../_services/AlertifyService.service';
import { UserService } from '../../_services/User.service';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  users: User[];

  constructor(private userService: UserService,
              private alertify: AlertifyServiceService,
              private route: ActivatedRoute) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.users = data[`users`];
    });
  }
}
