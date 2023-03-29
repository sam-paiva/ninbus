namespace Ninbus.EventBus.Test
{
    public class RetryPolicyConfigurationTest
    {
        private readonly RetryPolicyConfiguration _sut;

        public RetryPolicyConfigurationTest()
        {
            _sut = new RetryPolicyConfiguration();
        }

        [Fact]
        public void Should_Set_Interval_Time_When_Call_SetIntervalTime()
        {
            var policy = _sut.SetIntervalTime(TimeSpan.FromSeconds(1));
            Assert.Equal(policy.RetryInterval, TimeSpan.FromSeconds(1));
        }

        [Theory]
        [InlineData(5)]
        [InlineData(2)]
        [InlineData(8)]
        public void Should_Set_Max_Retry_Times_When_Call_RetryForTimes(int value)
        {
            var policy = _sut.RetryForTimes(value);
            Assert.Equal(policy.MaxRetryTimes, value);
        }

        [Fact]
        public void Should_Set_Retry_Forever_True_When_Call_RetryForever()
        {
            var policy = _sut.RetryForever();
            Assert.Equivalent(policy.ForeverRetry, true);
        }

        [Fact]
        public void Should_Set_Exception_Type_When_Call_ShouldDiscard()
        {
            var policy = _sut.ShouldDiscard<Exception>();
            Assert.False(policy.ForeverRetry);
            Assert.Equal(typeof(Exception), policy.ExceptionType);
        }
    }
}
