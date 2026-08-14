import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterOutlet, RouterLink } from '@angular/router';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  imports: [CommonModule, RouterOutlet, RouterLink],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('frontend');
  protected readonly auth = inject(AuthService);
  private readonly router = inject(Router);

  // auth.logout() alone just clears the token — it doesn't move the user
  // off whatever route they were on, which left them stranded on /lists
  // looking like logout had silently failed. Navigate to /login explicitly
  // so the UI actually reflects the logged-out state.
  protected logout(): void {
    this.auth.logout();
    this.router.navigate(['/login']);
  }
}
