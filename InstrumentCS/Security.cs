namespace TradingEngineServer.Instrument
{
    public class Security : IEquatable<Security>
    {
        public int Id { get; }
        public string Name { get; }

        public Security(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override bool Equals(object obj)
        {
            return obj is Security security && Equals(security);
        }

        public bool Equals(Security other)
        {
            return Id == other.Id && Name == other.Name;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Name);
        }

        public static bool operator ==(Security left, Security right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Security left, Security right)
        {
            return !(left == right);
        }

    }
}
