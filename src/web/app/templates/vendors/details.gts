import type { TOC } from '@ember/component/template-only';
import { on } from '@ember/modifier';
import { pageTitle } from 'ember-page-title';
import type VendorsDetailsController from 'web/controllers/vendors/details';

interface VendorsDetailsSignature {
  Args: {
    controller: VendorsDetailsController;
  };
}

<template>
  {{pageTitle "Vendor Details"}}
  <div class="page-header">
    <h1>Vendor Details</h1>
  </div>

  <form class="vendor-details" {{on "submit" @controller.save}}>
    <label for="vendor-name">Name</label>
    <input
      id="vendor-name"
      name="name"
      type="text"
      aria-invalid={{if @controller.errors.name "true"}}
      aria-describedby={{if @controller.errors.name "vendor-name-error"}}
      value={{@controller.name}}
      {{on "input" @controller.setText}}
    />
    {{#if @controller.errors.name}}
      <p class="field-error" id="vendor-name-error">
        {{@controller.errors.name}}
      </p>
    {{/if}}

    <label for="vendor-description">Description</label>
    <textarea
      id="vendor-description"
      name="description"
      rows="3"
      aria-invalid={{if @controller.errors.description "true"}}
      aria-describedby={{if
        @controller.errors.description
        "vendor-description-error"
      }}
      value={{@controller.description}}
      {{on "input" @controller.setText}}
    ></textarea>
    {{#if @controller.errors.description}}
      <p class="field-error" id="vendor-description-error">
        {{@controller.errors.description}}
      </p>
    {{/if}}

    <label for="vendor-contact">Contact</label>
    <input
      id="vendor-contact"
      name="contact"
      type="text"
      aria-invalid={{if @controller.errors.contact "true"}}
      aria-describedby={{if @controller.errors.contact "vendor-contact-error"}}
      value={{@controller.contact}}
      {{on "input" @controller.setText}}
    />
    {{#if @controller.errors.contact}}
      <p class="field-error" id="vendor-contact-error">
        {{@controller.errors.contact}}
      </p>
    {{/if}}

    <label for="vendor-phone">Phone</label>
    <input
      id="vendor-phone"
      name="phone"
      type="tel"
      aria-invalid={{if @controller.errors.phone "true"}}
      aria-describedby={{if @controller.errors.phone "vendor-phone-error"}}
      value={{@controller.phone}}
      {{on "input" @controller.setText}}
    />
    {{#if @controller.errors.phone}}
      <p class="field-error" id="vendor-phone-error">
        {{@controller.errors.phone}}
      </p>
    {{/if}}

    <label for="vendor-quickbooks-id">QuickBooks Id</label>
    <input
      id="vendor-quickbooks-id"
      name="quickbooksId"
      type="text"
      aria-invalid={{if @controller.errors.quickbooksId "true"}}
      aria-describedby={{if
        @controller.errors.quickbooksId
        "vendor-quickbooks-id-error"
      }}
      value={{@controller.quickbooksId}}
      {{on "input" @controller.setText}}
    />
    {{#if @controller.errors.quickbooksId}}
      <p class="field-error" id="vendor-quickbooks-id-error">
        {{@controller.errors.quickbooksId}}
      </p>
    {{/if}}

    <label for="vendor-notes">Notes</label>
    <textarea
      id="vendor-notes"
      name="notes"
      rows="5"
      aria-invalid={{if @controller.errors.notes "true"}}
      aria-describedby={{if @controller.errors.notes "vendor-notes-error"}}
      value={{@controller.notes}}
      {{on "input" @controller.setText}}
    ></textarea>
    {{#if @controller.errors.notes}}
      <p class="field-error" id="vendor-notes-error">
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
</template> satisfies TOC<VendorsDetailsSignature>;
