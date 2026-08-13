import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { catchError } from 'rxjs/operators';
import { of } from 'rxjs';

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
  imports: [CommonModule],
  templateUrl: './todo-list.html',
  styleUrls: ['./todo-list.css'],
})
export class TodoList implements OnInit {
  private http = inject(HttpClient);

  todoLists = signal<TodoListDto[]>([]);
  loading = signal(true);
  error = signal('');

  ngOnInit(): void {
    this.load();
  }

  load() {
    this.loading.set(true);
    this.error.set('');
    // HttpClient returns typed responses; proxy (or base URL) will forward to backend
    this.http.get<TodoListDto[]>('/api/todolists')
      .pipe(
        catchError((err) => {
          if (err?.status === 404) {
            this.todoLists.set([]);
            this.error.set('GET /api/todolists not implemented on backend (404).');
            return of([] as TodoListDto[]);
          }
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
    this.http.post<void>('/api/todolists', body)
      .pipe(
        catchError((err) => {
          this.error.set(err?.message ?? String(err));
          return of(null);
        })
      )
      .subscribe(() => this.load());
  }
}
