import type { TOC } from '@ember/component/template-only';
import { on } from '@ember/modifier';
import { pageTitle } from 'ember-page-title';
import type TechniciansDetailsController from 'web/controllers/technicians/details';

interface TechniciansDetailsSignature {
  Args: {
    controller: TechniciansDetailsController;
  };
}

<template>
  {{pageTitle "Technician Details"}}
  <div class="page-header">
    <h1>Technician Details</h1>
  </div>

  <form class="technician-details" {{on "submit" @controller.save}}>
    <label for="technician-full-name">Full Name</label>
    <input
      id="technician-full-name"
      name="fullName"
      type="text"
      aria-invalid={{if @controller.errors.fullName "true"}}
      aria-describedby={{if
        @controller.errors.fullName
        "technician-full-name-error"
      }}
      value={{@controller.fullName}}
      {{on "input" @controller.setText}}
    />
    {{#if @controller.errors.fullName}}
      <p class="field-error" id="technician-full-name-error">
        {{@controller.errors.fullName}}
      </p>
    {{/if}}

    <label for="technician-role">Role</label>
    <select
      id="technician-role"
      name="role"
      aria-invalid={{if @controller.errors.role "true"}}
      aria-describedby={{if @controller.errors.role "technician-role-error"}}
      {{on "change" @controller.setText}}
    >
      <option value="" selected={{@controller.roleUnselected}}>Select a role</option>
      {{#each @controller.roleOptions as |choice|}}
        <option value={{choice.value}} selected={{choice.selected}}>
          {{choice.value}}
        </option>
      {{/each}}
    </select>
    {{#if @controller.errors.role}}
      <p class="field-error" id="technician-role-error">
        {{@controller.errors.role}}
      </p>
    {{/if}}

    <label class="checkbox-label" for="technician-active">
      <input
        id="technician-active"
        name="active"
        type="checkbox"
        checked={{@controller.active}}
        {{on "change" @controller.setActive}}
      />
      Active
    </label>

    <label for="technician-phone">Phone</label>
    <input
      id="technician-phone"
      name="phone"
      type="tel"
      aria-invalid={{if @controller.errors.phone "true"}}
      aria-describedby={{if @controller.errors.phone "technician-phone-error"}}
      value={{@controller.phone}}
      {{on "input" @controller.setText}}
    />
    {{#if @controller.errors.phone}}
      <p class="field-error" id="technician-phone-error">
        {{@controller.errors.phone}}
      </p>
    {{/if}}

    <label for="technician-quickbooks-id">QuickBooks Id</label>
    <input
      id="technician-quickbooks-id"
      name="quickbooksId"
      type="text"
      aria-invalid={{if @controller.errors.quickbooksId "true"}}
      aria-describedby={{if
        @controller.errors.quickbooksId
        "technician-quickbooks-id-error"
      }}
      value={{@controller.quickbooksId}}
      {{on "input" @controller.setText}}
    />
    {{#if @controller.errors.quickbooksId}}
      <p class="field-error" id="technician-quickbooks-id-error">
        {{@controller.errors.quickbooksId}}
      </p>
    {{/if}}

    <label for="technician-notes">Notes</label>
    <textarea
      id="technician-notes"
      name="notes"
      rows="5"
      aria-invalid={{if @controller.errors.notes "true"}}
      aria-describedby={{if @controller.errors.notes "technician-notes-error"}}
      value={{@controller.notes}}
      {{on "input" @controller.setText}}
    ></textarea>
    {{#if @controller.errors.notes}}
      <p class="field-error" id="technician-notes-error">
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
</template> satisfies TOC<TechniciansDetailsSignature>;
