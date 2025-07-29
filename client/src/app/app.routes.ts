import { Routes } from '@angular/router';

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
  { path: '**', component: Home },
];
