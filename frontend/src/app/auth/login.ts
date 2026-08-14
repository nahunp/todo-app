import { Component, signal, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService } from '../services/auth.service';
import { extractErrorMessage } from '../shared/http-error';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './login.html',
  styleUrls: ['./login.css']
})
export class LoginComponent {
  private auth = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  email = signal('');
  password = signal('');
  error = signal('');
  loading = signal(false);
  // Set by RegisterComponent's redirect (?registered=1) after a successful registration.
  justRegistered = signal(this.route.snapshot.queryParamMap.get('registered') === '1');

  submit() {
    this.error.set('');
    if (!this.email() || !this.password()) {
      this.error.set('email and password required');
      return;
    }
    this.loading.set(true);
    this.auth.login(this.email(), this.password()).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/lists']);
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(extractErrorMessage(err));
      }
    });
  }
}
