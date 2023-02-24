namespace GameLogic
{

    public class Player
    {
        public int Id => _id;
        public string Name { get; }

        public int KnightCount { get; private set; }

        public int MovesCount { get; private set; }

        public int WinPoints { get; private set; }

        private readonly int _id;

        public Player(int id, string name)
        {
            _id = id;
            Name = name;
        }

        public void MakeMove() => MovesCount++;

        public void ReceiveWinPoints(int count) => WinPoints += count;
        public void ReceiveKnightCard() => KnightCount++;
    }
}
