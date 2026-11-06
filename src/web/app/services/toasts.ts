import { action } from '@ember/object';
import Service from '@ember/service';
import { tracked } from '@glimmer/tracking';

export type ToastKind = 'success';

export interface Toast {
  id: number;
  kind: ToastKind;
  message: string;
}

const AUTO_DISMISS_MS = 4000;

export default class ToastsService extends Service {
  @tracked items: Toast[] = [];

  autoDismissMs = AUTO_DISMISS_MS;

  private nextId = 1;
  private timers = new Map<number, ReturnType<typeof setTimeout>>();

  success(message: string): void {
    const id = this.nextId++;
    this.items = [...this.items, { id, kind: 'success', message }];
    this.timers.set(
      id,
      setTimeout(() => {
        this.dismiss(id);
      }, this.autoDismissMs),
    );
  }

  @action dismiss(id: number): void {
    clearTimeout(this.timers.get(id));
    this.timers.delete(id);
    this.items = this.items.filter((toast) => toast.id !== id);
  }

  willDestroy(): void {
    super.willDestroy();
    for (const timer of this.timers.values()) {
      clearTimeout(timer);
    }
    this.timers.clear();
  }
}
