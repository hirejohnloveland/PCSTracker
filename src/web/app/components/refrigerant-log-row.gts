import type { TOC } from '@ember/component/template-only';
import { LinkTo } from '@ember/routing';
import type { RefrigerantLogEntry } from 'web/services/refrigerant-log-api';

interface RefrigerantLogRowSignature {
  Element: HTMLTableRowElement;
  Args: {
    entry: RefrigerantLogEntry;
  };
}

// The API reports a date as yyyy-mm-dd. The log shows it as the spreadsheet does.
function formatDate(date: string): string {
  const [year, month, day] = date.split('-');
  return `${month}/${day}/${year}`;
}

<template>
  <tr ...attributes>
    <td class="date">{{formatDate @entry.date}}</td>
    <td>{{@entry.technician}}</td>
    <td class="ticket">{{@entry.ticketNumber}}</td>
    <td class="job">{{@entry.jobName}}</td>
    <td class="customer">{{@entry.customer}}</td>
    <td class="refrigerant">{{@entry.refrigerantType}}</td>
    <td class="amount">{{@entry.amountLbs}}</td>
    <td class="actions">
      <LinkTo
        @route="refrigerant-log.details"
        @model={{@entry.id}}
        class="button"
      >Details</LinkTo>
    </td>
  </tr>
</template> satisfies TOC<RefrigerantLogRowSignature>;
