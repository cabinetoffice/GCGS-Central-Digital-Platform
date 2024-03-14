using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using HttpSession = Microsoft.AspNetCore.Http.ISession;

namespace CO.CDP.OrganisationApp.Tests
{
    public class SessionTest
    {
        const string dummySessionKey = "session_key";
        const string dummySessionValue = "session_value";
        readonly Mock<IHttpContextAccessor> httpContextAccessorMock = new();
        readonly Mock<HttpSession> sessionMock = new();

        [Fact]
        public void Get_WhenSessionIsNull_ThrowException()
        {
            SetDefaultHttpContext();

            var session = new Session(httpContextAccessorMock.Object);

            Action act = () => session.Get<int>(dummySessionKey);

            act.Should().Throw<Exception>().WithMessage("Session is not available");
        }

        [Fact]
        public void Get_WhenNoValueInSession_ReturnsNull()
        {
            SetHttpContextWithSessionValue(default(string));

            var session = new Session(httpContextAccessorMock.Object);

            var val = session.Get<string?>(dummySessionKey);

            val.Should().BeNull();
        }

        [Fact]
        public void Get_WhenStringValueInSession_ReturnsSessionValue()
        {
            SetHttpContextWithSessionValue(dummySessionValue);

            var session = new Session(httpContextAccessorMock.Object);

            var val = session.Get<string>(dummySessionKey);

            val.Should().Be(dummySessionValue);
        }

        [Fact]
        public void Get_WhenIntValueInSession_ReturnsSessionValue()
        {
            SetHttpContextWithSessionValue(15);

            var session = new Session(httpContextAccessorMock.Object);

            var val = session.Get<int?>(dummySessionKey);

            val.Should().NotBeNull();
            val.Should().Be(15);
        }

        private record TestClass
        {
            public string Prop1 { get; set; } = "Test Val";
            public int Prop2 { get; set; } = 20;
            public bool Prop3 { get; set; } = false;
        }

        [Fact]
        public void Get_WhenClassObjectValueInSession_ReturnsSessionValue()
        {
            SetHttpContextWithSessionValue(new TestClass());

            var session = new Session(httpContextAccessorMock.Object);

            var val = session.Get<TestClass?>(dummySessionKey);

            val.Should().NotBeNull();
            val!.Prop1.Should().Be("Test Val");
            val!.Prop2.Should().Be(20);
            val!.Prop3.Should().Be(false);
        }

        [Fact]
        public void Set_WhenSessionIsNull_ThrowException()
        {
            SetDefaultHttpContext();

            var session = new Session(httpContextAccessorMock.Object);

            Action act = () => session.Set(dummySessionKey, dummySessionValue);

            act.Should().Throw<Exception>().WithMessage("Session is not available");
        }

        [Fact]
        public void Set_WhenSessionIsAvailable_HttpSessionSetIsCalled()
        {
            SetDefaultHttpContext(true);

            var session = new Session(httpContextAccessorMock.Object);

            session.Set(dummySessionKey, dummySessionValue);

            sessionMock?.Verify(_ => _.Set(dummySessionKey, It.IsAny<byte[]>()), Times.Once);
        }

        [Fact]
        public void Remove_WhenSessionIsNull_ThrowException()
        {
            SetDefaultHttpContext();

            var session = new Session(httpContextAccessorMock.Object);

            Action act = () => session.Remove(dummySessionKey);

            act.Should().Throw<Exception>().WithMessage("Session is not available");
        }

        [Fact]
        public void Remove_WhenSessionIsAvailable_HttpSessionRemoveIsCalled()
        {
            SetDefaultHttpContext(true);

            var session = new Session(httpContextAccessorMock.Object);

            session.Remove(dummySessionKey);

            sessionMock?.Verify(_ => _.Remove(dummySessionKey), Times.Once);
        }

        private void SetDefaultHttpContext(bool withDefaultSession = false)
        {
            var httpContext = default(HttpContext);

            if (withDefaultSession)
            {
                httpContext = new DefaultHttpContext { Session = sessionMock.Object };
            }

            httpContextAccessorMock.SetupGet(_ => _.HttpContext).Returns(httpContext);
        }

        private void SetHttpContextWithSessionValue<T>(T value)
        {
            var outVal = value == null ? default : Encoding.UTF8.GetBytes(JsonSerializer.Serialize(value));

            sessionMock
                .Setup(_ => _.TryGetValue(It.IsAny<string>(), out outVal))
                .Returns(value != null);

            httpContextAccessorMock.SetupGet(_ => _.HttpContext)
                .Returns(new DefaultHttpContext { Session = sessionMock.Object });
        }
    }
}