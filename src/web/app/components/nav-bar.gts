import { LinkTo } from '@ember/routing';

<template>
  <nav class="nav-bar" aria-label="Main">
    <LinkTo @route="index" class="nav-brand">Tracker</LinkTo>
    <ul class="nav-links">
      <li><LinkTo @route="index">Home</LinkTo></li>
    </ul>
  </nav>
</template>
