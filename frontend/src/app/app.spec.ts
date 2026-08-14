import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { App } from './app';

describe('App', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [App],
      // App's nav now uses RouterLink (issue #44), which eagerly injects
      // ActivatedRoute — needs a router context even for a "does it
      // construct" test, not just routed-content tests.
      providers: [provideRouter([])],
    }).compileComponents();
  });

  it('should create the app', () => {
    const fixture = TestBed.createComponent(App);
    const app = fixture.componentInstance;
    expect(app).toBeTruthy();
  });

  // The old 'should render title' test asserted on the ng-new placeholder's
  // <h1>Hello, frontend</h1>, which app.html no longer has — it's just
  // <router-outlet /> now (see issue #30). Removed rather than rewritten;
  // routed content is exercised by TodoList/TodoListDetail's own specs
  // (once those exist), not App's.
});
