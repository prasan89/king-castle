using NUnit.Framework;
using UnityEngine;

namespace KingSmash.Tests.EditMode
{
    /// Tests the kinematic equations used by TrajectoryPreview — no MonoBehaviour needed.
    public class TrajectoryMathTests
    {
        private const float G     = 9.81f;
        private const float Eps   = 0.05f;  // tolerance in Unity units

        private static Vector2 ProjectilePosition(Vector2 v0, float gravityScale, float t)
        {
            var g = new Vector2(0f, -G * gravityScale);
            return v0 * t + 0.5f * g * t * t;
        }

        [Test]
        public void HorizontalLaunch_XIncreasesWithTime()
        {
            var v0  = new Vector2(10f, 0f);
            var p1  = ProjectilePosition(v0, 1f, 0.5f);
            var p2  = ProjectilePosition(v0, 1f, 1.0f);
            Assert.Greater(p2.x, p1.x);
        }

        [Test]
        public void HorizontalLaunch_YDecreasesDueToGravity()
        {
            var v0 = new Vector2(10f, 0f);
            var p  = ProjectilePosition(v0, 1f, 1f);
            Assert.Less(p.y, 0f, "Gravity should pull the projectile down.");
        }

        [Test]
        public void HigherGravityScale_FasterFall()
        {
            var v0   = new Vector2(10f, 0f);
            var lowG = ProjectilePosition(v0, 1f, 1f);
            var hiG  = ProjectilePosition(v0, 2f, 1f);
            Assert.Less(hiG.y, lowG.y, "Higher gravity scale should produce lower y.");
        }

        [Test]
        public void UpwardLaunch_PeakThenDescends()
        {
            var v0  = new Vector2(0f, 15f);
            float t_peak = 15f / (G * 1.5f);
            var before = ProjectilePosition(v0, 1.5f, t_peak - 0.1f);
            var after  = ProjectilePosition(v0, 1.5f, t_peak + 0.1f);
            // Y at t_peak - dt should be higher than Y at t_peak + dt
            Assert.Greater(before.y, after.y - Eps);
        }

        [Test]
        public void AngleLaunch_MatchesClassicalRange()
        {
            // R = v0^2 * sin(2*theta) / g  (theta=45 gives max range)
            float v0     = 12f;
            float theta  = 45f * Mathf.Deg2Rad;
            float expectedRange = v0 * v0 * Mathf.Sin(2f * theta) / G;
            var launch = new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)) * v0;
            // Time of flight: t = 2*v0y/g
            float tof    = 2f * launch.y / G;
            var landing  = ProjectilePosition(launch, 1f, tof);
            Assert.AreEqual(expectedRange, landing.x, expectedRange * 0.02f,
                "Horizontal range should match classical projectile formula within 2%.");
        }
    }
}
