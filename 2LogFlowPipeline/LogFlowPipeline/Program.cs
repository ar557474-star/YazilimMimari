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
}