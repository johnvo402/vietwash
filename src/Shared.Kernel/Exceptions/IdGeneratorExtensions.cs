using System.Net;

namespace Shared.Kernel.Exceptions
{
    public static class IdGenerator
    {
        private static SpinLock _spinLock = new();
        private static readonly long _workerId;
        private static readonly long _maxWorkerId = -1L ^ (-1L << 6); // Max 63
        private static readonly long _sequenceMask = -1L ^ (-1L << 6); // Max 63

        private static readonly int _workerIdShift = 6; // Shift 6 bits
        private static readonly int _timestampLeftShift = 6 + 6; // Shift 12 bits

        private const long Twepoch = 1288834974657L;

        private static long _lastTimestamp = -1L;
        private static long _sequence = 0L;

        static IdGenerator()
        {
            if (
                long.TryParse(Environment.GetEnvironmentVariable("MachineId"), out var id)
                && id >= 0
                && id <= _maxWorkerId
            )
            {
                _workerId = id;
            }
            else
            {
                var hostname = Dns.GetHostName();
                _workerId = hostname.GetHashCode() & 0x3F; // 6 bits
            }
        }

        public static long NewId()
        {
            bool lockTaken = false;

            try
            {
                _spinLock.Enter(ref lockTaken);

                var timestamp = GetCurrentTimestamp();

                if (timestamp < _lastTimestamp)
                    throw new Exception(
                        $"Clock moved backwards. Refusing to generate id for {_lastTimestamp - timestamp} milliseconds"
                    );

                if (_lastTimestamp == timestamp)
                {
                    _sequence = (_sequence + 1) & _sequenceMask;
                    if (_sequence == 0)
                        timestamp = WaitUntilNextMillis(_lastTimestamp);
                }
                else
                {
                    _sequence = 0;
                }

                _lastTimestamp = timestamp;

                return ((timestamp - Twepoch) << _timestampLeftShift)
                    | (_workerId << _workerIdShift)
                    | _sequence;
            }
            finally
            {
                if (lockTaken)
                    _spinLock.Exit();
            }
        }

        private static long WaitUntilNextMillis(long lastTimestamp)
        {
            var timestamp = GetCurrentTimestamp();
            while (timestamp <= lastTimestamp)
            {
                Thread.SpinWait(1);
                timestamp = GetCurrentTimestamp();
            }
            return timestamp;
        }

        private static long GetCurrentTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
