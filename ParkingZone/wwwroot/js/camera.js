// ====== Referencias ======
const video = document.getElementById('video');
const canvas = document.getElementById('canvas');
const btnStart = document.getElementById('btnStart');
const btnCapture = document.getElementById('btnCapture');
const btnOCR = document.getElementById('btnOCR');
const btnSave = document.getElementById('btnSave');
const statusEl = document.getElementById('status');
const resultEl = document.getElementById('result');

let stream = null;

// Valores detectados por OCR (para guardar)
let lastPlate = '';
let lastDigits = '';
let lastConfidence = 0.0;

// ====== Utilidades ======
function logStatus(msg) {
    statusEl.textContent = msg;
    console.log('[OCR]', msg);
}
function canvasToBlobPng(c) {
    return new Promise((resolve, reject) => {
        if (!c || !c.width) return reject(new Error('Canvas vacío'));
        c.toBlob(b => b ? resolve(b) : reject(new Error('No se pudo generar PNG')), 'image/png');
    });
}
function enable(el, on = true) {
    if (!el) return;
    if (on) el.removeAttribute('disabled');
    else el.setAttribute('disabled', 'true');
}

// ====== Iniciar cámara (1080p ideal) ======
btnStart?.addEventListener('click', async () => {
    try {
        if (!stream) {
            stream = await navigator.mediaDevices.getUserMedia({
                video: { facingMode: 'environment', width: { ideal: 1920 }, height: { ideal: 1080 } },
                audio: false
            });
            video.srcObject = stream;
        }
        logStatus('Cámara iniciada');
    } catch (err) {
        logStatus('Error al iniciar cámara: ' + err.message);
    }
});

// ====== Capturar frame ======
btnCapture?.addEventListener('click', () => {
    if (!video.videoWidth) return logStatus('Esperando video...');

    const w = Math.min(1920, video.videoWidth);
    const h = Math.round(video.videoHeight * (w / video.videoWidth));

    canvas.width = w;
    canvas.height = h;

    const ctx = canvas.getContext('2d');
    ctx.filter = 'grayscale(100%) contrast(130%) brightness(105%)';
    ctx.drawImage(video, 0, 0, w, h);
    ctx.filter = 'none';

    enable(btnSave, false);
    lastPlate = '';
    lastDigits = '';
    lastConfidence = 0;

    logStatus('Imagen capturada. Presiona "Analizar placa".');
});

// ====== Analizar imagen (OCR) ======
btnOCR?.addEventListener('click', async () => {
    if (!canvas.width) return logStatus('Captura primero una imagen');

    try {
        logStatus('Preprocesando...');
        resultEl.textContent = '';

        const ctx = canvas.getContext('2d');
        const imgData = ctx.getImageData(0, 0, canvas.width, canvas.height);
        const processed = preprocess(imgData);

        const temp = document.createElement('canvas');
        temp.width = processed.width;
        temp.height = processed.height;
        temp.getContext('2d').putImageData(processed, 0, 0);

        const dataUrl = temp.toDataURL('image/png');

        logStatus('Analizando con Tesseract...');
        const worker = await Tesseract.createWorker('eng', 1, { logger: m => console.log(m) });

        await worker.setParameters({
            tessedit_pageseg_mode: '7',
            tessedit_char_whitelist: 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789',
            user_defined_dpi: '400',
            preserve_interword_spaces: '1'
        });

        const { data } = await worker.recognize(dataUrl);
        await worker.terminate();

        const raw = (data.text || '').toUpperCase().replace(/\s+/g, '');
        const base = normalizeText(raw);

        // Generar variantes (por confusiones comunes)
        const variants = unique([
            base,
            base.replace(/0/g, 'O'),
            base.replace(/O/g, '0'),
            base.replace(/1/g, 'I'),
            base.replace(/I/g, '1'),
            base.replace(/5/g, 'S'),
            base.replace(/S/g, '5'),
            base.replace(/8/g, 'B'),
            base.replace(/B/g, '8'),
            base.replace(/Z/g, '2'),
            base.replace(/G/g, '6'),
            base.replace(/Q/g, '0'),
            base.replace(/0/g, 'O').replace(/1/g, 'I').replace(/5/g, 'S')
        ]);

        // Intentar detectar placa
        let plate = '';
        let digits = '';
        for (const text of variants) {
            const res = extractPlate(text);
            if (res.plate || res.digits) {
                plate = res.plate || plate;
                digits = res.digits || digits;
                if (plate && digits) break;
            }
        }

        lastPlate = plate;
        lastDigits = digits;
        lastConfidence = lastPlate ? 0.9 : (lastDigits ? 0.6 : 0.0);

        resultEl.textContent =
            `RAW:
${raw}

NORMALIZADO:
${base}

Placa: ${lastPlate || '(no detectada)'}
Dígitos: ${lastDigits || '(no detectados)'}`;

        enable(btnSave, true);
        logStatus('Listo ✅ (presiona Guardar escaneo)');
    } catch (err) {
        console.error(err);
        logStatus('Error: ' + err.message);
        enable(btnSave, false);
    }
});

