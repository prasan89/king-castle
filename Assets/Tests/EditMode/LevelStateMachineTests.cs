using NUnit.Framework;
using KingSmash.Levels;

namespace KingSmash.Tests.EditMode
{
    public class LevelStateMachineTests
    {
        private LevelStateMachine _sm;

        // Named fields so we can properly remove them from the static event
        private LevelState _capturedNext;
        private bool _eventFired;

        [SetUp]
        public void SetUp()
        {
            _sm = new LevelStateMachine();
            _capturedNext = LevelState.LevelStart;
            _eventFired = false;
        }

        [TearDown]
        public void TearDown()
        {
            // Clear static event to avoid cross-test contamination
            LevelStateMachine.OnStateChanged -= CaptureNext;
            LevelStateMachine.OnStateChanged -= SetFired;
        }

        private void CaptureNext(LevelState _, LevelState next) => _capturedNext = next;
        private void SetFired(LevelState _, LevelState __) => _eventFired = true;

        [Test]
        public void InitialState_IsLevelStart()
            => Assert.AreEqual(LevelState.LevelStart, _sm.Current);

        [Test]
        public void ValidTransition_LevelStart_To_Playing()
        {
            bool result = _sm.TryTransition(LevelState.Playing);
            Assert.IsTrue(result);
            Assert.AreEqual(LevelState.Playing, _sm.Current);
        }

        [Test]
        public void InvalidTransition_LevelStart_To_LevelComplete_Fails()
        {
            bool result = _sm.TryTransition(LevelState.LevelComplete);
            Assert.IsFalse(result);
            Assert.AreEqual(LevelState.LevelStart, _sm.Current, "State should be unchanged after invalid transition.");
        }

        [Test]
        public void FullPlayLoop_AllTransitionsSucceed()
        {
            Assert.IsTrue(_sm.TryTransition(LevelState.Playing));
            Assert.IsTrue(_sm.TryTransition(LevelState.KingFlying));
            Assert.IsTrue(_sm.TryTransition(LevelState.Resolving));
            Assert.IsTrue(_sm.TryTransition(LevelState.LevelComplete));
        }

        [Test]
        public void RestartFromComplete_Succeeds()
        {
            _sm.TryTransition(LevelState.Playing);
            _sm.TryTransition(LevelState.KingFlying);
            _sm.TryTransition(LevelState.LevelComplete);
            Assert.IsTrue(_sm.TryTransition(LevelState.LevelStart));
        }

        [Test]
        public void RestartFromFailed_Succeeds()
        {
            _sm.TryTransition(LevelState.Playing);
            _sm.TryTransition(LevelState.LevelFailed);
            Assert.IsTrue(_sm.TryTransition(LevelState.LevelStart));
        }

        [Test]
        public void OnStateChanged_FiresOnValidTransition()
        {
            LevelStateMachine.OnStateChanged += CaptureNext;
            _sm.TryTransition(LevelState.Playing);
            Assert.AreEqual(LevelState.Playing, _capturedNext);
        }

        [Test]
        public void OnStateChanged_DoesNotFireOnInvalidTransition()
        {
            LevelStateMachine.OnStateChanged += SetFired;
            _sm.TryTransition(LevelState.LevelComplete); // invalid from LevelStart
            Assert.IsFalse(_eventFired);
        }

        [Test]
        public void KingFlying_CanTransitionToResolving()
        {
            _sm.TryTransition(LevelState.Playing);
            Assert.IsTrue(_sm.TryTransition(LevelState.KingFlying));
            Assert.IsTrue(_sm.TryTransition(LevelState.Resolving));
        }

        [Test]
        public void Playing_CanTransitionToFailed_Directly()
        {
            _sm.TryTransition(LevelState.Playing);
            Assert.IsTrue(_sm.TryTransition(LevelState.LevelFailed));
        }
    }
}
