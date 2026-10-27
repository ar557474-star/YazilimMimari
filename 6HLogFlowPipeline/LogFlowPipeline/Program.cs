/*using System;

class Program
{
    static void Main(string[] args)
    {
        // Komut satırından dosya adı gelirse onu al, gelmezse varsayılan dosyayı kullan
        string filePath = args.Length > 0 ? args[0] : "data/dataFile.log";

        // 1. Bileşenleri oluştur
        var source = new FileLineSource(filePath);
        var sink = new ConsoleSink();

        // 2. Pipeline'ı monte et
        var pipeline = new Pipeline(source, sink);

        // 3. Çalıştır
        pipeline.Run();
    }
}*/
/*
namespace LogFlowPipeline;
using System;

class Program
{
    static void Main(string[] args)
    {
        //string filePath = args.Length > 0 ? args[0] : "data/access-small.log";
        string filePath = args.Length > 0 ? args[0] : "data/dataFile.log";

        // 1. Bileşenleri Oluştur
        var source = new FileLineSource(filePath);
        var parser = new ParserStage(); // <-- Yeni Ara Aşama
        var sink = new ConsoleSink();   // <-- LogRecord Kabul Eden Sink

        // 2. Pipeline'ı Kur (Source -> Parser -> Sink)
        var pipeline = new Pipeline(source, parser, sink);

        // 3. Çalıştır
        pipeline.Run();

        // 4. Hocanın İstediği Hatalı Satır Özetini Yazdır
        Console.WriteLine($"\n--- İŞLEM TAMAMLANDI ---");
        Console.WriteLine($"Toplam Hatalı/Atlanan Satır Sayısı: {parser.ErrorCount}");
    }
}*/






//namespace LogFlowPipeline;

//using System;

//class Program
//{
//    static void Main(string[] args)
//    {
//        string filePath = "data/dataFile.log";
//        string outputPath = "data/output.log";

//        // Akıcı Builder (Fluent Builder) ile pipeline'ı kuruyoruz:
//        /*var pipeline = PipelineBuilder.From(new FileLineSource(filePath))
//            .Via(new ParserStage())
//            .Via(new PredicateFilterStage(r => r.Status >= 200)) // Örnek filtre: 200 ve üzeri statusler
//            .Via(new ProjectionStage("timestamp", "clientIp", "status", "path")) // Alan daraltma
//            .To(new FileSink(outputPath));*/

//        /*
//        var pipeline = PipelineBuilder.From(new FileLineSource(filePath))
//        .Via(new ParserStage())
//    // 1. Filtre: Sadece hatalı statüler (400+)
//        .Via(new PredicateFilterStage(r => r.Status >= 200))
//    // 2. Filtre: Sadece /api ile başlayan yollar
//        .Via(new PredicateFilterStage(r => r.Path != null && r.Path.StartsWith("/api")))
//    // 3. Filtre: Sadece belirli bir IP adresi
//        .Via(new PredicateFilterStage(r => r.ClientIp == "192.168.1.10"))
//        .Via(new ProjectionStage("timestamp", "clientIp", "status", "path"))
//        .To(new FileSink(outputPath));
//        */
//        var pipeline = PipelineBuilder.From(new FileLineSource(filePath))
//    .Via(new CustomLogParserStage())   // <-- burası
//    .Via(new PredicateFilterStage(r => r.Status >= 200))
//    .Via(new PredicateFilterStage(r => r.Path != null && r.Path.StartsWith("/api")))
//    .Via(new PredicateFilterStage(r => r.ClientIp == "192.168.1.10"))
//    .Via(new ProjectionStage("timestamp", "clientIp", "status", "path"))
//    .To(new FileSink(outputPath));


//        // Çalıştır
//        pipeline.Run();

//        Console.WriteLine($"\n--- İŞLEM TAMAMLANDI ---");
//        Console.WriteLine($"Sonuçlar {outputPath} dosyasına yazdırıldı.");
//    }
//}


