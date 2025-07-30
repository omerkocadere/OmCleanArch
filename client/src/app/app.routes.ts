import { Routes } from '@angular/router';
import { authGuard } from '../core/guards/auth-guard';
import { Home } from '../features/home/home';
import { Lists } from '../features/lists/lists';
import { MemberDetailed } from '../features/members/member-detailed/member-detailed';
import { MemberList } from '../features/members/member-list/member-list';
import { Messages } from '../features/messages/messages';
import { TestErrors } from '../features/test-errors/test-errors';

export const routes: Routes = [
  { path: '', component: Home },
  {
    path: '',
    runGuardsAndResolvers: 'always',
    canActivate: [authGuard],
    children: [
      { path: 'members', component: MemberList },
      { path: 'members/:id', component: MemberDetailed },
      { path: 'lists', component: Lists },
      { path: 'messages', component: Messages },
    ],
  },
  { path: 'errors', component: TestErrors },
  { path: '**', component: Home },
];
