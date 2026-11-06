import type { TOC } from '@ember/component/template-only';
import { pageTitle } from 'ember-page-title';
import RefrigerantLogForm from 'web/components/refrigerant-log-form';
import type { RefrigerantLogNewModel } from 'web/routes/refrigerant-log/new';

interface RefrigerantLogNewSignature {
  Args: {
    model: RefrigerantLogNewModel;
  };
}

<template>
  {{pageTitle "Add Refrigerant Log Entry"}}
  <div class="page-header">
    <h1>Add Refrigerant Log Entry</h1>
  </div>

  <RefrigerantLogForm
    @technicians={{@model.technicians}}
    @refrigerants={{@model.refrigerants}}
  />
</template> satisfies TOC<RefrigerantLogNewSignature>;
