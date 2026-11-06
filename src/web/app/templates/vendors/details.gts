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
      value={{@controller.name}}
      {{on "input" @controller.setText}}
    />

    <label for="vendor-description">Description</label>
    <textarea
      id="vendor-description"
      name="description"
      rows="3"
      value={{@controller.description}}
      {{on "input" @controller.setText}}
    ></textarea>

    <label for="vendor-contact">Contact</label>
    <input
      id="vendor-contact"
      name="contact"
      type="text"
      value={{@controller.contact}}
      {{on "input" @controller.setText}}
    />

    <label for="vendor-phone">Phone</label>
    <input
      id="vendor-phone"
      name="phone"
      type="tel"
      value={{@controller.phone}}
      {{on "input" @controller.setText}}
    />

    <label for="vendor-quickbooks-id">QuickBooks Id</label>
    <input
      id="vendor-quickbooks-id"
      name="quickbooksId"
      type="text"
      value={{@controller.quickbooksId}}
      {{on "input" @controller.setText}}
    />

    <label for="vendor-notes">Notes</label>
    <textarea
      id="vendor-notes"
      name="notes"
      rows="5"
      value={{@controller.notes}}
      {{on "input" @controller.setText}}
    ></textarea>

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
