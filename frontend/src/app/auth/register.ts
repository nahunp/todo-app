import { Component, signal, inject, computed, OnInit, AfterViewInit, OnDestroy, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AuthService, PasswordPolicy } from '../services/auth.service';
import { runtimeConfig } from '../runtime-config';

// Cloudflare's Turnstile script (loaded in index.html) attaches this
// global — not an npm package, there's no official Angular wrapper worth
// depending on for one widget. Explicit render()/remove() (not the
// data-sitekey auto-render form) so this component's own lifecycle
// (ngAfterViewInit/ngOnDestroy) controls exactly when the widget exists,
// rather than trusting a MutationObserver to notice Angular's DOM changes.
declare global {
  interface Window {
    turnstile?: {
      render(container: string | HTMLElement, options: {
        sitekey: string;
        callback: (token: string) => void;
        'expired-callback'?: () => void;
        'error-callback'?: () => void;
      }): string;
      remove(widgetId: string): void;
      reset(widgetId: string): void;
    };
  }
}

interface PasswordRule {
  label: string;
  met: boolean;
}

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './register.html',
  styleUrls: ['./register.css']
})
export class RegisterComponent implements OnInit, AfterViewInit, OnDestroy {
  private auth = inject(AuthService);
  private router = inject(Router);

  @ViewChild('turnstileContainer') private turnstileContainer!: ElementRef<HTMLDivElement>;
  private turnstileWidgetId: string | null = null;

  email = signal('');
  password = signal('');
  error = signal('');
  loading = signal(false);
  captchaToken = signal('');
  private passwordPolicy = signal<PasswordPolicy | null>(null);

  // Recomputed on every keystroke (password changes) and once the policy
  // arrives — never hardcoded, so this can't silently drift from what the
  // backend actually enforces. Empty until the policy loads, so the
  // checklist just doesn't render rather than showing wrong/guessed rules.
  passwordRules = computed<PasswordRule[]>(() => {
    const policy = this.passwordPolicy();
    if (!policy) return [];

    const pw = this.password();
    const rules: PasswordRule[] = [
      { label: `At least ${policy.requiredLength} characters`, met: pw.length >= policy.requiredLength },
    ];
    if (policy.requireDigit) rules.push({ label: 'At least one number', met: /\d/.test(pw) });
    if (policy.requireLowercase) rules.push({ label: 'At least one lowercase letter', met: /[a-z]/.test(pw) });
    if (policy.requireUppercase) rules.push({ label: 'At least one uppercase letter', met: /[A-Z]/.test(pw) });
    if (policy.requireNonAlphanumeric) rules.push({ label: 'At least one special character', met: /[^a-zA-Z0-9]/.test(pw) });
    if (policy.requiredUniqueChars > 1) {
      rules.push({ label: `At least ${policy.requiredUniqueChars} unique characters`, met: new Set(pw).size >= policy.requiredUniqueChars });
    }
    return rules;
  });

  ngOnInit(): void {
    // Non-fatal if this fails — the checklist just won't render, but
    // registration itself still works (the backend enforces the real
    // policy regardless of whether the frontend could preview it).
    this.auth.getPasswordPolicy().subscribe({
      next: (policy) => this.passwordPolicy.set(policy),
      error: () => {},
    });
  }

  ngAfterViewInit(): void {
    this.renderTurnstile();
  }

  ngOnDestroy(): void {
    if (this.turnstileWidgetId && window.turnstile) {
      window.turnstile.remove(this.turnstileWidgetId);
    }
  }

  private renderTurnstile(retriesLeft = 20): void {
    // index.html loads the Turnstile script async/defer — it usually beats
    // Angular's own bundle in practice, but isn't guaranteed to, so poll
    // briefly rather than assuming window.turnstile exists yet.
    if (!window.turnstile) {
      if (retriesLeft > 0) {
        setTimeout(() => this.renderTurnstile(retriesLeft - 1), 250);
      }
      return;
    }

    this.turnstileWidgetId = window.turnstile.render(this.turnstileContainer.nativeElement, {
      sitekey: runtimeConfig.turnstileSiteKey,
      callback: (token) => this.captchaToken.set(token),
      'expired-callback': () => this.captchaToken.set(''),
      'error-callback': () => this.captchaToken.set(''),
    });
  }

  submit() {
    this.error.set('');
    if (!this.email() || !this.password()) {
      this.error.set('email and password required');
      return;
    }
    if (!this.captchaToken()) {
      this.error.set('Please complete the verification challenge.');
      return;
    }

    this.loading.set(true);
    // backend register doesn't return a token — redirect to login after success
    this.auth.register(this.email(), this.password(), this.captchaToken()).subscribe({
      next: () => {
        this.loading.set(false);
        this.router.navigate(['/login'], { queryParams: { registered: '1' } });
      },
      error: (err) => {
        this.loading.set(false);
        this.error.set(err?.message ?? String(err));
        // A Turnstile token is single-use — a failed submit (e.g. a weak
        // password rejected by Identity) needs a fresh one, not a stale
        // token that'll just fail verification again on retry.
        if (this.turnstileWidgetId && window.turnstile) {
          window.turnstile.reset(this.turnstileWidgetId);
          this.captchaToken.set('');
        }
      }
    });
  }
}
