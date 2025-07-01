using Contracts.Application.Common.Interfaces.GenIdLong;

namespace Contracts.Infrastructure.Services.GenIdLong
{
    public class SnowflakeIdGenerator : IIdGenerator
    {
        private readonly object _lock = new();
        private readonly long _workerId;
        private readonly long _maxWorkerId = -1L ^ (-1L << 6); // 6 bits => max 63
        private readonly long _sequenceMask = -1L ^ (-1L << 6); // 6 bits => max 63

        private readonly int _workerIdShift = 6; // Shift 6 bits
        private readonly int _timestampLeftShift = 6 + 6; // Shift 12 bits

        private const long Twepoch = 1288834974657L;

        private long _lastTimestamp = -1L;
        private long _sequence = 0L;

        public SnowflakeIdGenerator(long workerId)
        {
            if (workerId > _maxWorkerId || workerId < 0)
                throw new ArgumentException($"Worker Id must be between 0 and {_maxWorkerId}");

            _workerId = workerId;
        }

        public long GenerateId()
        {
            lock (_lock)
            {
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
        }

        private long WaitUntilNextMillis(long lastTimestamp)
        {
            var timestamp = GetCurrentTimestamp();
            while (timestamp <= lastTimestamp)
            {
                timestamp = GetCurrentTimestamp();
            }
            return timestamp;
        }

        private long GetCurrentTimestamp()
        {
            return DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
    }
}
