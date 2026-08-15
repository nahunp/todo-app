import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { TodoListService } from '../services/todo-list.service';
import { TodoListDto } from '../models/todo';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-todo-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './todo-list.html',
  styleUrls: ['./todo-list.css'],
})
export class TodoList implements OnInit {
  private service = inject(TodoListService);
  private auth = inject(AuthService);
  private router = inject(Router);

  todoLists = signal<TodoListDto[]>([]);
  loading = signal(true);
  error = signal('');

  // UI state for delete confirmation
  pendingDeleteListId = signal<number | null>(null);

  // Same confirm-step pattern as list deletion, for the same reason —
  // this one's account-wide and permanent, so it gets at least as much
  // friction as deleting a single list does.
  confirmingAccountDeletion = signal(false);
  deletingAccount = signal(false);

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.error.set('');
    this.service.getLists()
      .pipe(
        catchError((err) => {
          this.error.set(err?.message ?? String(err));
          return of([] as TodoListDto[]);
        })
      )
      .subscribe((data) => {
        this.todoLists.set(Array.isArray(data) ? data : []);
        this.loading.set(false);
      });
  }

  create(name: string) {
    if (!name) return;
    this.service.createList(name)
      .pipe(
        catchError((err) => {
          this.error.set(err?.message ?? String(err));
          return of(null);
        })
      )
      .subscribe(() => this.load());
  }

  startDeleteList(id: number) {
    this.pendingDeleteListId.set(id);
  }

  cancelDeleteList() {
    this.pendingDeleteListId.set(null);
  }

  confirmDeleteList(id: number) {
    this.service.deleteList(id).subscribe({
      next: () => {
        this.pendingDeleteListId.set(null);
        this.load();
      },
      error: (err: unknown) => this.error.set((err as { message?: string })?.message ?? String(err)),
    });
  }

  startDeleteAccount() {
    this.confirmingAccountDeletion.set(true);
  }

  cancelDeleteAccount() {
    this.confirmingAccountDeletion.set(false);
  }

  confirmDeleteAccount() {
    this.deletingAccount.set(true);
    this.auth.deleteAccount().subscribe({
      next: () => {
        // logout() first — while the (now-invalid) token is still all the
        // auth interceptor has to attach, there's no request left to make
        // that needs it, but clearing local state before navigating keeps
        // the navbar/guards consistent with reality immediately, not
        // after whatever the router happens to re-evaluate next.
        this.auth.logout();
        this.router.navigate(['/login']);
      },
      error: (err: unknown) => {
        this.deletingAccount.set(false);
        this.confirmingAccountDeletion.set(false);
        this.error.set((err as { message?: string })?.message ?? String(err));
      },
    });
  }
}
