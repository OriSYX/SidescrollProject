using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShockwaveEffectController:MagicEffectControllerBase {

	[SerializeField] float force;
	[SerializeField] bool useDirectionProcesser;
	[SerializeField] bool eliminateNegativeVelocity;
	[SerializeField] float radius;
	[SerializeField] AnimationCurve dropoff;

	ContactFilter2D filter;
	static readonly Angle upRight = new Angle(60);
	static readonly Angle upLeft = new Angle(120);
	static readonly Angle downRight = new Angle(-75);
	static readonly Angle downLeft = new Angle(-115);

	private void Start() {
		filter=Utility.GetFilterByLayerName("Entity");
	}

	void Update() {

		int cnt = Physics2D.OverlapCircleNonAlloc(transform.position,radius,Utility.colliderBuffer,filter.layerMask);

		for(int i = 0;i<cnt;i++) {
			Rigidbody2D other = Utility.colliderBuffer[i].attachedRigidbody;
			if(!other) continue;
			if(other.gameObject.tag=="Stander") continue;

			bool isOtherGrounded = true;
			ObjectGroundedTester otherGrounded = other.GetComponent<ObjectGroundedTester>();
			if(other) isOtherGrounded=otherGrounded.grounded;

			Vector2 otherPosition = other.position;
			Vector2 otherClosestPosition = other.GetComponent<Collider2D>().ClosestPoint(transform.position);
			Vector2 offset = (otherPosition-(Vector2)transform.position).normalized;

			float distance = (otherClosestPosition-(Vector2)transform.position).magnitude;

			if(useDirectionProcesser) offset=DirectionProcesser(offset);

			float forceThisTime = force*(isOtherGrounded ? 0.6f : 1);

			float normalizedDistance = distance/radius;
			forceThisTime*=dropoff.Evaluate(normalizedDistance);

			if(eliminateNegativeVelocity) other.velocity=Utility.EliminateOnDirection(other.velocity,-offset);
			other.AddForce(offset*forceThisTime,ForceMode2D.Impulse);

		}

		Destroy(gameObject);

	}

	Vector2 DirectionProcesser(Vector2 direction) {
		if(((Angle)direction).IfBetween(upRight,upLeft)) {
			direction.y=1;
			if(Mathf.Abs(direction.x)>0.15f) direction.x=direction.x>0 ? 0.3f : -0.3f;
			else direction.x=0;
		} else {
			direction.y=0.3f;
			if(direction.x!=0) direction.x=direction.x>0 ? 1 : -1;
		}
		return direction.normalized;
	}

}