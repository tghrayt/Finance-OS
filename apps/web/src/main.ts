import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app';

const isSilentAuthIframe = window.self !== window.top && window.location.hash.includes('code=');

if (isSilentAuthIframe) {
  document.body.innerHTML = '';
} else {
  bootstrapApplication(App, appConfig)
    .catch((err) => console.error(err));
}
