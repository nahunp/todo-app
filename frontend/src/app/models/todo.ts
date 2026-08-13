export interface CreateTodoListCommand {
  name?: string | null;
}

export interface TodoListDto {
  id: number;
  name?: string | null;
}

export interface AddTodoItemCommand {
  todoListId: number;
  title: string;
  notes?: string | null;
  priority?: number;
  dueDate?: string | null; // ISO string
}

export interface TodoItemDto {
  id: number;
  title: string;
  notes?: string | null;
  isDone: boolean;
  priority: number;
  dueDate?: string | null;
}

export interface TodoListWithItemsDto {
  id: number;
  name: string;
  items: TodoItemDto[];
}

export interface RenameItemCommand {
  newTitle: string;
}

export interface RenameListCommand {
  newName: string;
}
