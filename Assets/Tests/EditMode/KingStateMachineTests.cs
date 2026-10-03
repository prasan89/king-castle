using NUnit.Framework;
using KingSmash.Characters;

namespace KingSmash.Tests.EditMode
{
    public class KingStateMachineTests
    {
        private KingStateMachine _sm;
        private KingState _capturedNext;
        private bool _eventFired;

        [SetUp]
        public void SetUp()
        {
            _sm = new KingStateMachine();
            _capturedNext = KingState.Idle;
            _eventFired = false;
        }

        [TearDown]
        public void TearDown()
        {
            KingStateMachine.OnStateChanged -= CaptureNext;
            KingStateMachine.OnStateChanged -= SetFired;
        }

        private void CaptureNext(KingState _, KingState next) => _capturedNext = next;
        private void SetFired(KingState _, KingState __) => _eventFired = true;

        [Test]
        public void InitialState_IsIdle()
            => Assert.AreEqual(KingState.Idle, _sm.Current);

        [Test]
        public void Idle_To_PreparingLaunch_Valid()
        {
            Assert.IsTrue(_sm.TryTransition(KingState.PreparingLaunch));
            Assert.AreEqual(KingState.PreparingLaunch, _sm.Current);
        }

        [Test]
        public void Idle_To_Flying_Invalid()
        {
            Assert.IsFalse(_sm.TryTransition(KingState.Flying));
            Assert.AreEqual(KingState.Idle, _sm.Current);
        }

        [Test]
        public void FullLaunchLoop_Valid()
        {
            Assert.IsTrue(_sm.TryTransition(KingState.PreparingLaunch));
            Assert.IsTrue(_sm.TryTransition(KingState.Launched));
            Assert.IsTrue(_sm.TryTransition(KingState.Flying));
            Assert.IsTrue(_sm.TryTransition(KingState.Impact));
            Assert.IsTrue(_sm.TryTransition(KingState.Flying));   // bounce
            Assert.IsTrue(_sm.TryTransition(KingState.Idle));     // land
        }

        [Test]
        public void ImpactToStunned_Valid()
        {
            _sm.TryTransition(KingState.PreparingLaunch);
            _sm.TryTransition(KingState.Launched);
            _sm.TryTransition(KingState.Flying);
            _sm.TryTransition(KingState.Impact);
            Assert.IsTrue(_sm.TryTransition(KingState.Stunned));
        }

        [Test]
        public void StunnedToDefeated_Valid()
        {
            _sm.TryTransition(KingState.PreparingLaunch);
            _sm.TryTransition(KingState.Launched);
            _sm.TryTransition(KingState.Flying);
            _sm.TryTransition(KingState.Impact);
            _sm.TryTransition(KingState.Stunned);
            Assert.IsTrue(_sm.TryTransition(KingState.Defeated));
        }

        [Test]
        public void IdleToVictory_Valid()
        {
            Assert.IsTrue(_sm.TryTransition(KingState.Victory));
        }

        [Test]
        public void OnStateChanged_FiresOnValidTransition()
        {
            KingStateMachine.OnStateChanged += CaptureNext;
            _sm.TryTransition(KingState.PreparingLaunch);
            Assert.AreEqual(KingState.PreparingLaunch, _capturedNext);
        }

        [Test]
        public void OnStateChanged_DoesNotFireOnInvalidTransition()
        {
            KingStateMachine.OnStateChanged += SetFired;
            _sm.TryTransition(KingState.Flying); // invalid from Idle
            Assert.IsFalse(_eventFired);
        }

        [Test]
        public void AimCancelled_PreparingLaunch_To_Idle()
        {
            _sm.TryTransition(KingState.PreparingLaunch);
            Assert.IsTrue(_sm.TryTransition(KingState.Idle));
        }
    }
}
