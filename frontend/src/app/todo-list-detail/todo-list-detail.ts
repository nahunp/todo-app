import { Component, OnInit, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { TodoListService } from '../services/todo-list.service';
import { TodoListWithItemsDto, TodoItemDto } from '../models/todo';
import { catchError, of } from 'rxjs';

@Component({
  selector: 'app-todo-list-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './todo-list-detail.html',
  styleUrls: ['./todo-list-detail.css']
})
export class TodoListDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private service = inject(TodoListService);

  list = signal<TodoListWithItemsDto | null>(null);
  loading = signal(true);
  error = signal('');

  newTitle = signal('');

  ngOnInit(): void {
    const idStr = this.route.snapshot.paramMap.get('id');
    const id = idStr ? Number(idStr) : NaN;
    if (!id || isNaN(id)) {
      this.error.set('Invalid list id');
      this.loading.set(false);
      return;
    }
    this.load(id);
  }

  load(id: number) {
    this.loading.set(true);
    this.error.set('');
    this.service.getList(id).pipe(
      catchError(err => { this.error.set(err?.message ?? String(err)); return of(null); })
    ).subscribe(dto => {
      this.list.set(dto as TodoListWithItemsDto | null);
      this.loading.set(false);
    });
  }

  addItem() {
    const title = this.newTitle();
    if (!title) return;
    const listId = this.list()?.id;
    if (!listId) return;
    this.service.addItem({ todoListId: listId, title }).subscribe(() => {
      this.newTitle.set('');
      this.load(listId);
    }, err => this.error.set(err?.message ?? String(err)));
  }

  renameItem(item: TodoItemDto) {
    const newTitle = prompt('New title', item.title);
    if (!newTitle || newTitle === item.title) return;
    const listId = this.list()!.id;
    this.service.renameItem(listId, item.id, newTitle).subscribe(() => this.load(listId), err => this.error.set(err?.message ?? String(err)));
  }

  removeItem(item: TodoItemDto) {
    if (!confirm(`Remove item '${item.title}'?`)) return;
    const listId = this.list()!.id;
    this.service.removeItem(listId, item.id).subscribe(() => this.load(listId), err => this.error.set(err?.message ?? String(err)));
  }

  renameList() {
    const newName = prompt('New list name', this.list()!.name);
    if (!newName || newName === this.list()!.name) return;
    const listId = this.list()!.id;
    this.service.renameList(listId, newName).subscribe(() => this.load(listId), err => this.error.set(err?.message ?? String(err)));
  }

  goBack() {
    this.router.navigate(['/lists']);
  }
}
