using UnityEngine;
using HexMap.Input;
using HexMap.Map;

namespace HexMap.Gameplay {
   public class CameraManager : MonoBehaviour {
      public static CameraManager Instance { get; private set; }

      public static bool Locked {
         set {
            Instance.enabled = !value;
         }
      }

      public static void ValidatePosition() {
         Instance.AdjustPosition(0f, 0f);
      }

      [SerializeField] float _StickMinZoom = -250;
      [SerializeField] float _StickMaxZoom = -45;

      [SerializeField] Transform _Stick = default;
      [SerializeField] Transform _Swivel = default;
      [SerializeField] InputReader _InputReader = default;
      [SerializeField] HexGrid _HexGrid = default;

      float zoom = 1f,
         moveSpeed = 250f,
         rotationAngle = 0,
         rotationSpeed = 180,
         rotateInput = 0;
      Vector2 movementInput;

      private void Awake() {
         if (Instance != null) {
            Destroy(Instance);
         }

         Instance = this;
      }

      private void OnEnable() {
         if (_InputReader != null) {
            _InputReader.ZoomCamera += AdjustZoom;
            _InputReader.MoveEvent += MoveCamera;
            _InputReader.RotateEvent += RotateCamera;
         }

         Instance = this;
      }

      private void OnDisable() {
         if (_InputReader != null) {
            _InputReader.ZoomCamera -= AdjustZoom;
            _InputReader.MoveEvent -= MoveCamera;
            _InputReader.RotateEvent -= RotateCamera;
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

            float distance = Mathf.Lerp(_StickMinZoom, _StickMaxZoom, zoom);
            _Stick.localPosition = new Vector3(0f, 0f, distance);
         }
      }

      #endregion

      #region Movement

      private void MoveCamera(Vector2 movement) {
         movementInput = movement;
         Debug.Log(movement);
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

      private Vector3 ClampPosition(Vector3 position) {
         float xMax = (_HexGrid.GetCellCountX() - 0.5f) * (2f * HexMetrics.innerRadius);
         position.x = Mathf.Clamp(position.x, 0f, xMax);

         float zMax = (_HexGrid.GetCellCountZ() - 1) * (1.5f * HexMetrics.outerRadius);
         position.z = Mathf.Clamp(position.z, 0f, zMax);

         return position;
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
   }
}