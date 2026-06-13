/**
 * Build a URL query string from a params object, skipping `undefined`/`null`
 * values. Returns `""` when nothing is set, or `"?a=1&b=2"` otherwise.
 * @module
 */
export function query(params: object): string {
  const sp = new URLSearchParams();
  for (const [k, v] of Object.entries(params)) {
    if (v === undefined || v === null) continue;
    sp.append(k, String(v));
  }
  const s = sp.toString();
  return s ? `?${s}` : '';
}
