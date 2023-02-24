namespace XProtocol
{
    public enum XPacketType
    {
        Unknown,
        Handshake,
        SuccessfulRegistration,
        StartGame,
        ThrowDice,
        ResultOfThrow,
        StepResult,
        ReceiveSource,
        MoveBandit,
        ReceivedKnightCard,
        ReceiveResource,
        PlayerTrade,
        Build,
        EndStep,
        Pause,
        PauseEnded,
        Winner,
        EndGame
    }
}
