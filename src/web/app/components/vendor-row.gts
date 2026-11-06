import type { TOC } from '@ember/component/template-only';
import { LinkTo } from '@ember/routing';
import type { Vendor } from 'web/services/vendors-api';

interface VendorRowSignature {
  Element: HTMLTableRowElement;
  Args: {
    vendor: Vendor;
  };
}

<template>
  <tr ...attributes>
    <td>{{@vendor.name}}</td>
    <td class="description">{{@vendor.description}}</td>
    <td class="actions">
      <LinkTo
        @route="vendors.details"
        @model={{@vendor.id}}
        class="button"
      >Details</LinkTo>
    </td>
  </tr>
</template> satisfies TOC<VendorRowSignature>;
