using UnityEngine;
using HexMap.Input;
using HexMap.Map;
using HexMap.Map.Grid;

namespace HexMap.Gameplay {
   public class CameraManager : MonoBehaviour {
      public static CameraManager Instance { get; private set; }

      [SerializeField] private float _stickMinZoom = -250;
      [SerializeField] private float _StickMaxZoom = -45;

      [SerializeField] private Transform _stick = default;
      //[SerializeField] Transform _swivel = default;
      [SerializeField] private InputReader _inputReader = default;

      private float zoom = 1f,
         //moveSpeed = 250f,
         rotationAngle = 0,
         rotationSpeed = 180,
         rotateInput = 0;
      private Vector2 movementInput;

      private void Awake() {
         if (Instance != null) {
            Destroy(Instance);
         }

         Instance = this;
      }

      private void OnEnable() {
         if (_inputReader != null) {
            _inputReader.ZoomCamera += AdjustZoom;
            _inputReader.MoveEvent += MoveCamera;
            _inputReader.RotateEvent += RotateCamera;
         }
      }

      private void OnDisable() {
         if (_inputReader != null) {
            _inputReader.ZoomCamera -= AdjustZoom;
            _inputReader.MoveEvent -= MoveCamera;
            _inputReader.RotateEvent -= RotateCamera;
         }
      }

      private void LateUpdate() {
         float xDelta = movementInput.x;
         float zDelta = movementInput.y;

         if (xDelta != 0f || zDelta != 0f) {
            AdjustPosition(xDelta, zDelta);
         }

         if (rotateInput != 0f) {
            AdjustRotation(rotateInput);
         }
      }

      #region Zoom 

      private void AdjustZoom(float delta) {
         if (delta != 0) {
            zoom = Mathf.Clamp01(zoom + delta);

            float distance = Mathf.Lerp(_stickMinZoom, _StickMaxZoom, zoom);
            _stick.localPosition = new Vector3(0f, 0f, distance);
         }
      }

      #endregion

      #region Movement

      private void MoveCamera(Vector2 movement) {
         movementInput = movement;
      }

      private void AdjustPosition(float xDelta, float zDelta) {
         Vector3 direction =
            transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
         float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
         float distance =
            Mathf.Lerp(400, 100, zoom) * damping * Time.deltaTime;

         Vector3 position = transform.localPosition;
         position += direction * distance;
         transform.localPosition = position;
      }

      #endregion

      #region Rotate

      private void RotateCamera(float movement) {
         rotateInput = movement;
      }

      private void AdjustRotation(float delta) {
         rotationAngle += delta * rotationSpeed * Time.deltaTime;
         if (rotationAngle < 0f) {
            rotationAngle += 360f;
         } else if (rotationAngle >= 360f) {
            rotationAngle -= 360f;
         }
         transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
      }

      #endregion

      public void Lock(bool value) {
         Instance.enabled = !value;
      }
   }
}