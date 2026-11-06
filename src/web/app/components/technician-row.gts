import type { TOC } from '@ember/component/template-only';
import { LinkTo } from '@ember/routing';
import type { Technician } from 'web/services/technicians-api';

interface TechnicianRowSignature {
  Element: HTMLTableRowElement;
  Args: {
    technician: Technician;
  };
}

<template>
  <tr ...attributes>
    <td>{{@technician.fullName}}</td>
    <td class="role">{{@technician.role}}</td>
    <td class="active">{{if @technician.active "Yes" "No"}}</td>
    <td class="actions">
      <LinkTo
        @route="technicians.details"
        @model={{@technician.id}}
        class="button"
      >Details</LinkTo>
    </td>
  </tr>
</template> satisfies TOC<TechnicianRowSignature>;
