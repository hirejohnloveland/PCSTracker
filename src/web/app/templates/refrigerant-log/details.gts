import type { TOC } from '@ember/component/template-only';
import { pageTitle } from 'ember-page-title';
import RefrigerantLogForm from 'web/components/refrigerant-log-form';
import type { RefrigerantLogDetailsModel } from 'web/routes/refrigerant-log/details';

interface RefrigerantLogDetailsSignature {
  Args: {
    model: RefrigerantLogDetailsModel;
  };
}

<template>
  {{pageTitle "Refrigerant Log Entry"}}
  <div class="page-header">
    <h1>Refrigerant Log Entry</h1>
  </div>

  <RefrigerantLogForm
    @entry={{@model.entry}}
    @technicians={{@model.technicians}}
    @refrigerants={{@model.refrigerants}}
  />
</template> satisfies TOC<RefrigerantLogDetailsSignature>;
