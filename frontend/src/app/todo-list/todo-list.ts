import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import { HttpClient } from '@angular/common/http';
import { catchError } from 'rxjs/operators';
import { of } from 'rxjs';
import { TodoListService } from '../services/todo-list.service';

// Typed models matching the API contract
export interface CreateTodoListCommand {
  name?: string | null;
}

export interface TodoListDto {
  id: number; // backend uses int for Id
  name?: string | null;
}

@Component({
  selector: 'app-todo-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './todo-list.html',
  styleUrls: ['./todo-list.css'],
})
export class TodoList implements OnInit {
  private http = inject(HttpClient);
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
    // HttpClient returns typed responses; proxy (or base URL) will forward to backend
    this.http.get<TodoListDto[]>('/api/v1/todolists')
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
    const body: CreateTodoListCommand = { name };
    this.http.post<void>('/api/v1/todolists', body)
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
    this.service.deleteList(id).subscribe(() => {
      this.pendingDeleteListId.set(null);
      this.load();
    }, err => this.error.set(err?.message ?? String(err)));
  }
}
