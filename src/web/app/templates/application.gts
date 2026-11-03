import { pageTitle } from 'ember-page-title';
import NavBar from 'web/components/nav-bar';

<template>
  {{pageTitle "Tracker"}}
  <NavBar />

  <main class="page">
    {{outlet}}
  </main>
</template>
