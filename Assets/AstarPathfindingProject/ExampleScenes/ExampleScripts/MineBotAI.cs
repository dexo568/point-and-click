using UnityEngine;
using Pathfinding;
using Pathfinding.RVO;
using Pathfinding.Util;

namespace Pathfinding.Examples {
	/** AI controller specifically made for the spider robot.
	 * The spider robot (or mine-bot) which has been copied from the Unity Example Project
	 * can have this script attached to be able to pathfind around with animations working properly.\n
	 * This script should be attached to a parent GameObject however since the original bot has Z+ as up.
	 * This component requires Z+ to be forward and Y+ to be up.\n
	 *
	 * It overrides the AIPath class, see that class's documentation for more information on most variables.\n
	 * Animation is handled by this component. The Animation component refered to in #anim should have animations named "awake" and "forward".
	 * The forward animation will have it's speed modified by the velocity and scaled by #animationSpeed to adjust it to look good.
	 * The awake animation will only be sampled at the end frame and will not play.\n
	 * When the end of path is reached, if the #endOfPathEffect is not null, it will be instantiated at the current position. However a check will be
	 * done so that it won't spawn effects too close to the previous spawn-point.
	 * \shadowimage{mine-bot.png}
	 */
	[RequireComponent(typeof(Seeker))]
	[HelpURL("http://arongranberg.com/astar/docs/class_pathfinding_1_1_examples_1_1_mine_bot_a_i.php")]
	public class MineBotAI : AIPath {
		/** Animation component.
		 * Should hold animations "awake" and "forward"
		 */
		public Animation anim;

		/** Minimum velocity for moving */
		public float sleepVelocity = 0.4F;

		/** Speed relative to velocity with which to play animations */
		public float animationSpeed = 0.2F;

		/** Effect which will be instantiated when end of path is reached.
		 * \see OnTargetReached */
		public GameObject endOfPathEffect;

		public new void Start () {
			// Prioritize the walking animation
			anim["forward"].layer = 10;

			// Play all animations
			anim.Play("awake");
			anim.Play("forward");

			// Setup awake animations properties
			anim["awake"].wrapMode = WrapMode.Clamp;
			anim["awake"].speed = 0;
			anim["awake"].normalizedTime = 1F;

			// Call Start in base script (AIPath)
			base.Start();
		}

		/** Point for the last spawn of #endOfPathEffect */
		protected Vector3 lastTarget;

		/**
		 * Called when the end of path has been reached.
		 * An effect (#endOfPathEffect) is spawned when this function is called
		 * However, since paths are recalculated quite often, we only spawn the effect
		 * when the current position is some distance away from the previous spawn-point
		 */
		public override void OnTargetReached () {
			if (endOfPathEffect != null && Vector3.Distance(tr.position, lastTarget) > 1) {
				GameObject.Instantiate(endOfPathEffect, tr.position, tr.rotation);
				lastTarget = tr.position;
			}
		}

		protected override void Update () {
			base.Update();

			// Calculate the velocity relative to this transform's orientation
			Vector3 relVelocity = tr.InverseTransformDirection(velocity);
			relVelocity.y = 0;

			if (relVelocity.sqrMagnitude <= sleepVelocity*sleepVelocity) {
				// Fade out walking animation
				anim.Blend("forward", 0, 0.2F);
			} else {
				// Fade in walking animation
				anim.Blend("forward", 1, 0.2F);

				// Modify animation speed to match velocity
				AnimationState state = anim["forward"];

				float speed = relVelocity.z;
				state.speed = speed*animationSpeed;
			}
		}

			/** Called during either Update or FixedUpdate depending on if rigidbodies are used for movement or not */
	protected override void MovementUpdate (float deltaTime) {
		if (!canMove) return;

		if (!interpolator.valid) {
			velocity2D = Vector3.zero;
		} else {
			var currentPosition = tr.position;

			interpolator.MoveToLocallyClosestPoint(currentPosition, true, false);
			interpolator.MoveToCircleIntersection2D(currentPosition, pickNextWaypointDist, movementPlane);
			targetPoint = interpolator.position;
			var dir = movementPlane.ToPlane(targetPoint-currentPosition);

			var distanceToEnd = dir.magnitude + interpolator.remainingDistance;
			// How fast to move depending on the distance to the target.
			// Move slower as the character gets closer to the target.
			float slowdown = slowdownDistance > 0 ? distanceToEnd / slowdownDistance : 1;

			// a = v/t, should probably expose as a variable
			float acceleration = speed / 0.4f;
			velocity2D += MovementUtilities.CalculateAccelerationToReachPoint(dir, dir.normalized*speed, velocity2D, acceleration, speed) * deltaTime;
			velocity2D = MovementUtilities.ClampVelocity(velocity2D, speed, slowdown, false, movementPlane.ToPlane(rotationIn2D ? tr.up : tr.forward));

			ApplyGravity(deltaTime);

			if (distanceToEnd <= endReachedDistance && !TargetReached) {
				TargetReached = true;
				OnTargetReached();
			}

			// Rotate towards the direction we are moving in
			var currentRotationSpeed = rotationSpeed * Mathf.Clamp01((Mathf.Sqrt(slowdown) - 0.3f) / 0.7f);
			//RotateTowards(velocity2D, currentRotationSpeed * deltaTime);

			var delta2D = CalculateDeltaToMoveThisFrame(movementPlane.ToPlane(currentPosition), distanceToEnd, deltaTime);
			Move(currentPosition, movementPlane.ToWorld(delta2D, verticalVelocity * deltaTime));

			velocity = movementPlane.ToWorld(velocity2D, verticalVelocity);
		}
	}
	}
}
