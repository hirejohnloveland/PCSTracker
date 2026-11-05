import { module, test } from 'qunit';
import { setupRenderingTest } from 'web/tests/helpers';
import { render } from '@ember/test-helpers';
import PaginationControls from 'web/components/pagination-controls';

module('Integration | Component | pagination-controls', function (hooks) {
  setupRenderingTest(hooks);

  test('on the first page Previous is disabled and Next is a link', async function (assert) {
    await render(
      <template><PaginationControls @page={{1}} @totalPages={{3}} /></template>,
    );

    assert.dom('nav').containsText('Page 1 of 3');
    assert.dom('span.disabled').hasText('Previous');
    assert.dom('a').hasText('Next');
  });

  test('on the last page Next is disabled and Previous is a link', async function (assert) {
    await render(
      <template><PaginationControls @page={{3}} @totalPages={{3}} /></template>,
    );

    assert.dom('nav').containsText('Page 3 of 3');
    assert.dom('span.disabled').hasText('Next');
    assert.dom('a').hasText('Previous');
  });

  test('it renders nothing when there are no pages', async function (assert) {
    await render(
      <template><PaginationControls @page={{1}} @totalPages={{0}} /></template>,
    );

    assert.dom('nav').doesNotExist();
  });
});
