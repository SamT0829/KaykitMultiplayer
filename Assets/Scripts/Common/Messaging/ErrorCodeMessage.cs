public enum ErrorCode
{
    Success,

    // account
    AccountUnableRetrieveData = 100,
    AccountNotExist,
    AccountOauthExpired,
    AccountWaitingOthers,
    AccountKickByDuplicatedLogin,

    // lobby 
    LobbyDBUnableRetrieveData = 200,

    // lobby / lobbyPlayer
    PlayerUnableRetrieveData,
    PlayerKickByDuplicatedLogin,

    PlayerStatusIsInRoom,
    PlayerStatusIsnotInRoom,


    // lobby / LobbyRoom
    LobbyRoomCreateFailed,
    LobbyRoomNameHasAlreadyBeenUsed,
    LobbyRoomHasInGame,
    LobbyRoomPlayerHasFull,
    LobbyRoomPasswordIsWrong,
    LobbyRoomDoesNotExist,

    // Game / GameRoom
    GameRoomIdHasAlreadyExist,
    GameRoomHasNotExist,
    GameBuilderCantFound,
    GameHasNotReady,

    // finance
    FirmUnableRetrieveCashData,

    // server
    ServerNotReady = 901,
}