// ====== Guardar (envía al backend /api/plate) ======
btnSave?.addEventListener('click', async () => {
    if (!canvas.width) return logStatus('No hay imagen para guardar');

    try {
        enable(btnSave, false);
        logStatus('Guardando...');

        const blob = await canvasToBlobPng(canvas);

        const fd = new FormData();
        fd.append('image', blob, 'scan.png');
        fd.append('plate', lastPlate);
        fd.append('digits', lastDigits);
        fd.append('confidence', String(lastConfidence));

        const r = await fetch('/api/plate', { method: 'POST', body: fd });
        if (!r.ok) {
            const txt = await r.text().catch(() => '');
            throw new Error(`No se pudo guardar: ${r.status} ${txt}`);
        }

        const saved = await r.json();
        logStatus(`Guardado ✅ (Id ${saved.id})`);
        if (saved?.image) resultEl.textContent += `\n\nGuardado en: ${saved.image}`;
    } catch (e) {
        console.error(e);
        logStatus('Error al guardar: ' + (e?.message || e));
        enable(btnSave, true);
    }
});

/* =========================
   PREPROCESAMIENTO
========================= */
function preprocess(img) {
    const w = img.width, h = img.height;
    const src = img.data;
    const dst = new Uint8ClampedArray(src.length);

    // Gris
    for (let i = 0; i < src.length; i += 4) {
        const y = 0.299 * src[i] + 0.587 * src[i + 1] + 0.114 * src[i + 2];
        dst[i] = dst[i + 1] = dst[i + 2] = y;
        dst[i + 3] = 255;
    }

    // Recorte vertical (60% central)
    const y0 = Math.floor(h * 0.2);
    const y1 = Math.floor(h * 0.8);
    const cropped = new Uint8ClampedArray((y1 - y0) * w * 4);
    for (let y = y0; y < y1; y++) {
        for (let x = 0; x < w; x++) {
            const i = (y * w + x) * 4;
            const j = ((y - y0) * w + x) * 4;
            cropped[j] = dst[i];
            cropped[j + 1] = dst[i + 1];
            cropped[j + 2] = dst[i + 2];
            cropped[j + 3] = 255;
        }
    }

    // Binarización adaptativa (umbral 0.85 más fuerte)
    const img2 = new ImageData(cropped, w, y1 - y0);
    const data = img2.data;
    const radius = 10;
    for (let y = radius; y < img2.height - radius; y++) {
        for (let x = radius; x < w - radius; x++) {
            let sum = 0, count = 0;
            for (let dy = -radius; dy <= radius; dy++)
                for (let dx = -radius; dx <= radius; dx++) {
                    sum += data[((y + dy) * w + (x + dx)) * 4];
                    count++;
                }
            const avg = sum / count;
            const i = (y * w + x) * 4;
            const v = data[i] < avg * 0.85 ? 0 : 255;
            data[i] = data[i + 1] = data[i + 2] = v;
        }
    }
    return img2;
}

/* ============ NORMALIZACIÓN ============ */
function normalizeText(txt) {
    return txt
        .replace(/[^A-Z0-9\- ]/g, '')
        .replace(/\s+/g, '')
        .replace(/S/g, '5')
        .replace(/O/g, '0')
        .replace(/I/g, '1')
        .replace(/Z/g, '2')
        .replace(/B/g, '8')
        .replace(/G/g, '6')
        .replace(/Q/g, '0');
}

/* ============ DETECCIÓN DE PLACA ============ */
function extractPlate(text) {
    const pats = [
        /\b([A-Z]{3})[- ]?(\d{3,4})\b/,   // ABC1234
        /\b(\d{3,4})[- ]?([A-Z]{3})\b/,   // 1234ABC
        /\b([A-Z]{2,4})[- ]?(\d{3,4})\b/, // BOL1825
        /\b(\d{3,4})[- ]?([A-Z]{2,4})\b/, // 1825BOL
    ];
    for (const pat of pats) {
        const m = text.match(pat);
        if (m) {
            const plate = m[0].replace(/[- ]/g, '');
            const digits = plate.match(/\d{3,4}/)?.[0] ?? '';
            return { plate, digits };
        }
    }
    const md = text.match(/\d{4}/);
    return { plate: '', digits: md ? md[0] : '' };
}

// Quita duplicados
function unique(arr) {
    const out = [];
    const seen = new Set();
    for (const x of arr) {
        if (!seen.has(x)) { seen.add(x); out.push(x); }
    }
    return out;
}
