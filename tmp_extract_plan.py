import json, re
from pathlib import Path

pdf_path = Path(r"C:\Users\sanja\Downloads\1737799035113543_50a76f96-15b0-4ef0-ade7-687936951766.pdf")
out_path = Path(r"C:\Users\sanja\BabyDiplomaApp\extracted-feeding-plan.json")

reader = None
for lib in ("pypdf", "PyPDF2"):
    try:
        if lib == "pypdf":
            from pypdf import PdfReader
        else:
            from PyPDF2 import PdfReader
        reader = PdfReader(str(pdf_path))
        break
    except Exception:
        continue

if reader is None:
    raise SystemExit("Neither pypdf nor PyPDF2 is available")

pages = []
for i, p in enumerate(reader.pages, 1):
    t = p.extract_text() or ""
    pages.append({"page": i, "text": t.strip()})

text = "\n\n".join(p["text"] for p in pages)
lines = [ln.strip() for ln in text.splitlines() if ln.strip()]

day_entries = []
i = 0
while i < len(lines):
    m = re.match(r"^(\d{1,2})\.\s*(.+)$", lines[i])
    if m:
        day = int(m.group(1))
        meals = [m.group(2).strip()]
        j = i + 1
        while j < len(lines):
            if re.match(r"^\d{1,2}\.\s+", lines[j]):
                break
            if lines[j].startswith("-- ") and "of" in lines[j]:
                j += 1
                continue
            meals.append(lines[j])
            j += 1
        day_entries.append({"day": day, "items": meals})
        i = j
    else:
        i += 1

day_entries = [d for d in day_entries if 1 <= d["day"] <= 56]

for d in day_entries:
    day = d["day"]
    if day <= 19:
        stage = "6+ months"
        meals_per_day = 1
    elif day <= 37:
        stage = "8+ to 9+ months"
        meals_per_day = 2
    else:
        stage = "10+ to 11+ months"
        meals_per_day = 3
    d["stage"] = stage
    d["recommended_meals_per_day"] = meals_per_day

payload = {
    "source_file": str(pdf_path),
    "title": "Plan uvođenja nemliječne ishrane za bebu",
    "pages": len(pages),
    "extracted_at": "2026-04-27",
    "notes": {
        "edmu": "ekstra djevičansko maslinovo ulje",
        "general_guidance": [
            "Namirnice uvoditi 3 dana uz postupno povećanje količine.",
            "Do 1. godine ne dodavati so, šećer, med ni kravlje mlijeko.",
            "Za bebe s alergijama i specifičnim stanjima konsultovati pedijatra."
        ]
    },
    "days": day_entries,
    "raw_text_preview": text[:5000]
}

out_path.write_text(json.dumps(payload, ensure_ascii=False, indent=2), encoding="utf-8")
print(f"Wrote {out_path}")
print(f"Parsed day entries: {len(day_entries)}")
