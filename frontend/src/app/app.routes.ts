import { Routes } from '@angular/router';
import { authGuard } from './services/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: 'lists', pathMatch: 'full' },
  { path: 'login', loadComponent: () => import('./auth/login').then((m) => m.LoginComponent) },
  { path: 'register', loadComponent: () => import('./auth/register').then((m) => m.RegisterComponent) },
  { path: 'lists', loadComponent: () => import('./todo-list/todo-list').then((m) => m.TodoList), canActivate: [authGuard] },
  { path: 'lists/:id', loadComponent: () => import('./todo-list-detail/todo-list-detail').then((m) => m.TodoListDetail), canActivate: [authGuard] },
  // No authGuard — Terms/Privacy need to be readable before signing up.
  { path: 'terms', loadComponent: () => import('./legal/terms').then((m) => m.TermsComponent) },
  { path: 'privacy', loadComponent: () => import('./legal/privacy').then((m) => m.PrivacyComponent) },
];
