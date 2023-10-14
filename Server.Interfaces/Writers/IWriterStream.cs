namespace Server.Interfaces.Writers
{
    public interface IWriterStream<T>
    {
        public void Write(T value, Stream stream);

        public Task WriteAsync(T value, Stream stream);
    }
}
