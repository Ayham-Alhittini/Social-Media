import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { NotFoundComponent } from './errors/not-found/not-found.component';
import { ServerErrorComponent } from './errors/server-error/server-error.component';
import { TestErrorComponent } from './errors/test-error/test-error.component';
import { HomeComponent } from './home/home.component';
import { ListsComponent } from './lists/lists.component';
import { MemberDetailComponent } from './members/member-detail/member-detail.component';
import { MemberEditComponent } from './members/member-edit/member-edit.component';
import { MemberListComponent } from './members/member-list/member-list.component';
import { AuthGuard } from './_guards/auth.guard';
import { PreventUnsavedChangesGuard } from './_guards/prevent-unsaved-changes.guard';
import { MemberDetailedResolver } from './_resolvers/member-detailed.resolver';
import { IsGuestGuard } from './_guards/is-guest.guard';
import { LoadUserResolver } from './_resolvers/load-user.resolver';
import { MessagesComponent } from './messages/messages.component';

const routes: Routes = [
  {path : '', component : HomeComponent, canActivate: [IsGuestGuard]},
  {path : 'home', component : HomeComponent, canActivate: [IsGuestGuard]},
  {path : '', 
    runGuardsAndResolvers : 'always',
    canActivate : [AuthGuard],
    children : [
      {path : 'members', component : MemberListComponent, canActivate : [AuthGuard]},
      {path : 'members/:username', component : MemberDetailComponent, resolve: {member: MemberDetailedResolver, user : LoadUserResolver}},
      {path : 'member/edit', component : MemberEditComponent, canDeactivate :[PreventUnsavedChangesGuard]},
      {path : 'lists', component : ListsComponent},
      {path : 'messages', component : MessagesComponent},
    ]
  },
  {path : 'error', component : TestErrorComponent},
  {path : 'not-found', component : NotFoundComponent},
  {path : 'server-error', component : ServerErrorComponent},
  {path : '**', component : HomeComponent},
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
