namespace Ninbus.EventBus.Test
{
    public class ResultTest
    {
        private Result _sut;

        [Fact]
        public void When_Success_Should_Return_Result_Success_True()
        {
            _sut = Result.Success();
            Assert.True(_sut.IsSuccess);
            Assert.Null(_sut.Exception);
        }

        [Fact]
        public void When_Error_Should_Return_Result_Success_False()
        {
            _sut = Result.Error(new Exception());
            Assert.False(_sut.IsSuccess);
            Assert.NotNull(_sut.Exception);
            Assert.IsType<Exception>(_sut.Exception);
        }
    }
}
