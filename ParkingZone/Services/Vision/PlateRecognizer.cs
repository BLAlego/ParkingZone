using System.Text.RegularExpressions;
using OpenCvSharp;
using Tesseract;
using CvRect = OpenCvSharp.Rect;

namespace ParkingZone.Services.Vision
{
    public class PlateRecognizer
    {
        public (string plate, string digits, float confidence, CvRect? rect) Process(byte[] imageBytes)
        {
            using var mat = Cv2.ImDecode(imageBytes, ImreadModes.Color);
            if (mat.Empty()) return ("", "", 0f, null);

            // --- 1) Preprocesado base ---
            using var gray = new Mat();
            Cv2.CvtColor(mat, gray, ColorConversionCodes.BGR2GRAY);
            Cv2.BilateralFilter(gray, gray, 9, 75, 75);
            Cv2.EqualizeHist(gray, gray);

            using var edged = new Mat();
            Cv2.Canny(gray, edged, 80, 200);

            // --- 2) Contornos tipo placa (rectángulo con ratio 2–6) ---
            Cv2.FindContours(edged, out Point[][] contours, out _, RetrievalModes.List, ContourApproximationModes.ApproxSimple);

            CvRect? bestRect = null;
            double bestArea = 0;
            foreach (var c in contours)
            {
                var peri = Cv2.ArcLength(c, true);
                var approx = Cv2.ApproxPolyDP(c, 0.02 * peri, true);
                if (approx.Length != 4) continue;

                var rect = Cv2.BoundingRect(approx);
                if (rect.Width < 120 || rect.Height < 30) continue;

                var ratio = (float)rect.Width / rect.Height;
                var area = rect.Width * rect.Height;
                if (ratio >= 2 && ratio <= 6 && area > bestArea)
                {
                    bestArea = area;
                    bestRect = rect;
                }
            }

            if (bestRect == null) return ("", "", 0f, null);

            // --- 3) ROI + upscaling + binarización robusta ---
            using var roi = new Mat(mat, bestRect.Value);
            using var roiGray = new Mat();
            Cv2.CvtColor(roi, roiGray, ColorConversionCodes.BGR2GRAY);

            // Escalar para ayudar a OCR
            var scale = 2.0;
            using var big = new Mat();
            Cv2.Resize(roiGray, big, new Size(), scale, scale, InterpolationFlags.Cubic);

            // Mejora local de contraste y umbral adaptativo
            using var clahe = Cv2.CreateCLAHE(2.0, new Size(8, 8));
            using var eq = new Mat();
            clahe.Apply(big, eq);

            using var thr = new Mat();
            Cv2.AdaptiveThreshold(eq, thr, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 11, 2);

            // Pequeño cierre morfológico para unir caracteres delgados
            using var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(3, 3));
            Cv2.MorphologyEx(thr, thr, MorphTypes.Close, kernel, iterations: 1);

            // --- 4) OCR (Tesseract) ---
            string raw;
            try
            {
                using var engine = new TesseractEngine("./tessdata", "eng", EngineMode.Default);
                engine.SetVariable("tessedit_char_whitelist", "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789- ");
                // Sin PixConverter: codificamos a PNG en memoria
                Cv2.ImEncode(".png", thr, out var pngBytes);
                using var pix = Pix.LoadFromMemory(pngBytes);
                using var page = engine.Process(pix, PageSegMode.SingleLine);
                raw = page.GetText();
            }
            catch
            {
                return ("", "", 0f, bestRect);
            }

            // --- 5) Normalización y validación de formato boliviano ---
            // Dejar solo letras, dígitos, guion y espacio para matchear variantes
            var norm = System.Text.RegularExpressions.Regex.Replace(raw.ToUpper(), @"[^A-Z0-9\- ]", "");

            // Patrones más comunes en BO: ABC1234, ABC-1234, ABC 1234, 1234ABC, 1234-ABC, 1234 ABC
            var patterns = new[]
            {
        @"\b([A-Z]{3})[- ]?(\d{4})\b",
        @"\b(\d{4})[- ]?([A-Z]{3})\b"
    };

            string plate = "";
            string digits = "";

            foreach (var pat in patterns)
            {
                var m = System.Text.RegularExpressions.Regex.Match(norm, pat);
                if (m.Success)
                {
                    if (char.IsLetter(m.Groups[1].Value[0]))
                    {
                        // Letras primero: ABC + 1234
                        plate = m.Groups[1].Value + m.Groups[2].Value; // ABC1234 sin separadores
                        digits = m.Groups[2].Value;                    // 1234
                    }
                    else
                    {
                        // Dígitos primero: 1234 + ABC
                        plate = m.Groups[1].Value + m.Groups[2].Value; // 1234ABC
                        digits = m.Groups[1].Value;                    // 1234
                    }
                    break;
                }
            }

            // Si no matcheó exactamente, como fallback: tomar 4 dígitos consecutivos
            if (string.IsNullOrEmpty(digits))
            {
                var md = System.Text.RegularExpressions.Regex.Match(norm, @"\b(\d{4})\b");
                if (md.Success) digits = md.Groups[1].Value;
            }

            // Confianza sencilla: si hubo match de patrón oficial, alta; si no, baja
            float conf = !string.IsNullOrEmpty(plate) ? 0.9f : (!string.IsNullOrEmpty(digits) ? 0.6f : 0f);

            return (plate, digits, conf, bestRect);
        }

    }
}
