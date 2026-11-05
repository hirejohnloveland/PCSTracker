import { pageTitle } from 'ember-page-title';
import ErrorDialog from 'web/components/error-dialog';
import NavBar from 'web/components/nav-bar';

<template>
  {{pageTitle "Tracker"}}
  <NavBar />

  <main class="page">
    {{outlet}}
  </main>

  <ErrorDialog />
</template>
