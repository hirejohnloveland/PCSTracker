import type { TOC } from '@ember/component/template-only';
import { on } from '@ember/modifier';
import { pageTitle } from 'ember-page-title';
import CustomerRow from 'web/components/customer-row';
import PaginationControls from 'web/components/pagination-controls';
import type CustomersIndexController from 'web/controllers/customers/index';
import type { CustomerPage } from 'web/services/customers-api';

interface CustomersIndexSignature {
  Args: {
    model: CustomerPage;
    controller: CustomersIndexController;
  };
}

<template>
  {{pageTitle "Customers"}}
  <div class="page-header">
    <h1>Customers</h1>
    <button type="button" {{on "click" @controller.syncWithQuickbooks}}>
      Sync with QuickBooks
    </button>
  </div>

  <form class="filter" {{on "submit" @controller.filterNow}}>
    <label for="customer-name-filter">Name</label>
    <input
      id="customer-name-filter"
      name="name"
      type="search"
      value={{@controller.name}}
      {{on "input" @controller.filterAsYouType}}
    />
    <button
      type="button"
      disabled={{unless @controller.hasFilters true}}
      {{on "click" @controller.clearFilters}}
    >
      Clear filters
    </button>
  </form>

  {{#if @model.data.length}}
    <div class="customers-table-wrap">
      <table class="customers">
        <thead>
          <tr>
            <th>Name</th>
            <th class="description">Description</th>
            <th class="actions"><span
                class="visually-hidden"
              >Actions</span></th>
          </tr>
        </thead>
        <tbody>
          {{#each @model.data as |customer|}}
            <CustomerRow @customer={{customer}} />
          {{/each}}
        </tbody>
      </table>
    </div>
  {{else}}
    <p>No customers found.</p>
  {{/if}}

  <PaginationControls
    @page={{@model.pagination.page}}
    @totalPages={{@model.pagination.totalPages}}
  />
</template> satisfies TOC<CustomersIndexSignature>;
