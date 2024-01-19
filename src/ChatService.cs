
public class ChatService
{
    private readonly List<(string clientId, DateTime timeReceived, string message)> _messages = new();
    private int _clientId = 0;

    internal int Register()
    {
        _clientId++;
        return _clientId;
    }

    // get most recent messages
    internal IEnumerable<(string clientId, DateTime timeReceived, string message)> GetLatestMessages(DateTime lastMessageCheck)
    {
        return _messages.Where(x => x.timeReceived >= lastMessageCheck);
    }

    // add message
    internal void AddMessage(string clientId, string message)
    {
        _messages.Add((clientId, DateTime.Now, message));
    }
}