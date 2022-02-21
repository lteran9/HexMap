using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexMap.Input;

public class CameraManager : MonoBehaviour
{
   [SerializeField] int activeCamIdx = default;

   [SerializeField] InputReader inputReader = default;

   [SerializeField] CinemachineVirtualCamera[] VCams = default;

   void OnEnable()
   {
      if (VCams.Length > 0)
      {
         for (int i = 0; i < VCams.Length; i++)
         {
            VCams[i].enabled = false;
         }
      }
   }

   // Start is called before the first frame update
   void Start()
   {

   }

   // Update is called once per frame
   void Update()
   {
      //
   }

   public void RotateCameraLeft()
   {
      var oldIdx = activeCamIdx;
      activeCamIdx++;
      VCams[oldIdx].enabled = false;
   }

   public void RotateCameraRight()
   {
      var oldIdx = activeCamIdx;
      activeCamIdx--;

      if (activeCamIdx < 0)
      {
         activeCamIdx = VCams.Length;
      }

      VCams[oldIdx].enabled = false;
   }
}
