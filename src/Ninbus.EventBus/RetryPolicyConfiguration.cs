using System;

namespace Ninbus.EventBus
{
    public class RetryPolicyConfiguration
    {
        internal RetryPolicyConfiguration() { }
        internal int MaxRetryTimes { get; private set; } = 3;
        internal bool ForeverRetry { get; private set; } = false;
        internal TimeSpan RetryInterval { get; private set; } = TimeSpan.FromSeconds(5);
        internal Type? ExceptionType { get; private set; }
        internal bool DiscardEvent { get; private set; } = false;

        public RetryPolicyConfiguration SetIntervalTime(TimeSpan interval)
        {
            RetryInterval = interval;
            return this;
        }
        public RetryPolicyConfiguration RetryForTimes(int times)
        {
            MaxRetryTimes = times;
            ForeverRetry = false;
            return this;
        }

        public RetryPolicyConfiguration RetryForever()
        {
            ForeverRetry = true;
            return this;
        }

        public RetryPolicyConfiguration NeverRetry()
        {
            ForeverRetry = false;
            DiscardEvent = true;
            return this;
        }

        public RetryPolicyConfiguration ShouldDiscard<Ex>() where Ex : Exception
        {
            ForeverRetry = false;
            ExceptionType = typeof(Ex);
            return this;
        }
    }
}