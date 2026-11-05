import Component from '@glimmer/component';
import { on } from '@ember/modifier';
import { service } from '@ember/service';
import { modifier } from 'ember-modifier';
import type ErrorDialogService from 'web/services/error-dialog';

const showModal = modifier((dialog: HTMLDialogElement) => {
  dialog.showModal();

  return () => {
    if (dialog.open) {
      dialog.close();
    }
  };
});

export default class ErrorDialog extends Component {
  @service declare errorDialog: ErrorDialogService;

  <template>
    {{#if this.errorDialog.current}}
      <dialog
        class="error-dialog {{this.errorDialog.current.severity}}"
        aria-labelledby="error-dialog-title"
        {{showModal}}
        {{on "close" this.errorDialog.dismiss}}
      >
        <h2 id="error-dialog-title">{{this.errorDialog.current.title}}</h2>
        <p>{{this.errorDialog.current.message}}</p>
        {{#if this.errorDialog.current.traceId}}
          <p class="trace-id">
            Reference:
            <code>{{this.errorDialog.current.traceId}}</code>
          </p>
        {{/if}}
        <button type="button" {{on "click" this.errorDialog.dismiss}}>
          Close
        </button>
      </dialog>
    {{/if}}
  </template>
}
