using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ScoreManagment : MonoBehaviour
{
    public static ScoreManagment Instance;

    [Header("Puntajes")]
    public int p1Score = 0;
    public int p2Score = 0;
    public int winScore = 5;

    [Header("UI")]
    public Text p1ScoreText;
    public Text p2ScoreText;

    [Header("Bola")]
    public Rigidbody2D ballRb;
    public Transform ballTransform;
    public float ballStartSpeed = 8f;
    public float delayRelanzar = 1.0f;

    private int _nextLaunchDir = 1;
    private bool isGameOver = false;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        ActualizarUI();
    }

    public void AddPointToPlayer(int playerIndex)
    {
        if (isGameOver) return;

        if (playerIndex == 1) p1Score++;
        else if (playerIndex == 2) p2Score++;

        ActualizarUI();

        // ¿alguien ganó?
        if (p1Score >= winScore)
        {
            EndMatch(1);
            return;
        }
        if (p2Score >= winScore)
        {
            EndMatch(2);
            return;
        }

        int dir = (playerIndex == 1) ? +1 : -1;
        _nextLaunchDir = dir;
        CancelInvoke(nameof(RelanzarBola));
        ResetBall();
        Invoke(nameof(RelanzarBola), delayRelanzar);
    }

    void EndMatch(int winnerPlayerIndex)
    {
        isGameOver = true;

        // Detener la bola
        if (ballRb)
        {
            ballRb.velocity = Vector2.zero;
            ballRb.angularVelocity = 0f;
        }

        Debug.Log($"¡Fin del juego! Ganó el Jugador {winnerPlayerIndex}");

    }

    void ActualizarUI()
    {
        if (p1ScoreText) p1ScoreText.text = "Player A: " + p1Score;
        if (p2ScoreText) p2ScoreText.text = "Player B: " + p2Score;
    }

    public void ResetBall()
    {
        if (!ballTransform || !ballRb) return;
        ballRb.velocity = Vector2.zero;
        ballRb.angularVelocity = 0f;
        ballTransform.position = Vector3.zero;
    }

    void RelanzarBola()
    {
        if (!ballRb || isGameOver) return;

        float y = Random.Range(-0.4f, 0.4f);
        Vector2 dir = new Vector2(Mathf.Sign(_nextLaunchDir), y).normalized;
        ballRb.velocity = dir * ballStartSpeed;
    }

    public void RestartMatch()
    {
        isGameOver = false;
        p1Score = 0;
        p2Score = 0;
        ActualizarUI();
        ResetBall();
        _nextLaunchDir = Random.value < 0.5f ? -1 : 1;
        Invoke(nameof(RelanzarBola), 0.6f);
    }

    public void LoadMenu(string menuSceneName)
    {
        SceneManager.LoadScene(menuSceneName);
    }
}
