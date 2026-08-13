import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { TodoListDto, TodoListWithItemsDto, AddTodoItemCommand, TodoItemDto } from '../models/todo';

@Injectable({ providedIn: 'root' })
export class TodoListService {
  private http = inject(HttpClient);
  private base = '/api/todolists';

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
    return this.http.put<void>(`${this.base}/${listId}/items/${itemId}`, { todoListId: listId, todoItemId: itemId, newTitle });
  }

  removeItem(listId: number, itemId: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/${listId}/items/${itemId}`);
  }

  renameList(listId: number, newName: string): Observable<void> {
    return this.http.put<void>(`${this.base}/${listId}`, { todoListId: listId, newName });
  }
}
