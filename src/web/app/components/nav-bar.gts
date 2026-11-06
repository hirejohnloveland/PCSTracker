import { LinkTo } from '@ember/routing';

<template>
  <nav class="nav-bar" aria-label="Main">
    <LinkTo @route="index" class="nav-brand">PCS Home</LinkTo>
    <ul class="nav-links">
      <li><LinkTo @route="vendors">Vendor Management</LinkTo></li>
      <li><LinkTo @route="customers">Customer Management</LinkTo></li>
    </ul>
  </nav>
</template>
