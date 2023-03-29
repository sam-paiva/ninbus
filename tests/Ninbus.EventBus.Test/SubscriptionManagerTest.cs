using Moq;

namespace Ninbus.EventBus.Test
{
    public class SubscriptionManagerTest
    {
        private readonly ISubscriptionManager _sut;

        public SubscriptionManagerTest()
        {
            _sut = new SubscriptionManager();
        }

        [Fact]
        public void Should_Return_Subscription_When_Subscribe()
        {
            var subscription = _sut.AddSubscription<EventTest>();
            Assert.IsType<Subscription<EventTest>>(subscription);
        }

        [Fact]
        public void Should_Return_Subscription_When_Find_Subscription()
        {
            _sut.AddSubscription<EventTest>();
            var subscription = _sut.FindSubscription<EventTest>();
            Assert.IsType<Subscription<EventTest>>(subscription);
        }

        [Fact]
        public void Should_Return_ISubscription_When_Find_Subscription()
        {
            _sut.AddSubscription<EventTest>();
            var subscription = _sut.FindSubscription(nameof(EventTest));
            Assert.IsAssignableFrom<ISubscription>(subscription);
        }

        [Fact]
        public void Should_Return_Null_When_Dont_Find_Subscription()
        {
            var subscription = _sut.FindSubscription(nameof(EventTest));
            Assert.Null(subscription);
        }
    }
}
