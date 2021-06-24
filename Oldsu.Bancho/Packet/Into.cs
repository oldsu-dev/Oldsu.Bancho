namespace Oldsu.Bancho.Packet
{
    /// <summary>
    ///     Converts packet into T type. eg. IB394APAcketOut
    /// </summary>
    public interface Into<T>
    {
        public T Into();
    }
}