import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TodoListDto, TodoListWithItemsDto, AddTodoItemCommand, TodoItemDto, RenameItemCommand, RenameListCommand } from '../models/todo';

@Injectable({ providedIn: 'root' })
export class TodoListService {
  private http = inject(HttpClient);
  private base = '/api/v1/todolists';

  getLists(): Observable<TodoListDto[]> {
    return this.http.get<TodoListDto[]>(this.base);
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

  renameList(listId: number, newName: string): Observable<void> {
    const body: RenameListCommand = { newName };
    return this.http.patch<void>(`${this.base}/${listId}`, body);
  }
}
