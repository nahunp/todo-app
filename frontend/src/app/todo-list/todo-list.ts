import { Component, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-todo-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './todo-list.html',
  styleUrls: ['./todo-list.css'],
})
export class TodoList implements OnInit {
  todoLists = signal<Array<{ id?: string; name?: string }>>([]);
  loading = signal(true);
  error = signal('');

  ngOnInit(): void {
    this.load();
  }

  async load() {
    this.loading.set(true);
    this.error.set('');
    try {
      const res = await fetch('/api/todolists');
      if (!res.ok) {
        if (res.status === 404) {
          this.todoLists.set([]);
          this.error.set('GET /api/todolists not implemented on backend (404).');
        } else {
          const text = await res.text();
          throw new Error(`${res.status} ${res.statusText}: ${text}`);
        }
      } else {
        const data = await res.json();
        this.todoLists.set(Array.isArray(data) ? data : []);
      }
    } catch (err: any) {
      this.error.set(err?.message ?? String(err));
    } finally {
      this.loading.set(false);
    }
  }

  async create(name: string) {
    if (!name) return;
    try {
      const res = await fetch('/api/todolists', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name }),
      });
      if (!res.ok) {
        const text = await res.text();
        throw new Error(`${res.status} ${res.statusText}: ${text}`);
      }
      await this.load();
    } catch (err: any) {
      this.error.set(err?.message ?? String(err));
    }
  }
}
