using UnityEngine;
using HexMap.Input;
using HexMap.Map;

public class CameraManager : MonoBehaviour
{
   [SerializeField] float _StickMinZoom = -250;
   [SerializeField] float _StickMaxZoom = -45;

   [SerializeField] Transform _Stick = default;
   [SerializeField] Transform _Swivel = default;
   [SerializeField] InputReader _InputReader = default;
   [SerializeField] HexGrid _HexGrid = default;

   float zoom = 1f, moveSpeed = 250f;

   void OnEnable()
   {
      if (_InputReader != null)
      {
         _InputReader.ZoomCamera += AdjustZoom;
         _InputReader.MoveEvent += MoveCamera;
         _InputReader.RotateCameraLeft += OnRotateCameraLeft;
         _InputReader.RotateCameraRight += OnRotateCameraRight;
      }
   }

   void OnDisable()
   {
      if (_InputReader != null)
      {
         _InputReader.ZoomCamera -= AdjustZoom;
         _InputReader.MoveEvent -= MoveCamera;
         _InputReader.RotateCameraLeft -= OnRotateCameraLeft;
         _InputReader.RotateCameraRight -= OnRotateCameraRight;
      }
   }

   #region Zoom 

   void AdjustZoom(float delta)
   {
      if (delta != 0)
      {
         zoom = Mathf.Clamp01(zoom + delta);

         Debug.Log(zoom);

         float distance = Mathf.Lerp(_StickMinZoom, _StickMaxZoom, zoom);
         _Stick.localPosition = new Vector3(0f, 0f, distance);
      }
   }

   #endregion

   #region Movement

   void MoveCamera(Vector2 movement)
   {
      float xDelta = movement.x;
      float zDelta = movement.y;
      if (xDelta != 0f || zDelta != 0f)
      {
         AdjustPosition(xDelta, zDelta);
      }
   }

   void AdjustPosition(float xDelta, float zDelta)
   {
      Vector3 direction =
         transform.localRotation * new Vector3(xDelta, 0f, zDelta).normalized;
      float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
      float distance =
         Mathf.Lerp(400, 100, zoom) * damping * Time.deltaTime;

      Vector3 position = transform.localPosition;
      position += direction * distance;
      transform.localPosition = position;
   }

   Vector3 ClampPosition(Vector3 position)
   {
      float xMax =
          (_HexGrid.GetChunkX() * HexMetrics.chunkSizeX - 0.5f) * (2f * HexMetrics.innerRadius);
      position.x = Mathf.Clamp(position.x, 0f, xMax);

      float zMax =
         (_HexGrid.GetChunkZ() * HexMetrics.chunkSizeZ - 1) * (1.5f * HexMetrics.outerRadius);
      position.z = Mathf.Clamp(position.z, 0f, zMax);

      return position;
   }

   #endregion

   #region Rotate

   void OnRotateCameraLeft()
   {
      transform.Rotate(Vector3.up, -90f);
   }

   void OnRotateCameraRight()
   {
      transform.Rotate(Vector3.up, 90f);
   }

   #endregion 
}
