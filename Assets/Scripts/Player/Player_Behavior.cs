using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Behavior : MonoBehaviour
{
    [SerializeField] private Player_Stat _stat;
    [SerializeField] private Animator _anim;

    private Rigidbody2D _rb;
    private Transform _transform;
    private Collider2D _collider;

    private float _moveInput;
    private bool _isOnGround;
    private bool _isSecondJump;
    private float _groundCheckDistance = 1f;

    private int _health;

    private float _attackCoolDown;


    /* Monobehavior methods */
    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _transform = transform;
        _collider = this.GetComponent<Collider2D>();
        _health = _stat._maxHealth;
    }
    private void Update()
    {
        _attackCoolDown += Time.deltaTime;
        Move();
    }
    private void OnCollisionEnter2D(Collision2D collision)
    {
        GroundCheck();
    }


    /* Movement handlers */
    private void Move()
    {
        float xSpeed = Mathf.MoveTowards(_rb.velocity.x, _stat._moveSpeed * _moveInput, _stat._accelerationSpeed * Time.fixedDeltaTime);
        _rb.velocity = Vector2.right * xSpeed + Vector2.up * _rb.velocity.y;
    }
    private void Jump()
    {
        if (_isOnGround || !_isSecondJump)
        {
            if (!_isOnGround) 
                _isSecondJump = true;

            _rb.velocity = _rb.velocity - _rb.velocity.y * Vector2.up;  // Reset y velocity
            _rb.AddForce(Vector2.up * _stat._jumpForce, ForceMode2D.Impulse);
            _anim.SetTrigger("jump");
            _anim.SetBool("onGround", false);
            _isOnGround = false;
        }
    }
    private void GroundCheck()
    {
        RaycastHit2D hit = Physics2D.Raycast(_transform.position, Vector2.down, _groundCheckDistance, LayerMask.GetMask("Ground"));
        if (hit.collider != null)
        {
            _isOnGround = true;
            _isSecondJump = false;
            _anim.SetBool("onGround", true);
        }
    }


    /* Health Handlers */
    public void TakeDamage(int value, Transform other)
    {
        _health += value;
        _anim.SetTrigger("hurt");
        HurtEffect();   // Not Implement yet

        if (_health <= 0) Die();
    }
    private void HurtEffect()
    {
        /* Implement hurt mechanic here */

    }
    private void Die()
    {
        this.gameObject.SetActive(false);
    }


    /* Attack Handlers */
    private void AttackAnimation()
    {
        int randomAttackAnimation = Random.Range(0, 2);

        if (_moveInput == 0)
            _anim.SetTrigger(randomAttackAnimation == 1 ? "attack1" : "attack2");                  
            _anim.SetTrigger(randomAttackAnimation == 1 ? "walkAttack1" : "walkAttack2");   
    }
    private void Attack()
    {
        int facingDir = (int)_transform.localScale.x;
        Vector3 hitPos = _transform.position + Vector3.right * _stat._attackRange / 2 * facingDir;
        Collider2D[] hit = Physics2D.OverlapCircleAll(hitPos, _stat._attackRange/2, LayerMask.GetMask("Duck"));

        if (hit.Length != 0)
            foreach (Collider2D enemy in hit)
                enemy.GetComponent<Enemy_Behavior>().TakeDamage(-1, _transform);

    }


    /* Input Handlers => New Input System */
    private void OnMove(InputValue value)  
    {
        _moveInput = value.Get<Vector2>().x;
        _anim.SetInteger("moveInput", (int)_moveInput);
        if (_moveInput != 0) _transform.localScale = new Vector3((int)_moveInput, 1, 1);    // Flip player to direction
    }
    private void OnJump()
    {
        Jump();
    }
    private void OnAttack()
    {
        if (_attackCoolDown >= _stat._attackCooldown)
        {
            _attackCoolDown = 0;
            Attack();   // Not implement yet
            AttackAnimation();
        }
    }
}