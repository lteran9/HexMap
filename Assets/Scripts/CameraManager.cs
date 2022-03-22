using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexMap.Input;

public class CameraManager : MonoBehaviour
{
   [SerializeField] Transform stick = default;
   [SerializeField] Transform swivel = default;
   [SerializeField] InputReader inputReader = default;

   float zoom = 1f;

   void OnEnable()
   {
      if (inputReader != null)
         inputReader.ZoomCamera += AdjustZoom;
   }

   void OnDisable()
   {
      if (inputReader != null)
         inputReader.ZoomCamera -= AdjustZoom;
   }

   // Start is called before the first frame update
   void Start()
   {
      //
   }

   // Update is called once per frame
   void Update()
   {

   }

   void AdjustZoom(float delta)
   {
      zoom = Mathf.Clamp01(zoom + delta);
   }
}
