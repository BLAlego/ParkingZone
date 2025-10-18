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
        stream = await navigator.mediaDevices.getUserMedia({
            video: { facingMode: 'environment' },
            audio: false
        });
        video.srcObject = stream;
        logStatus('Cámara iniciada');
    } catch (e) {
        logStatus('No se pudo iniciar cámara: ' + e.message);
    }
});

// Capturar frame al canvas
btnCapture?.addEventListener('click', () => {
    if (!video.videoWidth) { logStatus('Espera a que cargue el video'); return; }
    // Ajusta resolución objetivo (más píxeles = mejor OCR, pero más lento)
    const targetW = Math.min(1280, video.videoWidth); // usa hasta 1280
    const scale = targetW / video.videoWidth;
    const targetH = Math.round(video.videoHeight * scale);

    canvas.width = targetW;
    canvas.height = targetH;

    const ctx = canvas.getContext('2d');
    // Preprocesado simple con filtros CSS de canvas
    ctx.filter = 'grayscale(100%) contrast(160%) brightness(110%)';
    ctx.drawImage(video, 0, 0, targetW, targetH);
    // Limpia filtros para usos posteriores
    ctx.filter = 'none';

    logStatus('Imagen capturada');
});

// OCR local con Tesseract.js
btnOCR?.addEventListener('click', async () => {
    if (!canvas.width) { logStatus('Captura primero.'); return; }

    try {
        logStatus('Analizando...');
        resultEl.textContent = '';

        // Exporta la imagen del canvas como DataURL (PNG recomendado para umbral)
        const dataUrl = canvas.toDataURL('image/png');

        // Worker de Tesseract
        const worker = await Tesseract.createWorker('eng', 1, {
            logger: m => console.log(m) // progreso en consola
        });

        // Parámetros que suelen ayudar en placas
        await worker.setParameters({
            tessedit_char_whitelist: 'ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789- ',
            preserve_interword_spaces: '1',
            user_defined_dpi: '300'
            // psm: 7  // (single line) -> si usas v4 addParam; en v5 usamos setParameters
        });

        const { data } = await worker.recognize(dataUrl);
        await worker.terminate();

        const raw = (data && data.text ? data.text : '').toString();
        const cleanAll = raw.toUpperCase().replace(/[^A-Z0-9\- ]/g, ''); // deja letras, dígitos, -, espacio

        // Patrones más comunes BO: ABC1234, ABC-1234, ABC 1234 | 1234ABC, 1234-ABC, 1234 ABC
        const patterns = [
            /\b([A-Z]{3})[- ]?(\d{4})\b/, // letras primero
            /\b(\d{4})[- ]?([A-Z]{3})\b/  // dígitos primero
        ];

        let plate = '';
        let digits = '';
        for (const pat of patterns) {
            const m = cleanAll.match(pat);
            if (m) {
                if (/[A-Z]/.test(m[1][0])) {
                    // LLL + NNNN
                    plate = `${m[1]}${m[2]}`;  // ABC1234
                    digits = m[2];             // 1234
                } else {
                    // NNNN + LLL
                    plate = `${m[1]}${m[2]}`;  // 1234ABC
                    digits = m[1];             // 1234
                }
                break;
            }
        }

        // Fallback: si no matchea, intenta 4 dígitos seguidos
        if (!digits) {
            const md = cleanAll.match(/\b(\d{4})\b/);
            if (md) digits = md[1];
        }

        // Mostrar con depuración para ver qué está leyendo
        resultEl.textContent =
            `RAW:\n${raw}\n\nNORMALIZADO:\n${cleanAll}\n\n` +
            `Placa: ${plate || '(no detectada)'}\n` +
            `Dígitos: ${digits || '(no detectados)'}`;

        logStatus('Listo ✅');
    } catch (err) {
        console.error(err);
        logStatus('Error: ' + (err?.message || err));
    }
});
