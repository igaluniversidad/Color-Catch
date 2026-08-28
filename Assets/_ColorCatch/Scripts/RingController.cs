using UnityEngine;
using UnityEngine.InputSystem;

public class RingController : MonoBehaviour
{
    [Header("Configuración de Rotación")]
    [SerializeField] private float rotationSpeed = 25f;

    // Cuadrantes en orden horario: 0 = Top, 1 = Right, 2 = Bottom, 3 = Left
    [SerializeField]
    private GameColor[] quadrantColors = new GameColor[4]
    {
        GameColor.Red,
        GameColor.Blue,
        GameColor.Green,
        GameColor.Yellow
    };

    private float targetZAngle = 0f;
    private int topSectorIndex = 0;

    void Update()
    {
        if (GameManager.Instance == null) return;

        var state = GameManager.Instance.CurrentState;
        if (state != GameManager.GameState.Playing && state != GameManager.GameState.WaitingToStart)
        {
            return;
        }

        HandleInput();
        SmoothRotate();
    }

    private void HandleInput()
    {
        bool pressed = false;
        Vector2 pointerPos = Vector2.zero;

        // Detección táctil (Móvil / Simulador)
        if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
        {
            pressed = true;
            pointerPos = Touchscreen.current.primaryTouch.position.ReadValue();
        }
        // Detección de Mouse (Editor / PC)
        else if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            pressed = true;
            pointerPos = Mouse.current.position.ReadValue();
        }

        if (pressed)
        {
            // Si estábamos esperando el primer toque, arranca el juego inmediatamente
            if (GameManager.Instance.CurrentState == GameManager.GameState.WaitingToStart)
            {
                GameManager.Instance.StartGame();
            }

            // Tap mitad izquierda -> antihorario / Tap mitad derecha -> horario
            if (pointerPos.x < Screen.width * 0.5f)
            {
                RotateLeft();
            }
            else
            {
                RotateRight();
            }
        }
    }

    public void RotateLeft()
    {
        targetZAngle += 90f;
        topSectorIndex = (topSectorIndex + 1) % 4;
        HapticManager.Instance?.TriggerLightFeedback();
        AudioManager.Instance?.PlayRingRotate();
    }

    public void RotateRight()
    {
        targetZAngle -= 90f;
        topSectorIndex = (topSectorIndex - 1 + 4) % 4;
        HapticManager.Instance?.TriggerLightFeedback();
        AudioManager.Instance?.PlayRingRotate();
    }

    private void SmoothRotate()
    {
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetZAngle);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    public GameColor GetTopColor()
    {
        return quadrantColors[topSectorIndex];
    }
}