import type { TOC } from '@ember/component/template-only';
import { on } from '@ember/modifier';
import { LinkTo } from '@ember/routing';
import { pageTitle } from 'ember-page-title';
import PaginationControls from 'web/components/pagination-controls';
import RefrigerantLogRow from 'web/components/refrigerant-log-row';
import type RefrigerantLogIndexController from 'web/controllers/refrigerant-log/index';
import type { RefrigerantLogIndexModel } from 'web/routes/refrigerant-log/index';

interface RefrigerantLogIndexSignature {
  Args: {
    model: RefrigerantLogIndexModel;
    controller: RefrigerantLogIndexController;
  };
}

<template>
  {{pageTitle "Refrigerant Log"}}
  <div class="page-header">
    <h1>Refrigerant Log</h1>
    <LinkTo @route="refrigerant-log.new" class="button">Add Entry</LinkTo>
  </div>

  <form class="filter" {{on "submit" @controller.filterNow}}>
    <div class="filter-field">
      <label for="refrigerant-log-date-from">From</label>
      <input
        id="refrigerant-log-date-from"
        name="dateFrom"
        type="date"
        value={{@controller.dateFrom}}
        {{on "change" @controller.applyFilter}}
      />
    </div>

    <div class="filter-field">
      <label for="refrigerant-log-date-to">To</label>
      <input
        id="refrigerant-log-date-to"
        name="dateTo"
        type="date"
        aria-describedby={{if
          @controller.dateRangeError
          "refrigerant-log-date-range-error"
        }}
        value={{@controller.dateTo}}
        {{on "change" @controller.applyFilter}}
      />
    </div>

    <div class="filter-field">
      <label for="refrigerant-log-technician-filter">Technician</label>
      <select
        id="refrigerant-log-technician-filter"
        name="technician"
        {{on "change" @controller.applyFilter}}
      >
        <option value="" selected={{unless @controller.technician true}}>All
          technicians</option>
        {{#each @controller.technicianOptions as |choice|}}
          <option value={{choice.value}} selected={{choice.selected}}>
            {{choice.value}}
          </option>
        {{/each}}
      </select>
    </div>

    <div class="filter-field filter-field-grow">
      <label for="refrigerant-log-customer-filter">Customer</label>
      <input
        id="refrigerant-log-customer-filter"
        name="customer"
        type="search"
        value={{@controller.customer}}
        {{on "input" @controller.filterCustomerAsYouType}}
      />
    </div>

    <button
      type="button"
      disabled={{unless @controller.hasFilters true}}
      {{on "click" @controller.clearFilters}}
    >
      Clear filters
    </button>
  </form>
  {{#if @controller.dateRangeError}}
    <p class="field-error" id="refrigerant-log-date-range-error">
      {{@controller.dateRangeError}}
    </p>
  {{/if}}

  {{#if @model.entries.data.length}}
    <div class="refrigerant-log-table-wrap">
      <table class="refrigerant-log">
        <thead>
          <tr>
            <th class="date">Date</th>
            <th>Technician</th>
            <th class="ticket">Ticket</th>
            <th class="job">Job</th>
            <th class="customer">Customer</th>
            <th class="refrigerant">Refrigerant</th>
            <th class="amount">Lbs</th>
            <th class="actions"><span
                class="visually-hidden"
              >Actions</span></th>
          </tr>
        </thead>
        <tbody>
          {{#each @model.entries.data as |entry|}}
            <RefrigerantLogRow @entry={{entry}} />
          {{/each}}
        </tbody>
      </table>
    </div>
  {{else}}
    <p>No refrigerant log entries found.</p>
  {{/if}}

  <PaginationControls
    @page={{@model.entries.pagination.page}}
    @totalPages={{@model.entries.pagination.totalPages}}
  />
</template> satisfies TOC<RefrigerantLogIndexSignature>;
