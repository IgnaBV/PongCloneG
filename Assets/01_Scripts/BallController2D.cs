using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class BallController2D : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float speed = 8f;               
    [SerializeField] private float serveDelay = 0.75f;       
    [SerializeField, Range(0f, 0.9f)] private float minYRatio = 0.25f;
    [SerializeField] private float maxBounceAngleDeg = 75f; 
    [SerializeField, Range(0.05f, 0.9f)] private float minXRatio = 0.35f; 
    [SerializeField, Range(0f, 0.1f)] private float angleJitter = 0.02f;  

    [Header("Rebote con paddle (player)")]
    [SerializeField, Range(0f, 1f)] private float controlFactor = 0.75f;
    // 0 = rebote físico puro | 1 = dirección totalmente controlada por punto de impacto

    [Tooltip("Al chocar con el paddle, aumenta levemente la velocidad")]
    [SerializeField] private float speedGainOnPaddle = 0.25f;

    private Rigidbody2D rb;
    private Collider2D col;
    private Vector2 normal;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();

        // Recomendado para Pong: sin gravedad y colisiones discretas
        rb.gravityScale = 0f;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private Vector2 EnsureMinX(Vector2 dir, float preferredXSign)
    {
        dir = dir.normalized;
        float sx = Mathf.Sign(preferredXSign == 0 ? (dir.x == 0 ? 1 : dir.x) : preferredXSign);

        float ax = Mathf.Abs(dir.x);
        if (ax < minXRatio)
        {
            // recalcula manteniendo Y y forzando X mínima
            float ySign = Mathf.Sign(dir.y == 0 ? 1 : dir.y);
            float yMag = Mathf.Sqrt(Mathf.Max(0f, 1f - (minXRatio * minXRatio)));
            dir = new Vector2(minXRatio * sx, yMag * ySign);
        }

        dir.x += sx * angleJitter;
        return dir.normalized;
    }
    
    private void OnEnable()
    {
        ResetAndServe();
    }
    public void ResetAndServe()
    {
        StopAllCoroutines();
        transform.position = Vector3.zero;
        rb.velocity = Vector2.zero;
        StartCoroutine(ServeAfterDelay());
    }

    private System.Collections.IEnumerator ServeAfterDelay()
    {
        yield return new WaitForSeconds(serveDelay);
        LaunchRandom();
    }

    private void LaunchRandom()
    {
        int dirX = Random.value < 0.5f ? -1 : 1; 
        float y = Random.Range(-1f, 1f);
        if (Mathf.Abs(y) < minYRatio)
        {
            y = Mathf.Sign(y == 0 ? 1 : y) * minYRatio;
        }

        Vector2 dir = new Vector2(dirX, y).normalized;
        rb.velocity = dir * speed;
    }

    private void FixedUpdate()
    {
        if (rb.velocity.sqrMagnitude > 0.01f)
        {
            float signX = Mathf.Sign(rb.velocity.x == 0 ? 1 : rb.velocity.x);
            Vector2 dir = EnsureMinX(rb.velocity, signX);
            rb.velocity = dir * speed;
        }


    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        string tag = collision.collider.tag;

        if (tag == "wall")
        {
            Vector2 inVel = rb.velocity;
            if (collision.contactCount > 0)
            {
                Vector2 normal = collision.GetContact(0).normal;
                Vector2 reflected = Vector2.Reflect(inVel, normal).normalized * speed;
                rb.velocity = reflected;
            }
            else
            {
                Vector2 reflected = Vector2.Reflect(inVel, normal).normalized;
                reflected = EnsureMinX(reflected, Mathf.Sign(inVel.x)); // conserva el signo en X
                rb.velocity = reflected * speed;

            }
        }
        else if (tag == "Player")
        {
            // Punto de impacto en el paddle
            float paddleHeight = collision.collider.bounds.size.y;
            float yDelta = (transform.position.y - collision.collider.transform.position.y) / (paddleHeight * 0.5f);
            yDelta = Mathf.Clamp(yDelta, -1f, 1f);

            float maxBounceAngleDeg = 75f;
            float maxAngle = maxBounceAngleDeg * Mathf.Deg2Rad;
            float angle = yDelta * maxAngle;

            float xSign = (transform.position.x < collision.collider.transform.position.x) ? -1f : 1f;
            xSign *= -1f;

            Vector2 newDir = new Vector2(Mathf.Cos(angle) * xSign, Mathf.Sin(angle)).normalized;

            newDir = EnsureMinX(newDir, xSign);

            speed += speedGainOnPaddle;
            rb.velocity = newDir * speed;
        }


    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("wallPoint0"))
        {
            ScoreManagment.Instance?.AddPointToPlayer(2);
        }
        else if (other.CompareTag("wallPoint1"))
        {
            ScoreManagment.Instance?.AddPointToPlayer(1);
        }
    }

}
