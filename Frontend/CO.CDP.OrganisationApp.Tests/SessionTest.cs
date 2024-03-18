using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Session;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CO.CDP.OrganisationApp.Tests
{
    public class SessionTest
    {
        [Fact]
        public void Get_WhenSessionIsNull_ThrowException()
        {
            var httpContextAccessor = GivenHttpContextWithNoSession();
            var session = new Session(httpContextAccessor);

            Action act = () => session.Get<int>("session_key");

            act.Should().Throw<Exception>().WithMessage("Session is not available");
        }

        [Fact]
        public void Get_WhenHttpContextIsMissing_ThrowException()
        {
            var httpContextAccessor = GivenNoHttpContext();
            var session = new Session(httpContextAccessor);

            Action act = () => session.Get<int>("session_key");

            act.Should().Throw<Exception>().WithMessage("Session is not available");
        }

        [Fact]
        public void Get_WhenNoValueInSession_ReturnsNull()
        {
            var httpContextAccessor = GivenHttpContextWithSession();
            var session = new Session(httpContextAccessor);
            session.Set("session_key", default(string));

            var val = session.Get<string?>("session_key");

            val.Should().BeNull();
        }

        [Fact]
        public void Get_WhenStringValueInSession_ReturnsSessionValue()
        {
            var httpContextAccessor = GivenHttpContextWithSession();
            var session = new Session(httpContextAccessor);
            session.Set("session_key", "session_value");

            var val = session.Get<string>("session_key");

            val.Should().Be("session_value");
        }

        [Fact]
        public void Get_WhenIntValueInSession_ReturnsSessionValue()
        {
            var httpContextAccessor = GivenHttpContextWithSession();

            var session = new Session(httpContextAccessor);
            session.Set("session_key", 15);

            var val = session.Get<int?>("session_key");

            val.Should().NotBeNull();
            val.Should().Be(15);
        }

        private record TestClass
        {
            public string Prop1 { get; init; } = "Test Val";
            public int Prop2 { get; init; } = 20;
            public bool Prop3 { get; init; } = true;
        }

        [Fact]
        public void Get_WhenClassObjectValueInSession_ReturnsSessionValue()
        {
            var httpContextAccessor = GivenHttpContextWithSession();
            var session = new Session(httpContextAccessor);
            session.Set("session_key", new TestClass
            {
                Prop1 = "Test value",
                Prop2 = 42,
                Prop3 = false
            });

            var val = session.Get<TestClass?>("session_key");

            val.Should().Be(new TestClass
            {
                Prop1 = "Test value",
                Prop2 = 42,
                Prop3 = false
            });
        }

        [Fact]
        public void Set_WhenSessionIsNull_ThrowException()
        {
            var httpContextAccessor = GivenHttpContextWithNoSession();
            var session = new Session(httpContextAccessor);

            Action act = () => session.Set("session_key", "session_value");

            act.Should().Throw<Exception>().WithMessage("Session is not available");
        }

        [Fact]
        public void Set_WhenSessionIsAvailable_ValueIsSet()
        {
            var httpContextAccessor = GivenHttpContextWithSession();
            var session = new Session(httpContextAccessor);
            session.Set("session_key", "session_value");

            Assert.Equal("session_value", session.Get<string>("session_key"));
        }

        [Fact]
        public void Remove_WhenSessionIsNull_ThrowException()
        {
            var httpContextAccessor = GivenHttpContextWithNoSession();
            var session = new Session(httpContextAccessor);

            Action act = () => session.Remove("session_key");

            act.Should().Throw<Exception>().WithMessage("Session is not available");
        }

        [Fact]
        public void Remove_WhenSessionIsAvailable_ValueIsRemoved()
        {
            var httpContextAccessor = GivenHttpContextWithSession();
            var session = new Session(httpContextAccessor);

            session.Remove("session_key");

            Assert.Null(session.Get<string>("session_key"));
        }

        private static IHttpContextAccessor GivenHttpContextWithNoSession()
        {
            return new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        private static IHttpContextAccessor GivenNoHttpContext()
        {
            return new HttpContextAccessor();
        }

        private static IHttpContextAccessor GivenHttpContextWithSession()
        {
            return new HttpContextAccessor
            {
                HttpContext = new DefaultHttpContext
                {
                    Session = new DistributedSession(
                        new MemoryDistributedCache(Options.Create(new MemoryDistributedCacheOptions())),
                        "session_key",
                        TimeSpan.FromHours(1),
                        TimeSpan.Zero,
                        () => true,
                        LoggerFactory.Create(_ => { }),
                        true
                    )
                }
            };
        }
    }
}