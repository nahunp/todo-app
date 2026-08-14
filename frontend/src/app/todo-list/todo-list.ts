import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { TodoListService } from '../services/todo-list.service';
import { TodoListDto } from '../models/todo';

@Component({
  selector: 'app-todo-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './todo-list.html',
  styleUrls: ['./todo-list.css'],
})
export class TodoList implements OnInit {
  private service = inject(TodoListService);

  todoLists = signal<TodoListDto[]>([]);
  loading = signal(true);
  error = signal('');

  // UI state for delete confirmation
  pendingDeleteListId = signal<number | null>(null);

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
}