/*
namespace LogFlowPipeline;

using System;

class Program
{
    static void Main(string[] args)
    {
        string filePath = "data/dataFile.log";
        string outputPath = "data/output.log";

        // ═══════════════ KULLANICIYA SOR ═══════════════
        Console.WriteLine("=== PIPELINE FILTRE SECIMI ===");
        Console.WriteLine("Uygulamak istediginiz filtreleri secin (e/h):\n");

        // 1. Filtre: Status esigi
        bool useStatusFilter = SorEvetHayir("1. Status filtresi uygulansin mi?");
        int statusEsigi = 200;
        if (useStatusFilter)
        {
            Console.Write("   Esik degeri girin (ornek: 200, 400): ");
            int.TryParse(Console.ReadLine(), out statusEsigi);
        }

        // 2. Filtre: Path on eki
        bool usePathFilter = SorEvetHayir("2. Path filtresi uygulansin mi? (/api ile baslayanlar)");
        string pathOnEki = "/api";
        if (usePathFilter)
        {
            Console.Write("   On ek girin (ornek: /api, /images): ");
            string? girdi = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(girdi)) pathOnEki = girdi.Trim();
        }

        // 3. Filtre: IP adresi
        bool useIpFilter = SorEvetHayir("3. IP filtresi uygulansin mi?");
        string ipAdresi = "192.168.1.10";
        if (useIpFilter)
        {
            Console.Write("   IP adresi girin (ornek: 192.168.1.10): ");
            string? girdi = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(girdi)) ipAdresi = girdi.Trim();
        }
        Console.WriteLine("================================\n");

        // ═══════════════ PIPELINE'I KUR ═══════════════
        var builder = PipelineBuilder.From(new FileLineSource(filePath))
            .Via(new ParserStage());

        // Kullanicinin sectigi filtreleri SADECE o zaman ekle
        if (useStatusFilter)
            builder = builder.Via(new PredicateFilterStage(r => r.Status >= statusEsigi));

        if (usePathFilter)
            builder = builder.Via(new PredicateFilterStage(r => r.Path != null && r.Path.StartsWith(pathOnEki)));

        if (useIpFilter)
            builder = builder.Via(new PredicateFilterStage(r => r.ClientIp == ipAdresi));

        var pipeline = builder
            .Via(new ProjectionStage("timestamp", "clientIp", "status", "path"))
            .To(new FileSink(outputPath));

        // ═══════════════ CALISTIR ═══════════════
        Console.WriteLine("Secilen filtreler:");
        Console.WriteLine(useStatusFilter ? $"  [AKTIF]  Status >= {statusEsigi}" : "  [PASIF]  Status filtresi");
        Console.WriteLine(usePathFilter ? $"  [AKTIF]  Path '{pathOnEki}' ile baslayan" : "  [PASIF]  Path filtresi");
        Console.WriteLine(useIpFilter ? $"  [AKTIF]  IP == {ipAdresi}" : "  [PASIF]  IP filtresi");
        Console.WriteLine("\nPipeline calistiriliyor...\n");

        pipeline.Run();

        Console.WriteLine("\n--- ISLEM TAMAMLANDI ---");
        Console.WriteLine($"Sonuclar {outputPath} dosyasina yazdirildi.");
    }

    /// <summary>Kullaniciya e/h sorusu sorar, Enter/e -> true, h -> false</summary>
    static bool SorEvetHayir(string soru)
    {
        Console.Write($"{soru} (e/h): ");
        string? cevap = Console.ReadLine();
        return string.IsNullOrWhiteSpace(cevap) || cevap.Trim().ToLower() == "e";
    }
}*/

/*
namespace LogFlowPipeline;

class Program
{
    static int Main(string[] args)
    {
        string? configPath = null;
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == "--config") configPath = args[i + 1];

        if (configPath == null)
        {
            Console.Error.WriteLine("Kullanım: logflow --config <yaml dosyası>");
            return 2;
        }

        try
        {
            var registry = ComponentRegistry.Discover(typeof(Program).Assembly);
            var pipeline = new PipelineLoader(registry).LoadFromFile(configPath);

            Console.WriteLine($"Pipeline '{pipeline.Name}' çalışıyor...");
            pipeline.Run();
            Console.WriteLine("Tamamlandı.");
            return 0;
        }
        catch (ConfigException ex)
        {
            Console.Error.WriteLine($"Yapılandırma hatası: {ex.Message}");
            return 1;
        }
    }
}
*/

namespace LogFlowPipeline;

class Program
{
    static int Main(string[] args)
    {
        string? configPath = null;
        for (int i = 0; i < args.Length - 1; i++)
            if (args[i] == "--config") configPath = args[i + 1];

        if (configPath == null)
        {
            Console.Error.WriteLine("Kullanım: logflow --config <yaml dosyası>");
            return 2;
        }

        Pipeline? pipeline = null;
        try
        {
            var registry = ComponentRegistry.Discover(typeof(Program).Assembly);
            pipeline = new PipelineLoader(registry).LoadFromFile(configPath);

            Console.WriteLine($"Pipeline '{pipeline.Name}' çalışıyor...");
            var summary = pipeline.Run();
            summary.Print(Console.Out);

            return summary.IsBalanced ? 0 : 4;
        }
        catch (ConfigException ex)          // ÖNCE alt sınıf
        {
            Console.Error.WriteLine($"Yapılandırma hatası: {ex.Message}");
            return 1;
        }
        catch (PipelineException ex)
        {
            Console.Error.WriteLine($"Pipeline durdu: {ex.Message}");
            pipeline?.LastSummary?.Print(Console.Error);   // kısmi özet
            return 3;
        }
    }
}





/* ..\publish\logflow.exe --config configs/error-report.yaml
..\publish\logflow.exe --config configs/api-traffic.yaml
..\publish\logflow.exe --config configs/bandwidth-by-client.yaml 

cd LogFlowPipeline
dir ../publish
dotnet run -- --config configs/chaos-test.yaml

 
 */