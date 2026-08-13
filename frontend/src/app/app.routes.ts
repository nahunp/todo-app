import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'lists', pathMatch: 'full' },
  { path: 'lists', loadComponent: () => import('./todo-list/todo-list').then((m) => m.TodoList) },
];
