import type { TOC } from '@ember/component/template-only';
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
    <td class="quickbooks-id">{{@vendor.quickbooksId}}</td>
    <td>{{if @vendor.isActive "Active" "Inactive"}}</td>
  </tr>
</template> satisfies TOC<VendorRowSignature>;
