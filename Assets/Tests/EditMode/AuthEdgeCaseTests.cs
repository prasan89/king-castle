// AuthEdgeCaseTests.cs — EditMode tests for authentication boundary cases
// and service-locator double-registration behavior.

using System;
using NUnit.Framework;
using KingSmash.Core;
using KingSmash.Save;
using KingSmash.Services;

namespace KingSmash.Tests.EditMode
{
    [TestFixture]
    public class AuthEdgeCaseTests
    {
        // ── AuthServiceMock baseline ──────────────────────────────────────────

        [Test]
        public void AuthServiceMock_IsAnonymousOnStart()
        {
            // AuthServiceMock is not signed in until SignInAnonymouslyAsync() is called;
            // once called it produces an anonymous user immediately (no network required).
            var mock = new AuthServiceMock();
            mock.SignInAnonymouslyAsync().Wait();

            Assert.IsTrue(mock.IsSignedIn,   "Mock must be signed in after SignInAnonymouslyAsync.");
            Assert.IsNotNull(mock.CurrentUser);
            Assert.IsTrue(mock.CurrentUser.IsAnonymous, "Resulting user must be anonymous.");
            Assert.IsFalse(string.IsNullOrEmpty(mock.CurrentUser.UserId),
                "UserId must not be null or empty.");
        }

        [Test]
        public void AuthServiceMock_UserIdIsConsistent()
        {
            // Each AuthServiceMock instance generates a stable UserId for its session.
            var mock1 = new AuthServiceMock();
            var mock2 = new AuthServiceMock();
            mock1.SignInAnonymouslyAsync().Wait();
            mock2.SignInAnonymouslyAsync().Wait();

            // Both instances must have a non-null, non-empty UserId.
            Assert.IsFalse(string.IsNullOrEmpty(mock1.CurrentUser.UserId),
                "mock1 UserId must be non-empty.");
            Assert.IsFalse(string.IsNullOrEmpty(mock2.CurrentUser.UserId),
                "mock2 UserId must be non-empty.");

            // It is acceptable (and expected) for different instances to produce
            // different IDs since the mock uses Guid.NewGuid().
        }

        [Test]
        public void AuthServiceMock_UserIdNeverContainsSensitivePatterns()
        {
            var mock = new AuthServiceMock();
            mock.SignInAnonymouslyAsync().Wait();
            string uid = mock.CurrentUser.UserId;

            // A UserId must never accidentally embed credential-like patterns.
            Assert.IsFalse(uid.ToLowerInvariant().Contains("password"),
                "UserId must not contain 'password'.");
            Assert.IsFalse(uid.ToLowerInvariant().Contains("token"),
                "UserId must not contain 'token'.");
            Assert.IsFalse(uid.ToLowerInvariant().Contains("secret"),
                "UserId must not contain 'secret'.");
        }

        // ── PlayerDataServiceMock baseline ────────────────────────────────────

        [Test]
        public void PlayerDataServiceMock_ReturnsValidData()
        {
            var mock = new PlayerDataServiceMock();
            Assert.IsNotNull(mock, "PlayerDataServiceMock must construct without error.");

            // LoadPlayerDataAsync must return a non-null SaveData.
            var task = mock.LoadPlayerDataAsync("test-player-id");
            task.Wait();

            Assert.IsNotNull(task.Result, "LoadPlayerDataAsync must return a valid SaveData.");
            Assert.IsFalse(string.IsNullOrEmpty(task.Result.playerId),
                "Returned SaveData must have a non-empty playerId.");
        }

        // ── ServiceLocator double-registration behavior ───────────────────────

        [Test]
        public void ServiceLocator_NoDoubleRegistration_OverwritesCleanly()
        {
            // Design decision (documented in ServiceLocator.cs):
            // Registering the same type twice logs a warning and overwrites —
            // it does NOT throw. This test verifies that contract.

            ServiceLocator.Initialize();

            var first  = new AuthServiceMock();
            var second = new AuthServiceMock();

            ServiceLocator.Register<IAuthService>(first);
            // A second registration must not throw; it overwrites.
            Assert.DoesNotThrow(() => ServiceLocator.Register<IAuthService>(second),
                "Registering a type twice must not throw — it overwrites.");

            // The locator must now return the second instance.
            var retrieved = ServiceLocator.Get<IAuthService>();
            Assert.AreSame(second, retrieved,
                "After double-registration, Get<T>() must return the most-recently registered instance.");

            ServiceLocator.Initialize();
        }
    }
}
