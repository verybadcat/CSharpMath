import { chromium } from '@playwright/test';
const base = process.env.BLAZOR_BASE_URL ?? 'http://127.0.0.1:4173';
const launchOptions = process.env.BROWSER_EXECUTABLE_PATH ? { executablePath: process.env.BROWSER_EXECUTABLE_PATH, headless: true } : { channel: 'chromium', headless: true };
const browser = await chromium.launch(launchOptions);
const page = await browser.newPage({ deviceScaleFactor: 2, viewport: { width: 1100, height: 800 } });
const errors = []; page.on('pageerror', e => errors.push(String(e))); page.on('console', m => { if (m.type() === 'error') errors.push(m.text()); });
await page.goto(base, { waitUntil: 'networkidle' });
const canvas = page.locator('canvas'); await canvas.waitFor({ state: 'visible' });
await page.waitForFunction(() => { const c = document.querySelector('canvas'); return c && c.width > 0 && c.height > 0; });
const readCanvas = () => canvas.evaluate(c => {
  const ctx = c.getContext('2d');
  if (!ctx) return { width: c.width, height: c.height, css: [c.clientWidth, c.clientHeight], nonWhite: false, signature: '' };
  const data = ctx.getImageData(0, 0, c.width, c.height).data;
  let nonWhite = false, dark = 0, signature = 0;
  for (let i = 0; i < data.length; i += 4) {
    if (data[i + 3] > 0 && (data[i] < 245 || data[i + 1] < 245 || data[i + 2] < 245)) nonWhite = true, dark++;
    if (i % 16 === 0) signature = (signature * 31 + data[i] + data[i + 1] * 3 + data[i + 2] * 7) >>> 0;
  }
  return { width: c.width, height: c.height, css: [c.clientWidth, c.clientHeight], nonWhite, dark, signature };
});
await page.waitForFunction(() => { const c = document.querySelector('canvas'); return c && c.width === c.clientWidth && c.height === c.clientHeight; }, { timeout: 10000 });
const first = await readCanvas();
console.log(JSON.stringify({first, errors}));
if (!first.nonWhite || !first.width || !first.height || first.width !== first.css[0] || first.height !== first.css[1]) throw new Error('initial canvas was blank, uninitialized, or dimensions did not match CSS');
await page.locator('#latex').fill('\\notacommand{'); await page.locator('#formula-error').waitFor({ state: 'visible' });
await page.locator('#latex').fill('x^2 + y^2 = z^2'); await page.waitForTimeout(300);
await page.waitForFunction(previous => { const c = document.querySelector('canvas'), ctx = c?.getContext('2d'); if (!ctx) return false; const d = ctx.getImageData(0, 0, c.width, c.height).data; return d.some((v, i) => i % 4 === 3 && v > 0) && c.width > 0; }, first);
const valid = await readCanvas();
if (!valid.nonWhite || valid.signature === first.signature) throw new Error('valid input did not change rendered pixels');
const before = valid.width; await page.setViewportSize({ width: 700, height: 800 }); await page.waitForTimeout(500); const resized = await readCanvas(); const after = resized.width;
if (!resized.nonWhite || before === after || resized.width !== resized.css[0] || resized.height !== resized.css[1]) throw new Error(`canvas did not respond to resize, was blank, or dimensions mismatched (${before} -> ${after})`);
await page.locator('#latex').fill('\\frac{1}{2}\\\\\\int_0^1 x dx');
await page.waitForFunction(previous => {
  const c = document.querySelector('canvas'), ctx = c?.getContext('2d'); if (!ctx) return false;
  const data = ctx.getImageData(0, 0, c.width, c.height).data; let nonWhite = false, signature = 0;
  for (let i = 0; i < data.length; i += 4) {
    if (data[i + 3] > 0 && (data[i] < 245 || data[i + 1] < 245 || data[i + 2] < 245)) nonWhite = true;
    if (i % 16 === 0) signature = (signature * 31 + data[i] + data[i + 1] * 3 + data[i + 2] * 7) >>> 0;
  }
  return nonWhite && signature !== previous;
}, resized.signature, { timeout: 10000 });
const multiline = await readCanvas();
if (!multiline.nonWhite || multiline.signature === resized.signature) throw new Error('multiline input did not change rendered pixels after resize');
await page.goto(`${base}/missing`); await page.goto(base); await page.locator('canvas').waitFor({ state: 'visible' });
if (errors.length) throw new Error(`browser errors: ${errors.join('; ')}`);
console.log(JSON.stringify({ dpr: 2, first, valid, multiline, resized: [before, after], status: 'pass' })); await browser.close();
