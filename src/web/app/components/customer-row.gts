import type { TOC } from '@ember/component/template-only';
import { LinkTo } from '@ember/routing';
import type { Customer } from 'web/services/customers-api';

interface CustomerRowSignature {
  Element: HTMLTableRowElement;
  Args: {
    customer: Customer;
  };
}

<template>
  <tr ...attributes>
    <td>{{@customer.name}}</td>
    <td class="description">{{@customer.description}}</td>
    <td class="actions">
      <LinkTo
        @route="customers.details"
        @model={{@customer.id}}
        class="button"
      >Details</LinkTo>
    </td>
  </tr>
</template> satisfies TOC<CustomerRowSignature>;
