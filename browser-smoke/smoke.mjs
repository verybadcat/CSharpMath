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
  if (!ctx) return { width: c.width, height: c.height, css: [c.clientWidth, c.clientHeight], dpr: window.devicePixelRatio, nonWhite: false, signature: '' };
  const data = ctx.getImageData(0, 0, c.width, c.height).data;
  let nonWhite = false, dark = 0, signature = 0;
  for (let i = 0; i < data.length; i += 4) {
    if (data[i + 3] > 0 && (data[i] < 245 || data[i + 1] < 245 || data[i + 2] < 245)) nonWhite = true, dark++;
    if (i % 16 === 0) signature = (signature * 31 + data[i] + data[i + 1] * 3 + data[i + 2] * 7) >>> 0;
  }
  return { width: c.width, height: c.height, css: [c.clientWidth, c.clientHeight], dpr: window.devicePixelRatio, nonWhite, dark, signature };
});
await page.waitForFunction(() => {
  const c = document.querySelector('canvas'), dpr = window.devicePixelRatio || 1, ctx = c?.getContext('2d');
  if (!ctx || Math.abs(c.width - c.clientWidth * dpr) > 1 || Math.abs(c.height - c.clientHeight * dpr) > 1) return false;
  const data = ctx.getImageData(0, 0, c.width, c.height).data;
  for (let i = 0; i < data.length; i += 4)
    if (data[i + 3] > 0 && (data[i] < 245 || data[i + 1] < 245 || data[i + 2] < 245)) return true;
  return false;
}, null, { timeout: 30000 });
const first = await readCanvas();
console.log(JSON.stringify({first, errors}));
const hasScaledBackingStore = sample => Math.abs(sample.width - sample.css[0] * sample.dpr) <= 1 && Math.abs(sample.height - sample.css[1] * sample.dpr) <= 1;
if (!first.nonWhite || !first.width || !first.height || !hasScaledBackingStore(first)) throw new Error('initial canvas was blank, uninitialized, or did not use a DPR-scaled backing store');
await page.locator('#latex').fill('\\notacommand{'); await page.locator('#formula-error').waitFor({ state: 'visible' });
await page.locator('#latex').fill('x^2 + y^2 = z^2'); await page.waitForTimeout(300);
await page.waitForFunction(previous => {
  const c = document.querySelector('canvas'), ctx = c?.getContext('2d'); if (!ctx) return false;
  const data = ctx.getImageData(0, 0, c.width, c.height).data; let nonWhite = false, signature = 0;
  for (let i = 0; i < data.length; i += 4) {
    if (data[i + 3] > 0 && (data[i] < 245 || data[i + 1] < 245 || data[i + 2] < 245)) nonWhite = true;
    if (i % 16 === 0) signature = (signature * 31 + data[i] + data[i + 1] * 3 + data[i + 2] * 7) >>> 0;
  }
  return nonWhite && signature !== previous;
}, first.signature, { timeout: 10000 });
const valid = await readCanvas();
if (!valid.nonWhite || valid.signature === first.signature) throw new Error('valid input did not change rendered pixels');
const before = valid.width;
await page.setViewportSize({ width: 700, height: 800 });
await page.waitForFunction(previousCssWidth => {
  const c = document.querySelector('canvas'), dpr = window.devicePixelRatio || 1, ctx = c?.getContext('2d');
  if (!ctx || c.clientWidth === previousCssWidth || Math.abs(c.width - c.clientWidth * dpr) > 1 || Math.abs(c.height - c.clientHeight * dpr) > 1) return false;
  const data = ctx.getImageData(0, 0, c.width, c.height).data;
  for (let i = 0; i < data.length; i += 4)
    if (data[i + 3] > 0 && (data[i] < 245 || data[i + 1] < 245 || data[i + 2] < 245)) return true;
  return false;
}, valid.css[0], { timeout: 10000 });
const resized = await readCanvas(); const after = resized.width;
if (!resized.nonWhite || before === after || !hasScaledBackingStore(resized)) throw new Error(`canvas did not respond to resize, was blank, or did not use a DPR-scaled backing store (${before} -> ${after})`);
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
