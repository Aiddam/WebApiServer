namespace Server.Interfaces.Parsers
{
    public interface IParser
    {
        public Request Parse(string header);
    }
}
