export interface CreateTodoListCommand {
  name?: string | null;
}

export interface TodoListDto {
  id: number;
  name?: string | null;
}

export type PriorityLevel = 'Low' | 'Medium' | 'High';
export type TodoItemCategory = 'None' | 'Work' | 'Personal' | 'Health';
export type DueDateState = 'None' | 'Overdue' | 'Today' | 'Upcoming';

export interface AddTodoItemCommand {
  todoListId: number;
  title: string;
  notes?: string | null;
  priority?: PriorityLevel;
  dueDate?: string | null; // ISO string
  category?: TodoItemCategory;
}

export interface TodoItemDto {
  id: number;
  title: string;
  notes?: string | null;
  isDone: boolean;
  completedAt?: string | null;
  priority: PriorityLevel;
  dueDate?: string | null;
  category: TodoItemCategory;
  dueDateState: DueDateState;
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

export interface SetPriorityRequest {
  priority: PriorityLevel;
}

export interface SetDueDateRequest {
  dueDate: string | null;
}

export interface SetCategoryRequest {
  category: TodoItemCategory;
}
