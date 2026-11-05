import Component from '@glimmer/component';
import { hash } from '@ember/helper';
import { LinkTo } from '@ember/routing';

export interface PaginationControlsSignature {
  Args: {
    page: number;
    totalPages: number;
  };
}

export default class PaginationControls extends Component<PaginationControlsSignature> {
  get isVisible(): boolean {
    return this.args.totalPages > 0;
  }

  get hasPrevious(): boolean {
    return this.args.page > 1;
  }

  get hasNext(): boolean {
    return this.args.page < this.args.totalPages;
  }

  get previousPage(): number {
    return Math.min(this.args.page - 1, this.args.totalPages);
  }

  get nextPage(): number {
    return this.args.page + 1;
  }

  <template>
    {{#if this.isVisible}}
      <nav class="pagination" aria-label="Pagination">
        {{#if this.hasPrevious}}
          <LinkTo @query={{hash page=this.previousPage}}>Previous</LinkTo>
        {{else}}
          <span class="disabled">Previous</span>
        {{/if}}

        <span>Page {{@page}} of {{@totalPages}}</span>

        {{#if this.hasNext}}
          <LinkTo @query={{hash page=this.nextPage}}>Next</LinkTo>
        {{else}}
          <span class="disabled">Next</span>
        {{/if}}
      </nav>
    {{/if}}
  </template>
}
