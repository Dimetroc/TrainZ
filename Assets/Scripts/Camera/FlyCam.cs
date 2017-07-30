using UnityEngine;
using System.Collections;

public class FlyCam : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private float _rotationSpeed;
    [SerializeField] private float _regularSpeed;
    [SerializeField] private float _speedMultiplier;

    private Vector2 _movementVector;
    private bool _isSpeedBoost;

    private Transform _tr;

    private float _rotationX = 0.0f;
    private float _rotationY = 0.0f;

    private void Start()
    {
        _tr = transform;
    }

    void LateUpdate()
    {
        CheckKeyInput();
        Move();
        Rotate();
    }

    private void CheckKeyInput()
    {
        _movementVector = Vector2.zero;

        if (Input.GetKey(KeyCode.W))
        {
            _movementVector += Vector2.up;
        }

        if (Input.GetKey(KeyCode.A))
        {
            _movementVector += Vector2.left;
        }

        if (Input.GetKey(KeyCode.S))
        {
            _movementVector += Vector2.down;
        }

        if (Input.GetKey(KeyCode.D))
        {
            _movementVector  += Vector2.right;
        }

        _isSpeedBoost = false;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _isSpeedBoost = true;
        }
    }

    private void Move()
    {
        _tr.Translate(new Vector3(_movementVector.x, 0, _movementVector.y)*Time.deltaTime*_regularSpeed*(_isSpeedBoost ? _speedMultiplier : 1.0f));
    }

    private void Rotate()
    {
        _rotationX += Input.GetAxis("Mouse X") * _rotationSpeed * Time.deltaTime;
        _rotationY += Input.GetAxis("Mouse Y") * _rotationSpeed * Time.deltaTime;
        _rotationY = Mathf.Clamp(_rotationY, -90, 90);

        transform.localRotation = Quaternion.AngleAxis(_rotationX, Vector3.up);
        transform.localRotation *= Quaternion.AngleAxis(_rotationY, Vector3.left);
    }

}
