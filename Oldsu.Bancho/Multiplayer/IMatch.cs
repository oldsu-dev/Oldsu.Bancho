namespace Oldsu.Bancho.Multiplayer
{
    public interface IMatch
    {
        int? Join(int clientId, string? password);
        bool Leave(int clientId);
    }
}