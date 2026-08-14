import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './register.html',
  styleUrls: ['./register.css']
})
export class RegisterComponent {
  private auth = inject(AuthService);
  private router = inject(Router);

  email = signal('');
  password = signal('');
  error = signal('');
  loading = signal(false);

  submit() {
    this.error.set('');
    if (!this.email() || !this.password()) {
      this.error.set('email and password required');
      return;
    }
    this.loading.set(true);
    // backend register doesn't return a token — redirect to login after success
    this.auth.register(this.email(), this.password()).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/login'], { queryParams: { registered: '1' } });
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.message ?? String(err));
      }
    });
  }
}
