import type { TOC } from '@ember/component/template-only';
import { on } from '@ember/modifier';
import { pageTitle } from 'ember-page-title';
import PaginationControls from 'web/components/pagination-controls';
import VendorRow from 'web/components/vendor-row';
import type VendorsIndexController from 'web/controllers/vendors/index';
import type { VendorPage } from 'web/services/vendors-api';

interface VendorsIndexSignature {
  Args: {
    model: VendorPage;
    controller: VendorsIndexController;
  };
}

<template>
  {{pageTitle "Vendors"}}
  <div class="page-header">
    <h1>Vendors</h1>
    <button type="button" {{on "click" @controller.syncWithQuickbooks}}>
      Sync with QuickBooks
    </button>
  </div>

  <form class="filter" {{on "submit" @controller.filterNow}}>
    <label for="vendor-name-filter">Name</label>
    <input
      id="vendor-name-filter"
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
    <div class="vendors-table-wrap">
      <table class="vendors">
        <thead>
          <tr>
            <th>Name</th>
            <th class="description">Description</th>
            <th class="contact">Contact</th>
            <th class="phone">Phone</th>
            <th class="notes">Notes</th>
            <th class="quickbooks-id">QuickBooks Id</th>
            <th>Status</th>
          </tr>
        </thead>
        <tbody>
          {{#each @model.data as |vendor|}}
            <VendorRow @vendor={{vendor}} />
          {{/each}}
        </tbody>
      </table>
    </div>
  {{else}}
    <p>No vendors found.</p>
  {{/if}}

  <PaginationControls
    @page={{@model.pagination.page}}
    @totalPages={{@model.pagination.totalPages}}
  />
</template> satisfies TOC<VendorsIndexSignature>;
