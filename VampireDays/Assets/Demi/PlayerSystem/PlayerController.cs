using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("移動")]
    [Tooltip("スキル補正前の基本移動速度")]
    public float moveSpeed = 5f;

    [Header("入力")]
    public InputActionReference moveAction;


    //==================================================
    // コンポーネント
    //==================================================

    private Rigidbody rb;

    private MoveSpeedUpSkill moveSpeedUpSkill;


    //==================================================
    // 入力
    //==================================================

    private Vector2 moveInput;


    //==================================================
    // Unity
    //==================================================

    private void Awake()
    {
        rb =
            GetComponent<Rigidbody>();

        rb.useGravity = false;
        rb.isKinematic = true;

        moveSpeedUpSkill =
            GetComponent<MoveSpeedUpSkill>();
    }


    private void OnEnable()
    {
        moveAction.action.Enable();
    }


    private void OnDisable()
    {
        moveAction.action.Disable();
    }


    private void Update()
    {
        moveInput =
            moveAction.action.ReadValue<Vector2>();
    }


    private void FixedUpdate()
    {
        Vector3 move =
            new Vector3(
                moveInput.x,
                0f,
                moveInput.y
            );

        //==================================================
        // 移動速度計算
        //==================================================

        float currentMoveSpeed =
            moveSpeed;

        if (moveSpeedUpSkill != null)
        {
            currentMoveSpeed =
                moveSpeedUpSkill.CalculateMoveSpeed(
                    moveSpeed
                );
        }

        //==================================================
        // 移動
        //==================================================

        rb.MovePosition(
            rb.position +
            move *
            currentMoveSpeed *
            Time.fixedDeltaTime
        );
    }
}