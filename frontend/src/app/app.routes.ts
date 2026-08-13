import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'lists', pathMatch: 'full' },
  { path: 'lists', loadComponent: () => import('./todo-list/todo-list').then((m) => m.TodoList) },
  { path: 'lists/:id', loadComponent: () => import('./todo-list-detail/todo-list-detail').then((m) => m.TodoListDetail) },
];
