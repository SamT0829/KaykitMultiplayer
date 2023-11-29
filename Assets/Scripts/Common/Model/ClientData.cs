public class ClientData
{
    private static ClientData instance = new ClientData();
    public static ClientData Instance { get { return instance; } }

    public LobbyPlayerInfo LobbyPlayerInfo = new LobbyPlayerInfo();
    public LobbyGameInfo LobbyGameInfo = new LobbyGameInfo();

    public int SessionId { get; set; }
    public string LobbyServerAddreas { get; set; }
    public string GameServerAddreas { get; set; }

}
