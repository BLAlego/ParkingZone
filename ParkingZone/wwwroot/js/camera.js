const video = document.getElementById('video');
const canvas = document.getElementById('canvas');
const btnStart = document.getElementById('btnStart');
const btnCapture = document.getElementById('btnCapture');
const btnOCR = document.getElementById('btnOCR');
const statusEl = document.getElementById('status');
const resultEl = document.getElementById('result');

let stream;

function logStatus(msg) {
    statusEl.textContent = msg;
    console.log('[OCR]', msg);
}

// Iniciar cámara
btnStart?.addEventListener('click', async () => {
    try {
        stream = await navigator.mediaDevices.getUserMedia({ video: { facingMode: 'environment' }, audio: false });
        video.srcObject = stream;
        logStatus('Cámara iniciada');
    } catch (err) {
        logStatus('Error al iniciar cámara: ' + err.message);
    }
});

// Capturar frame del video
btnCapture?.addEventListener('click', () => {
    if (!video.videoWidth) return logStatus('Esperando video...');
    const w = Math.min(1280, video.videoWidth);
    const h = Math.round(video.videoHeight * (w / video.videoWidth));
    canvas.width = w;
    canvas.height = h;
    const ctx = canvas.getContext('2d');
    ctx.filter = 'grayscale(100%) contrast(260%) brightness(130%)';
    ctx.drawImage(video, 0, 0, w, h);
    ctx.filter = 'none';
    logStatus('Imagen capturada. Presiona "Analizar placa".');
});

// Analizar imagen
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
            tessedit_pageseg_mode: '7', // single line
            tessedit_char_whitelist: 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789',
            user_defined_dpi: '400'
        });

        const { data } = await worker.recognize(dataUrl);
        await worker.terminate();

        const raw = (data.text || '').toUpperCase().replace(/\s+/g, '');
        const corrected = normalizeText(raw);
        const { plate, digits } = extractPlate(corrected);

        resultEl.textContent =
            `RAW:\n${raw}\nNORMALIZADO:\n${corrected}\n\n` +
            `Placa: ${plate || '(no detectada)'}\nDígitos: ${digits || '(no detectados)'}`;

        logStatus('Listo ✅');
    } catch (err) {
        console.error(err);
        logStatus('Error: ' + err.message);
    }
});

/* === PREPROCESAMIENTO AVANZADO === */
function preprocess(img) {
    const w = img.width, h = img.height;
    const src = img.data;
    const dst = new Uint8ClampedArray(src.length);

    // Convertir a escala de grises
    for (let i = 0; i < src.length; i += 4) {
        const y = 0.299 * src[i] + 0.587 * src[i + 1] + 0.114 * src[i + 2];
        dst[i] = dst[i + 1] = dst[i + 2] = y;
        dst[i + 3] = 255;
    }

    // Recorte vertical: tomamos solo el 60% central (donde suele estar la placa)
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

    // Binarización adaptativa (para pantallas con brillo)
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
            const v = data[i] < avg * 0.9 ? 0 : 255; // invertido
            data[i] = data[i + 1] = data[i + 2] = v;
        }
    }

    return img2;
}

/* === NORMALIZACIÓN === */
function normalizeText(txt) {
    return txt
        .replace(/[^A-Z0-9]/g, '')
        .replace(/S/g, '5')
        .replace(/O/g, '0')
        .replace(/I/g, '1')
        .replace(/Z/g, '2')
        .replace(/B/g, '8')
        .replace(/G/g, '6')
        .replace(/Q/g, '0')
        .replace(/E{2,}/g, 'E'); // corrige EEEE → E
}

/* === DETECCIÓN DE PLACAS === */
function extractPlate(text) {
    const patterns = [
        /\b([A-Z]{3})(\d{3,4})\b/, // ABC1234
        /\b(\d{3,4})([A-Z]{3})\b/, // 1234ABC
        /\b(\d{4})[A-Z]{3}\b/,     // 1852PHD
        /\b[A-Z]{3}(\d{4})\b/      // PHD1852
    ];
    for (const pat of patterns) {
        const m = text.match(pat);
        if (m) {
            const plate = m[0];
            const digits = plate.match(/\d{3,4}/)?.[0] ?? '';
            return { plate, digits };
        }
    }
    const md = text.match(/\d{4}/);
    return { plate: '', digits: md ? md[0] : '' };
}
