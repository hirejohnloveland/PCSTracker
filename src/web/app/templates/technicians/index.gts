import type { TOC } from '@ember/component/template-only';
import { on } from '@ember/modifier';
import { pageTitle } from 'ember-page-title';
import PaginationControls from 'web/components/pagination-controls';
import TechnicianRow from 'web/components/technician-row';
import type TechniciansIndexController from 'web/controllers/technicians/index';
import type { TechnicianPage } from 'web/services/technicians-api';

interface TechniciansIndexSignature {
  Args: {
    model: TechnicianPage;
    controller: TechniciansIndexController;
  };
}

<template>
  {{pageTitle "Technicians"}}
  <div class="page-header">
    <h1>Technicians</h1>
    <button type="button" {{on "click" @controller.syncWithQuickbooks}}>
      Sync with QuickBooks
    </button>
  </div>

  <form class="filter" {{on "submit" @controller.filterNow}}>
    <label for="technician-name-filter">Name</label>
    <input
      id="technician-name-filter"
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
    <div class="technicians-table-wrap">
      <table class="technicians">
        <thead>
          <tr>
            <th>Full Name</th>
            <th class="role">Role</th>
            <th class="active">Active</th>
            <th class="actions"><span
                class="visually-hidden"
              >Actions</span></th>
          </tr>
        </thead>
        <tbody>
          {{#each @model.data as |technician|}}
            <TechnicianRow @technician={{technician}} />
          {{/each}}
        </tbody>
      </table>
    </div>
  {{else}}
    <p>No technicians found.</p>
  {{/if}}

  <PaginationControls
    @page={{@model.pagination.page}}
    @totalPages={{@model.pagination.totalPages}}
  />
</template> satisfies TOC<TechniciansIndexSignature>;
