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

  // Inline edit / confirm UI state
  editingItemId = signal<number | null>(null);
  editingItemTitle = signal('');
  pendingDeleteItemId = signal<number | null>(null);

  editingList = signal(false);
  editingListName = signal('');

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

  // Inline rename flows for items
  startRenameItem(item: TodoItemDto) {
    this.editingItemId.set(item.id);
    this.editingItemTitle.set(item.title ?? '');
  }

  saveRenameItem() {
    const itemId = this.editingItemId();
    if (itemId == null) return;
    const newTitle = this.editingItemTitle();
    if (!newTitle) return;
    const listId = this.list()!.id;
    this.service.renameItem(listId, itemId, newTitle).subscribe(() => {
      this.editingItemId.set(null);
      this.editingItemTitle.set('');
      this.load(listId);
    }, err => this.error.set(err?.message ?? String(err)));
  }

  cancelRenameItem() {
    this.editingItemId.set(null);
    this.editingItemTitle.set('');
  }

  // In-page remove confirmation
  startRemoveItem(item: TodoItemDto) {
    this.pendingDeleteItemId.set(item.id);
  }

  confirmRemoveItem() {
    const itemId = this.pendingDeleteItemId();
    if (itemId == null) return;
    const listId = this.list()!.id;
    this.service.removeItem(listId, itemId).subscribe(() => {
      this.pendingDeleteItemId.set(null);
      this.load(listId);
    }, err => this.error.set(err?.message ?? String(err)));
  }

  cancelRemoveItem() {
    this.pendingDeleteItemId.set(null);
  }

  // Complete / Reopen using POST endpoints
  toggleComplete(item: TodoItemDto) {
    const listId = this.list()!.id;
    if (item.isDone) {
      this.service.reopenItem(listId, item.id).subscribe(() => this.load(listId), err => this.error.set(err?.message ?? String(err)));
    } else {
      this.service.completeItem(listId, item.id).subscribe(() => this.load(listId), err => this.error.set(err?.message ?? String(err)));
    }
  }

  // Inline rename for list
  startRenameList() {
    this.editingListName.set(this.list()!.name ?? '');
    this.editingList.set(true);
  }

  saveRenameList() {
    const newName = this.editingListName();
    if (!newName) return;
    const listId = this.list()!.id;
    this.service.renameList(listId, newName).subscribe(() => {
      this.editingList.set(false);
      this.editingListName.set('');
      this.load(listId);
    }, err => this.error.set(err?.message ?? String(err)));
  }

  cancelRenameList() {
    this.editingList.set(false);
    this.editingListName.set('');
  }

  goBack() {
    this.router.navigate(['/lists']);
  }
}
