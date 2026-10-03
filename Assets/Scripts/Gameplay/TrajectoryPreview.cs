using UnityEngine;

namespace KingSmash.Gameplay
{
    /// Renders a dotted arc trajectory preview using a LineRenderer.
    /// Uses the same kinematic equations as KingProjectile so it matches actual flight.
    [RequireComponent(typeof(LineRenderer))]
    public class TrajectoryPreview : MonoBehaviour
    {
        [SerializeField] private int _stepCount = 40;
        [SerializeField] private float _stepTime = 0.05f;
        [SerializeField] private LayerMask _collisionMask;
        [SerializeField] private float _dotRadius = 0.06f;

        private LineRenderer _lr;

        private void Awake()
        {
            _lr = GetComponent<LineRenderer>();
            _lr.positionCount = 0;
            _lr.startWidth = _dotRadius * 2f;
            _lr.endWidth   = _dotRadius * 2f;
            // Dotted pattern via texture offset — art sets up the material
            SetVisible(false);
        }

        private void OnEnable()
        {
            AimController.OnAimUpdated    += HandleAimUpdated;
            AimController.OnAimCancelled  += HandleAimCancelled;
            AimController.OnLaunchReleased += HandleLaunched;
        }

        private void OnDisable()
        {
            AimController.OnAimUpdated    -= HandleAimUpdated;
            AimController.OnAimCancelled  -= HandleAimCancelled;
            AimController.OnLaunchReleased -= HandleLaunched;
        }

        private void HandleAimUpdated(Vector2 direction, float power, Vector2 launchWorldPos, float gravityScale)
        {
            SetVisible(true);
            DrawTrajectory(launchWorldPos, direction * power, gravityScale);
        }

        private void HandleAimCancelled()  => SetVisible(false);
        private void HandleLaunched(Vector2 _, float __)  => SetVisible(false);

        /// Kinematic arc: p(t) = p0 + v0*t + 0.5*g*t^2
        private void DrawTrajectory(Vector2 origin, Vector2 initialVelocity, float gravityScale)
        {
            float gMag = UnityEngine.Physics2D.gravity.magnitude * gravityScale;
            Vector2 gVec = UnityEngine.Physics2D.gravity.normalized * gMag;

            _lr.positionCount = _stepCount;
            Vector2 pos = origin;
            Vector2 vel = initialVelocity;

            for (int i = 0; i < _stepCount; i++)
            {
                _lr.SetPosition(i, pos);

                // Check for early termination on collision
                var hit = Physics2D.CircleCast(pos, _dotRadius, vel.normalized, vel.magnitude * _stepTime, _collisionMask);
                if (hit.collider != null)
                {
                    _lr.positionCount = i + 1;
                    break;
                }

                vel  += gVec * _stepTime;
                pos  += vel  * _stepTime;
            }
        }

        private void SetVisible(bool visible) => _lr.enabled = visible;
    }
}
