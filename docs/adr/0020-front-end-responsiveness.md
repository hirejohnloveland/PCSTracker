# ADR 0020: Front-End Layout Is Responsive by Default

## Context

The PCS front end is used on phones, tablets, and desktops. Layout that assumes a wide viewport overflows, truncates, or forces awkward horizontal scrolling. Without a shared approach, each screen invents its own breakpoint, wrapping rules, and table treatment, and agents keep shipping desktop-only markup.

## Decision

- Every new or changed screen is usable at a phone-width viewport. The document already declares a mobile viewport ([`index.html`](../../src/web/index.html)).
- Styles are mobile-first in [`app.css`](../../src/web/app/styles/app.css). Base rules fit a narrow viewport. Wider spacing and fixed column widths are added with `@media (width >= 40rem)`. Do not introduce other breakpoints unless a screen truly needs a third layout tier, and document why in the change.
- Horizontal toolbars (nav, page headers, filters, pagination) use flexbox with `flex-wrap`. They stack or wrap on a narrow viewport. Do not add a separate mobile nav (hamburger, off-canvas) while the link set stays small.
- Data tables sit in a wrapper with `overflow-x: auto` so wide content scrolls inside the table, not the page. Cell text uses `overflow-wrap: anywhere`. Prefer this over card-list alternate markup unless a table truly cannot work on a phone.
- Dialogs and other overlays stay within the viewport (`width` capped relative to the screen, `max-width` for readability). Long reference strings wrap.
- Prefer CSS adjustments over duplicate mobile templates. Markup changes for responsiveness are limited to structural wrappers that CSS needs (such as a table scroll container).

## Consequences

- Agents and developers treat phone width as the default layout target, then enhance for wider viewports.
- One shared breakpoint keeps the stylesheet predictable. Extra breakpoints need a reason.
- Tables remain tables; narrow screens may scroll horizontally inside the wrapper.
- A denser nav or multi-column form may later need a dedicated pattern; this ADR does not invent those yet.

## Reference implementation

- Shared styles and the `40rem` breakpoint: [`app.css`](../../src/web/app/styles/app.css)
- Table scroll wrapper: [`vendors/index` template](../../src/web/app/templates/vendors/index.gts), `.vendors-table-wrap`
- Wrapping chrome: `.nav-bar`, `.page-header`, `.filter`, `.pagination` in [`app.css`](../../src/web/app/styles/app.css)
