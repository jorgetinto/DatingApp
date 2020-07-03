import {Routes} from '@angular/router';
import { HomeComponent } from './home/home.component';
import { MemberListComponent } from './members/member-list/member-list.component';
import { MessagesComponent } from './messages/messages.component';
import { ListComponent } from './list/list.component';
import { AuthGuard } from './_guards/auth.guard';
import { MemberDetailComponent } from './members/member-detail/member-detail.component';
import { MemberDetailResolver } from './_resolvers/member-detail.resolver';
import { MemberListResolver } from './_resolvers/member-list.resolver';
import { MemberEditComponent } from './members/member-edit/member-edit.component';
import { MemberEditResolver } from './_resolvers/member-edit.resolver';
import { PreventUnsavedChanges } from './_guards/prevent-unsaved-changes.guard';
import { ListsResolver } from './_resolvers/Lists.resolver';
import { MessagesResolver } from './_resolvers/messages.resolver';
import { AdminPanelComponent } from './admin/admin-panel/admin-panel.component';

export const appRoutes: Routes = [
    { path: 'home', component: HomeComponent},
    {
        path: '',
        runGuardsAndResolvers: 'always',
        canActivate: [AuthGuard],
        children: [
            { path: 'members', component: MemberListComponent, canActivate: [AuthGuard], resolve: {users: MemberListResolver}
                    , data: {roles: ['Member', 'Admin', 'Moderator']}},
            { path: 'members/:id', component: MemberDetailComponent, resolve: {user: MemberDetailResolver}
                    , data: {roles: ['Member', 'Admin', 'Moderator']}},
            { path: 'member/edit', component: MemberEditComponent,
                    resolve: {user: MemberEditResolver}, canDeactivate: [PreventUnsavedChanges]
                    , data: {roles: ['Member', 'Admin', 'Moderator']}},
            { path: 'messages', component: MessagesComponent, resolve: {messages: MessagesResolver}, data: {roles: ['Member', 'Admin', 'Moderator']}},
            { path: 'lists', component: ListComponent, resolve: {users: ListsResolver}, data: {roles: ['Member', 'Admin', 'Moderator']}},
            { path: 'admin', component: AdminPanelComponent, data: {roles: ['Admin', 'Moderator']}}
        ]
    },
    { path: '**', redirectTo: 'home', pathMatch: 'full'},
];
