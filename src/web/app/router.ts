import EmberRouter from '@embroider/router';
import config from 'web/config/environment';

export default class Router extends EmberRouter {
  location = config.locationType;
  rootURL = config.rootURL;
}

Router.map(function () {
  this.route('vendors', function () {
    this.route('details', { path: '/:vendor_id' });
  });
  this.route('customers', function () {
    this.route('details', { path: '/:customer_id' });
  });
  this.route('technicians', function () {
    this.route('details', { path: '/:technician_id' });
  });
});
