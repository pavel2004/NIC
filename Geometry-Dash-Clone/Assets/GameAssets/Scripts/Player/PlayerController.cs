using System.Collections.Generic;
using GameAssets.Scripts.Obstacles;
using GameAssets.Scripts.Utils;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;


namespace GameAssets.Scripts.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Enum")] [SerializeField] private MoveSpeed currentMoveSpeed;
        [SerializeField] private GameModes currentGameMode;
        public float fitness;
        public Transform start;
        [Header("References")] 
        [SerializeField] private LayerMask groundMask;
        [SerializeField] private LayerMask obs;
        [SerializeField] private Transform playerSprite;
        [SerializeField] private SpriteRenderer sprite;
        [Header("Settings")] 
        [SerializeField] private float[] speedValues = { };
        [SerializeField] private float jumpForce = 10f;
        [SerializeField] private float rotationSpeed = 5f;
        [SerializeField] private float groundCheckRadius;
        [SerializeField] private float shipGravityScale;
        [SerializeField] private float cubeGravityScale;
        private Rigidbody2D rb;
        public bool die = false;
        private int _gravity = 1;

        private void Start()
        {
            Time.timeScale = 2;
            rb = GetComponent<Rigidbody2D>();
        }

        public List<float> Do(bool action)
        {
            if (!die)
            {
                Vector3 downOffset = Vector3.down * _gravity * 0.5f;
                Vector2 rightSize = Vector2.right * 1.1f;
                Vector2 upSize = Vector2.up * groundCheckRadius;

                Vector3 position = transform.position + downOffset;
                Vector2 size = rightSize + upSize;

                var obj = Physics2D.OverlapBox(position, size, 0, obs);
                if (obj != null)
                {
                    Debug.Log("under" + obj.name);
                }
               HandleMovement();
               LimitFallSpeed();
               var is_die = HandleOnWallHit();
               if (obj != null && obj.CompareTag("Finish")) 
               {
                   is_die = true;
                    
               }
               var isFly = false;
               if (currentGameMode == GameModes.Cube)
               {
                   Cube(action);
                   sprite.color = Color.white;
               }
               else if (currentGameMode == GameModes.Ship)
               {
                   Ship(action);
                   sprite.color = Color.black;
                   isFly = true;
               }
   
               fitness = transform.position.x - start.position.x ;
               if (is_die)
               {
                   this.GameObject().SetActive(false);
                   die = true;
               }

               //Debug.Log(die);
               List<float> input = new List<float>();
               var closest = ClosestObstacle();
               //Debug.Log("closet" + closest);
               input.Add(closest[1] - (2*closest[0]));
               Ray some = new Ray();
               some.direction = Vector3.down;
               RaycastHit hitinfo = new RaycastHit();
               var under = Physics.Raycast(some, out hitinfo, obs);
               if (under)
               {
                   input.Add(transform.position.y - hitinfo.point.y);
               }
               else
               {
                   input.Add(0);
               }
               Debug.Log(isFly);
               input.Add(rb.velocity.x);
               if (isFly)
               {   
                   input.Add(1);
               }
               else
               {
                   input.Add(0);
               }
               return input;  
            }
            List<float> empty = new List<float>();
            empty.Add(0);
            empty.Add(0);
            empty.Add(0);
            empty.Add(0);
            return empty;


            //HandleGameModeBehaviour();

        }   

        private List<float> ClosestObstacle()
        {
            int journey_depth = 1000;
            SpriteRenderer invis = GetComponentInChildren<SpriteRenderer>();
            //invis.enabled = false;
            Vector3 startPos = transform.position;
            Vector3 savePos = startPos;
            float type = 0;
            for (int i = 0; i < journey_depth; i++)
            {
                if (!OnGrounded())
                {
                    transform.position += Vector3.down * Time.deltaTime * 9.8f ;
                    if (transform.position.y < -5)
                    {
                        var pos = transform.position;
                        pos.y = -4.99f;
                        transform.position = pos;
                    }
                }
                HandleMovement();
                LimitFallSpeed();
                bool is_die  = HandleOnWallHit();
                type = TouchWall2(); 
                if (currentGameMode == GameModes.Cube)
                {
                    Cube(false);
                    sprite.color = Color.white;
                }
                else if (currentGameMode == GameModes.Ship)
                {
                    Ship(false);
                    sprite.color = Color.black;

                }

                //Debug.Log(savePos);
                if (!is_die) savePos = transform.position;
                else break;
            }

            //Debug.Log(savePos.x);
            transform.position = startPos;
            //invis.enabled = true;
            List<float> rt = new List<float>();
            rt.Add(type);
            rt.Add(savePos.x - startPos.x);
            return  rt;
        }
        private void HandleMovement()
        {
            var moveSpeedIndex = (int)currentMoveSpeed;
            transform.position += Vector3.right * (speedValues[moveSpeedIndex] * Time.deltaTime);
        }

        /**
        private void HandleGameModeBehaviour()
        {
             Invoke(currentGameMode.ToString(), 0); 
        }
        */
        private bool HandleOnWallHit()
        {
            if (TouchWall())
            {
                return true;
                //SceneLoader.ReloadScene();
            }
            else
            {
                return false;
            }
        }
        
        private void Cube(bool action)
        {
            if (OnGrounded())
            {

                var rotation = playerSprite.rotation.eulerAngles;
                //rotation.z = Mathf.Round(rotation.z / 90) * 90;
                //playerSprite.rotation = Quaternion.Euler(rotation); // Complete the rotation
                if (UnityEngine.Input.GetMouseButton(0) || UnityEngine.Input.GetButton("Jump") || action)
                {
                    Jump();
                }
            }
            else
            {
                //playerSprite.Rotate(Vector3.back, _gravity * rotationSpeed * Time.deltaTime);
            }

            rb.gravityScale = cubeGravityScale * _gravity;
        }
        
        private void Ship(bool action)
        { 
            //playerSprite.rotation = Quaternion.Euler(0, 0, rb.velocity.y * 2);
            rb.gravityScale = shipGravityScale;
            if (UnityEngine.Input.GetMouseButton(0) || UnityEngine.Input.GetButton("Jump") || action)
            {
                rb.velocity = new Vector2(0, 4);
            }
            
        }

        public void initCopy()
        {
            rb = GetComponent<Rigidbody2D>();
        }
        
        private void Jump()
        {
            rb.velocity = Vector2.zero;
            rb.AddForce(Vector2.up * jumpForce * _gravity, ForceMode2D.Impulse);
        }
        private bool OnGrounded()
        {
            Vector3 downOffset = Vector3.down * _gravity * 0.5f;
            Vector2 rightSize = Vector2.right * 1.1f;
            Vector2 upSize = Vector2.up * groundCheckRadius;

            Vector3 position = transform.position + downOffset;
            Vector2 size = rightSize + upSize;

            return Physics2D.OverlapBox(position, size, 0, groundMask);
        }
        private bool TouchWall()
        {
            Vector2 offset = Vector2.right * 0.55f;
            Vector2 size = new Vector2(groundCheckRadius * 2, 0.8f);
            Vector2 position = (Vector2)transform.position + offset;
            // var obj = Physics2D.OverlapBox(position, size, 0, groundMask);
            // if(obj) Debug.Log(obj.name);
            return Physics2D.OverlapBox(position, size, 0, groundMask) || Physics2D.OverlapBox(position, size, 0, obs);
        }
        private float TouchWall2()
        {
            Vector2 offset = Vector2.right * 0.55f;
            Vector2 size = new Vector2(groundCheckRadius * 2, 0.8f);
            Vector2 position = (Vector2)transform.position + offset;
            // var obj = Physics2D.OverlapBox(position, size, 0, groundMask);
            // if(obj) Debug.Log(obj.name);
            if (Physics2D.OverlapBox(position, size, 0, groundMask))
            {
                return 1;
            }else if (Physics2D.OverlapBox(position, size, 0, obs))
            {
                return 0;
            }
            else
            {
                return 1;
            }
        }

        private void LimitFallSpeed()
        {

            if ((rb.velocity.y  * _gravity) < -24.2f)
            {
                rb.velocity = new Vector2(rb.velocity.x, -24.2f * _gravity);
            }
        }
        public void ChangeThroughPortal(MoveSpeed moveSpeed, GameModes gameMode , Gravity gravity, int state)
        {
            switch (state)
            {
                case 0:
                    currentMoveSpeed = moveSpeed;
                    break;
                case 1:
                    currentGameMode = gameMode;
                    break;
                case 2:
                    _gravity = (int)gravity;
                    rb.gravityScale = Mathf.Abs(rb.gravityScale) * (int)gravity;
                    break;
            }
        }
        
        private void OnDrawGizmosSelected()
        {
            DrawGizmoForOnGrounded();
            DrawGizmoForTouchWall();
        }

        private void DrawGizmoForOnGrounded()
        {
            Vector3 downOffset = Vector3.down * _gravity * 0.5f;
            Vector2 rightSize = Vector2.right * 1.1f;
            Vector2 upSize = Vector2.up * groundCheckRadius;

            Vector3 position = transform.position + downOffset;
            Vector2 size = rightSize + upSize;

            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(position, new Vector3(size.x, size.y, 0.1f));
        }

        private void DrawGizmoForTouchWall()
        {
            Vector2 offset = Vector2.right * 0.55f;
            Vector2 size = new Vector2(groundCheckRadius * 2, 0.8f);
            Vector2 position = (Vector2)transform.position + offset;

            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(position, new Vector3(size.x, size.y, 0.1f));
        }
        
    }
}