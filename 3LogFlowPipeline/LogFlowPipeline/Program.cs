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
namespace LogFlowPipeline;

using System;

class Program
{
    static void Main(string[] args)
    {
        string filePath = "data/dataFile.log";
        string outputPath = "data/output.log";

        // Akıcı Builder (Fluent Builder) ile pipeline'ı kuruyoruz:
        /*var pipeline = PipelineBuilder.From(new FileLineSource(filePath))
            .Via(new ParserStage())
            .Via(new PredicateFilterStage(r => r.Status >= 200)) // Örnek filtre: 200 ve üzeri statusler
            .Via(new ProjectionStage("timestamp", "clientIp", "status", "path")) // Alan daraltma
            .To(new FileSink(outputPath));*/

        /*
        var pipeline = PipelineBuilder.From(new FileLineSource(filePath))
        .Via(new ParserStage())
    // 1. Filtre: Sadece hatalı statüler (400+)
        .Via(new PredicateFilterStage(r => r.Status >= 200))
    // 2. Filtre: Sadece /api ile başlayan yollar
        .Via(new PredicateFilterStage(r => r.Path != null && r.Path.StartsWith("/api")))
    // 3. Filtre: Sadece belirli bir IP adresi
        .Via(new PredicateFilterStage(r => r.ClientIp == "192.168.1.10"))
        .Via(new ProjectionStage("timestamp", "clientIp", "status", "path"))
        .To(new FileSink(outputPath));
        */
        var pipeline = PipelineBuilder.From(new FileLineSource(filePath))
    .Via(new CustomLogParserStage())   // <-- burası
    .Via(new PredicateFilterStage(r => r.Status >= 200))
    .Via(new PredicateFilterStage(r => r.Path != null && r.Path.StartsWith("/api")))
    .Via(new PredicateFilterStage(r => r.ClientIp == "192.168.1.10"))
    .Via(new ProjectionStage("timestamp", "clientIp", "status", "path"))
    .To(new FileSink(outputPath));


        // Çalıştır
        pipeline.Run();

        Console.WriteLine($"\n--- İŞLEM TAMAMLANDI ---");
        Console.WriteLine($"Sonuçlar {outputPath} dosyasına yazdırıldı.");
    }
}