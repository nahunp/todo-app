import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

// No async config-loading step needed here — index.html's
// <script src="config.js"> runs synchronously before this bundle even
// starts, so window.__appConfig is already set. See runtime-config.ts.
bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
