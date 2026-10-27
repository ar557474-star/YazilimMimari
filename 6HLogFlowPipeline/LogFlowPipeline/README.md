# LogFlow

Log dosyalarını **Source → Stage(ler) → Sink** akışıyla işleyen, yapılandırma
odaklı bir pipeline uygulaması. Pipeline'ın şekli koda değil, bir YAML dosyasına
yazılır. Yeni bir pipeline için **yeniden derleme gerekmez**.

## Gereksinimler
- .NET 10 SDK

## Derleme ve yayınlama
```
dotnet build
dotnet publish -c Release -o ../publish
```
Yayınlama, `publish/` klasöründe `logflow.exe` üretir.

## Çalıştırma
Komutlar `LogFlowPipeline` proje klasöründen çalıştırılmalıdır (config içindeki
yollar bulunulan klasöre göredir).

```
logflow --config configs/error-report.yaml
```
Geliştirme sırasında:
```
dotnet run -- --config configs/error-report.yaml
```

## Hazır yapılandırmalar

| Dosya | Ne yapar | Çıktı |
|---|---|---|
| `configs/error-report.yaml` | status >= 400 olan istekleri listeler | `out/errors.txt` |
| `configs/api-traffic.yaml` | `/api` ile başlayan istekleri çıkarır | `out/api-traffic.txt` |
| `configs/bandwidth-by-client.yaml` | istemci IP'si ve bayt sayısını çıkarır | `out/bandwidth.txt` |

Aynı çalıştırılabilir dosya, üç farklı config ile üç farklı çıktı üretir.

## Hata mesajı örnekleri

Yapılandırma hataları **hiçbir veri işlenmeden önce** yakalanır, açık bir mesaj
yazdırılır ve program `1` çıkış koduyla biter (fail fast, fail loudly).
Hata durumunda çıktı dosyaları oluşturulmaz/değiştirilmez.

**1. Bilinmeyen aşama türü** (`type: filtr`)

Yapilandirma hatasi: stage #2: bilinmeyen tür 'filer'. Kullanilabilir türler: filter, parse, project

**2. Eksik parametre** (filter'da `op` yok)
Yapilandirma hatasi: stage #2 ('filter'): zorunlu parametre 'op' eksik.

**3. Yanlış türde parametre** (`value: abc`)

Yapilandirma hatasi: stage #2 ('filter'): 'value' parametresi tamsayi olmali, 'abc' bulundu.
Yapilandirma hatasi: pipeline.stages bos; en az bir 'parse' asamasi gerekli.
Yapilandirma hatasi: sink ('file'): zorunlu parametre 'path' eksik.


**4. Boş işlem hattı** (`stages: []`)

Yapilandirma hatasi: pipeline.stages bos; en az bir 'parse' asamasi gerekli.


**5. Olmayan kaynak dosyası** (` `)
Yapilandirma hatasi: sink ('file'): zorunlu parametre 'path' eksik.

## Testler
```
dotnet test
```
Yükleyici testleri: geçerli config, bilinmeyen aşama, eksik parametre,
yanlış türde parametre, boş pipeline.

## Daha fazla bilgi
Yapılandırma şeması ve tasarım için `ARCHITECTURE.md` dosyasına bakın.