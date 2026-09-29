public class Pipeline
{
    private readonly ISource<string> _source;
    private readonly ISink<string> _sink;

    public Pipeline(ISource<string> source, ISink<string> sink)
    {
        _source = source;
        _sink = sink;
    }

    public void Run()
    {
        // Source veriyi ürettikçe doğrudan Sink'e aktaran minik köprü (DirectEmitter)
        _source.Produce(new DirectEmitter(_sink));
    }

    private class DirectEmitter : IEmitter<string>
    {
        private readonly ISink<string> _sink;

        public DirectEmitter(ISink<string> sink)
        {
            _sink = sink;
        }

        public void Emit(string item)
        {
            _sink.Consume(item);
        }
    }
}