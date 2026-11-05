import { action } from '@ember/object';
import Service from '@ember/service';
import { tracked } from '@glimmer/tracking';
import { ApiError } from 'web/services/api-client';

export type ErrorSeverity = 'warning' | 'error';

export interface ErrorDialogContent {
  severity: ErrorSeverity;
  title: string;
  message: string;
  traceId: string | null;
}

const UNEXPECTED_MESSAGE = 'An unexpected error occurred.';

function toContent(error: unknown): ErrorDialogContent {
  if (!(error instanceof ApiError)) {
    return {
      severity: 'error',
      title: 'Something went wrong',
      message: UNEXPECTED_MESSAGE,
      traceId: null,
    };
  }

  const message = error.detail ?? error.title;

  if (error.status >= 400 && error.status < 500) {
    return {
      severity: 'warning',
      title: 'Unable to complete this action',
      message,
      traceId: null,
    };
  }

  return {
    severity: 'error',
    title: 'Something went wrong',
    message,
    traceId: error.status >= 500 ? error.traceId : null,
  };
}

export default class ErrorDialogService extends Service {
  @tracked current: ErrorDialogContent | null = null;

  show(error: unknown): void {
    this.current = toContent(error);
  }

  @action dismiss(): void {
    this.current = null;
  }
}
