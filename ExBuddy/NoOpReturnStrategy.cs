#pragma warning disable 1998

namespace ExBuddy
{
    using Clio.Utilities;
    using ExBuddy.Interfaces;
    using ExBuddy.Logging;
    using System.Threading.Tasks;

    public class NoOpReturnStrategy : IReturnStrategy
    {
        #region IAetheryteId Members

        public uint AetheryteId { get; set; }

        #endregion IAetheryteId Members

        #region IZoneId Members

        public ushort ZoneId { get; set; }

        #endregion IZoneId Members

        public override string ToString()
        {
            return "NoOp: Can't figure out what we are supposed to do, hopefully someone else can help us.";
        }

        #region IReturnStrategy Members

        public Vector3 InitialLocation { get; set; }

        public async Task<bool> ReturnToLocation()
        {
            return true;
        }

        public async Task<bool> ReturnToZone()
        {
            Logger.Instance.Warn("Could not find a return strategy for ZoneId: {0}", ZoneId);
            return true;
        }

        #endregion IReturnStrategy Members
    }
}