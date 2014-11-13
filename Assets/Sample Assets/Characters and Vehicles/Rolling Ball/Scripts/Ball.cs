using UnityEngine;

public class Ball : MonoBehaviour
{
	[SerializeField] private float movePower = 1;               // The force added to the ball to move it.
    [SerializeField] private bool useTorque = true;             // Whether or not to use torque to move the ball.
    [SerializeField] private float maxAngularVelocity = 25;     // The maximum velocity the ball can rotate at.
    [SerializeField] private float jumpPower = 2;               // The force added to the ball when it jumps.
	private SphereCollider ballCollider;
    
    private const float GroundRayLength = 1f;                   // The length of the ray to check if the ball is grounded.


    void Start()
	{
        // Set the maximum angular velocity.
        ballCollider=gameObject.GetComponent<SphereCollider>();
		rigidbody.maxAngularVelocity = maxAngularVelocity;
	}

	private const float VelocityCap = 4f;
	private float ballY;
	
	public void Move (Vector3 move, bool jump)
    {
        // Set the move direction to be relative to the camera.
	    var moveDirection = Camera.main.transform.TransformDirection( move );
		moveDirection.y=0;
		//int moveMultiplier =(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.S)||Input.GetKey(KeyCode.D))?1:0;
		moveDirection=moveDirection.normalized;
        // If using torque to rotate the ball...
		if (useTorque) 
            // ... add torque around the axis defined by the move direction.
            rigidbody.AddTorque(new Vector3(moveDirection.z, 0, -moveDirection.x) * movePower);
		else
            // Otherwise add force in the move direction.
			rigidbody.AddForce( moveDirection * movePower );
		//rigidbody.velocity=new Vector3(moveDirection.x*movePower*moveMultiplier,rigidbody.velocity.y,moveDirection.z*movePower*moveMultiplier);
		Vector2 v = new Vector2(rigidbody.velocity.x,rigidbody.velocity.z);
			if(v.magnitude>movePower){
				v.Normalize();
				v*=movePower;
			rigidbody.velocity=new Vector3(v.x,rigidbody.velocity.y,v.y);
			}
			/*if(move.x+move.y==0){
			if(rigidbody.velocity.magnitude>0)
				rigidbody.velocity=Vector3.zero;
			}else{
				rigidbody.velocity = moveDirection * movePower;
			}*/
		/*Vector3 xzVel = new Vector3(rigidbody.velocity.x,0,rigidbody.velocity.z);
		if(xzVel.magnitude>VelocityCap){
			xzVel=xzVel.normalized*VelocityCap;
			rigidbody.velocity=new Vector3(xzVel.x,rigidbody.velocity.y,xzVel.z);
			}*/
        // If on the ground and jump is pressed...
        if (Mathf.Abs(rigidbody.velocity.y)<0.1f&&Physics.Raycast(transform.position, -Vector3.up, ballCollider.radius+0.1f) && jump)
        {
            // ... add force in upwards.
           rigidbody.velocity=new Vector3(rigidbody.velocity.x,jumpPower,rigidbody.velocity.z);
			//rigidbody.AddForce(Vector3.up*jumpPower,ForceMode.Impulse);
        }else{
			//ballY-=jumpPower;
        }
		
	}
}
