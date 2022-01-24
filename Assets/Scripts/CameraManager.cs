using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HexMap.Input;

public class CameraManager : MonoBehaviour
{
   [SerializeField] InputReader inputReader = default;

   // Start is called before the first frame update
   void Start()
   {
      if (inputReader != null)
      {
         inputReader.onMove += onMoveInputReceived;
      }
      else
      {
         Debug.Log("Input Reader reference has not been established.");
      }
   }

   // Update is called once per frame
   void Update()
   {

   }

   void onMoveInputReceived(Vector2 direction)
   {
      Debug.Log(direction);
   }
}
