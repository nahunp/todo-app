import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TodoListDto, TodoListWithItemsDto, AddTodoItemCommand, TodoItemDto, RenameItemCommand, RenameListCommand, CreateTodoListCommand, PriorityLevel, TodoItemCategory, SetPriorityRequest, SetDueDateRequest, SetCategoryRequest } from '../models/todo';
import { runtimeConfig } from '../runtime-config';

@Injectable({ providedIn: 'root' })
export class TodoListService {
  private http = inject(HttpClient);
  // Getter, not a cached field — see AuthService's identical comment.
  private get base() { return `${runtimeConfig.apiBaseUrl}/api/v1/todolists`; }

  getLists(): Observable<TodoListDto[]> {
    return this.http.get<TodoListDto[]>(this.base);
  }

  createList(name: string): Observable<void> {
    const body: CreateTodoListCommand = { name };
    return this.http.post<void>(this.base, body);
  }

  getList(id: number): Observable<TodoListWithItemsDto> {
    return this.http.get<TodoListWithItemsDto>(`${this.base}/${id}`);
  }

  addItem(cmd: AddTodoItemCommand): Observable<{ id: number }> {
    return this.http.post<{ id: number }>(`${this.base}/${cmd.todoListId}/items`, cmd);
  }

  renameItem(listId: number, itemId: number, newTitle: string): Observable<void> {
    const body: RenameItemCommand = { newTitle };
    return this.http.patch<void>(`${this.base}/${listId}/items/${itemId}`, body);
  }

  removeItem(listId: number, itemId: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${listId}/items/${itemId}`);
  }

  completeItem(listId: number, itemId: number): Observable<void> {
    return this.http.post<void>(`${this.base}/${listId}/items/${itemId}/complete`, null);
  }

  reopenItem(listId: number, itemId: number): Observable<void> {
    return this.http.post<void>(`${this.base}/${listId}/items/${itemId}/reopen`, null);
  }

  setPriority(listId: number, itemId: number, priority: PriorityLevel): Observable<void> {
    const body: SetPriorityRequest = { priority };
    return this.http.patch<void>(`${this.base}/${listId}/items/${itemId}/priority`, body);
  }

  setDueDate(listId: number, itemId: number, dueDate: string | null): Observable<void> {
    const body: SetDueDateRequest = { dueDate };
    return this.http.patch<void>(`${this.base}/${listId}/items/${itemId}/due-date`, body);
  }

  setCategory(listId: number, itemId: number, category: TodoItemCategory): Observable<void> {
    const body: SetCategoryRequest = { category };
    return this.http.patch<void>(`${this.base}/${listId}/items/${itemId}/category`, body);
  }

  renameList(listId: number, newName: string): Observable<void> {
    const body: RenameListCommand = { newName };
    return this.http.patch<void>(`${this.base}/${listId}`, body);
  }

  deleteList(listId: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${listId}`);
  }
}
