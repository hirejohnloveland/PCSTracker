import type { TOC } from '@ember/component/template-only';
import { on } from '@ember/modifier';
import { pageTitle } from 'ember-page-title';
import PaginationControls from 'web/components/pagination-controls';
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
    <table class="vendors">
      <thead>
        <tr>
          <th>Name</th>
          <th>Status</th>
        </tr>
      </thead>
      <tbody>
        {{#each @model.data as |vendor|}}
          <tr>
            <td>{{vendor.name}}</td>
            <td>{{if vendor.isActive "Active" "Inactive"}}</td>
          </tr>
        {{/each}}
      </tbody>
    </table>
  {{else}}
    <p>No vendors found.</p>
  {{/if}}

  <PaginationControls
    @page={{@model.pagination.page}}
    @totalPages={{@model.pagination.totalPages}}
  />
</template> satisfies TOC<VendorsIndexSignature>;
