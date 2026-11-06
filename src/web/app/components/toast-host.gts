import Component from '@glimmer/component';
import { fn } from '@ember/helper';
import { on } from '@ember/modifier';
import { service } from '@ember/service';
import type ToastsService from 'web/services/toasts';

export default class ToastHost extends Component {
  @service declare toasts: ToastsService;

  <template>
    <div class="toast-host" role="status" aria-live="polite">
      {{#each this.toasts.items key="id" as |toast|}}
        <div class="toast {{toast.kind}}">
          <span>{{toast.message}}</span>
          <button
            type="button"
            class="toast-close"
            aria-label="Dismiss notification"
            {{on "click" (fn this.toasts.dismiss toast.id)}}
          >
            &times;
          </button>
        </div>
      {{/each}}
    </div>
  </template>
}
