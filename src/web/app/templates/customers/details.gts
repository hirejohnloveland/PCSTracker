import type { TOC } from '@ember/component/template-only';
import { on } from '@ember/modifier';
import { pageTitle } from 'ember-page-title';
import type CustomersDetailsController from 'web/controllers/customers/details';

interface CustomersDetailsSignature {
  Args: {
    controller: CustomersDetailsController;
  };
}

<template>
  {{pageTitle "Customer Details"}}
  <div class="page-header">
    <h1>Customer Details</h1>
  </div>

  <form class="customer-details" {{on "submit" @controller.save}}>
    <label for="customer-name">Name</label>
    <input
      id="customer-name"
      name="name"
      type="text"
      aria-invalid={{if @controller.errors.name "true"}}
      aria-describedby={{if @controller.errors.name "customer-name-error"}}
      value={{@controller.name}}
      {{on "input" @controller.setText}}
    />
    {{#if @controller.errors.name}}
      <p class="field-error" id="customer-name-error">
        {{@controller.errors.name}}
      </p>
    {{/if}}

    <label for="customer-description">Description</label>
    <textarea
      id="customer-description"
      name="description"
      rows="3"
      aria-invalid={{if @controller.errors.description "true"}}
      aria-describedby={{if
        @controller.errors.description
        "customer-description-error"
      }}
      value={{@controller.description}}
      {{on "input" @controller.setText}}
    ></textarea>
    {{#if @controller.errors.description}}
      <p class="field-error" id="customer-description-error">
        {{@controller.errors.description}}
      </p>
    {{/if}}

    <label for="customer-contact">Contact</label>
    <input
      id="customer-contact"
      name="contact"
      type="text"
      aria-invalid={{if @controller.errors.contact "true"}}
      aria-describedby={{if
        @controller.errors.contact
        "customer-contact-error"
      }}
      value={{@controller.contact}}
      {{on "input" @controller.setText}}
    />
    {{#if @controller.errors.contact}}
      <p class="field-error" id="customer-contact-error">
        {{@controller.errors.contact}}
      </p>
    {{/if}}

    <label for="customer-phone">Phone</label>
    <input
      id="customer-phone"
      name="phone"
      type="tel"
      aria-invalid={{if @controller.errors.phone "true"}}
      aria-describedby={{if @controller.errors.phone "customer-phone-error"}}
      value={{@controller.phone}}
      {{on "input" @controller.setText}}
    />
    {{#if @controller.errors.phone}}
      <p class="field-error" id="customer-phone-error">
        {{@controller.errors.phone}}
      </p>
    {{/if}}

    <label for="customer-quickbooks-id">QuickBooks Id</label>
    <input
      id="customer-quickbooks-id"
      name="quickbooksId"
      type="text"
      aria-invalid={{if @controller.errors.quickbooksId "true"}}
      aria-describedby={{if
        @controller.errors.quickbooksId
        "customer-quickbooks-id-error"
      }}
      value={{@controller.quickbooksId}}
      {{on "input" @controller.setText}}
    />
    {{#if @controller.errors.quickbooksId}}
      <p class="field-error" id="customer-quickbooks-id-error">
        {{@controller.errors.quickbooksId}}
      </p>
    {{/if}}

    <label for="customer-notes">Notes</label>
    <textarea
      id="customer-notes"
      name="notes"
      rows="5"
      aria-invalid={{if @controller.errors.notes "true"}}
      aria-describedby={{if @controller.errors.notes "customer-notes-error"}}
      value={{@controller.notes}}
      {{on "input" @controller.setText}}
    ></textarea>
    {{#if @controller.errors.notes}}
      <p class="field-error" id="customer-notes-error">
        {{@controller.errors.notes}}
      </p>
    {{/if}}

    <div class="form-actions">
      <button type="submit" disabled={{@controller.saveDisabled}}>
        Save
      </button>
      <button type="button" {{on "click" @controller.back}}>
        Back to List
      </button>
    </div>
  </form>
</template> satisfies TOC<CustomersDetailsSignature>;
