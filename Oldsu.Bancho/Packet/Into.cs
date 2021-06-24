namespace Oldsu.Bancho.Packet
{
    /// <summary>
    ///     Converts packet into T type. eg. IB394APAcketOut
    /// </summary>
    public interface Into<out T>
    {
        public T Into();
    }
}