import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'lists', pathMatch: 'full' },
  { path: 'login', loadComponent: () => import('./auth/login').then((m) => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./auth/register').then((m) => m.RegisterComponent) },
  { path: 'lists', loadComponent: () => import('./todo-list/todo-list').then((m) => m.TodoList) },
  { path: 'lists/:id', loadComponent: () => import('./todo-list-detail/todo-list-detail').then((m) => m.TodoListDetail) },
];
