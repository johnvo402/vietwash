using System.Collections;
using Contracts.Common.Messages;

namespace AuthSerivce.Tests
{
    public class MessageResultComparer : IEqualityComparer
    {
        public new bool Equals(object? x, object? y)
        {
            if (ReferenceEquals(x, y))
                return true;

            if (x is not MessageResult a || y is not MessageResult b)
                return false;

            return a.Message == b.Message && a.En == b.En && a.Vi == b.Vi;
        }

        public int GetHashCode(object obj)
        {
            if (obj is not MessageResult mr)
                return 0;
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + (mr.Message?.GetHashCode() ?? 0);
                hash = hash * 23 + (mr.En?.GetHashCode() ?? 0);
                hash = hash * 23 + (mr.Vi?.GetHashCode() ?? 0);
                return hash;
            }
        }
    }
}
