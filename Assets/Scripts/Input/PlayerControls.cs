// GENERATED AUTOMATICALLY FROM 'Assets/Settings/Input/PlayerControls.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

namespace HexMap.Input
{
    public class @PlayerControls : IInputActionCollection, IDisposable
    {
        public InputActionAsset asset { get; }
        public @PlayerControls()
        {
            asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerControls"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""c9a0076e-6a53-4bcf-9d32-13890a606f11"",
            ""actions"": [
                {
                    ""name"": ""Click"",
                    ""type"": ""Button"",
                    ""id"": ""4e8c77ca-b176-4f33-825c-71634bfa3b9f"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""MouseMove"",
                    ""type"": ""PassThrough"",
                    ""id"": ""b3f1cd0e-1bf1-46b9-a318-598c02247ac1"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""RotateCameraLeft"",
                    ""type"": ""Button"",
                    ""id"": ""1d31c86d-7af6-41f7-8ef0-71d5876d5fdc"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""RotateCameraRight"",
                    ""type"": ""Button"",
                    ""id"": ""0460bdd4-f102-43c3-b9e2-b50d28d1e16d"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Zoom"",
                    ""type"": ""PassThrough"",
                    ""id"": ""da81db53-241c-4a41-98b6-4928f756a5e0"",
                    ""expectedControlType"": """",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""CameraMove"",
                    ""type"": ""PassThrough"",
                    ""id"": ""d3ec747f-0054-4233-b23d-72a0def7fff6"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                }
            ],
            ""bindings"": [
                {
                    ""name"": """",
                    ""id"": ""00bbd5ae-2bd5-440c-ab6f-ab6e8152c1ee"",
                    ""path"": ""<Mouse>/leftButton"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Default"",
                    ""action"": ""Click"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""d4e21ee9-c0d0-4af8-bfa9-2839d24623a5"",
                    ""path"": ""<Mouse>/position"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""MouseMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""364c4402-95f9-40b8-a128-41c655d8f46b"",
                    ""path"": ""<Keyboard>/leftArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Default"",
                    ""action"": ""RotateCameraLeft"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""a1e02107-2e37-4960-b617-f4e9b453afcb"",
                    ""path"": ""<Keyboard>/rightArrow"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Default"",
                    ""action"": ""RotateCameraRight"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""0442b244-207d-445e-99dd-8461bc765606"",
                    ""path"": ""<Mouse>/scroll/y"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Zoom"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""730e9a2f-d1b1-4249-9209-2f077052948a"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": ""ScaleVector2(x=0.6,y=0.8)"",
                    ""groups"": """",
                    ""action"": ""CameraMove"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""2f7bf6bc-f5ee-42b0-8d0a-6b5e17b81f9d"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Default"",
                    ""action"": ""CameraMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""53e0e360-5291-42b0-b9e2-a8f8f5d3b1eb"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Default"",
                    ""action"": ""CameraMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""2a31d236-6ec0-4edc-ada0-e1f413d806ab"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Default"",
                    ""action"": ""CameraMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""8ee79b2c-9a36-4f07-a5cf-9dcc0ef2ed54"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Default"",
                    ""action"": ""CameraMove"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Default"",
            ""bindingGroup"": ""Default"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": true,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
            // Player
            m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
            m_Player_Click = m_Player.FindAction("Click", throwIfNotFound: true);
            m_Player_MouseMove = m_Player.FindAction("MouseMove", throwIfNotFound: true);
            m_Player_RotateCameraLeft = m_Player.FindAction("RotateCameraLeft", throwIfNotFound: true);
            m_Player_RotateCameraRight = m_Player.FindAction("RotateCameraRight", throwIfNotFound: true);
            m_Player_Zoom = m_Player.FindAction("Zoom", throwIfNotFound: true);
            m_Player_CameraMove = m_Player.FindAction("CameraMove", throwIfNotFound: true);
        }

        public void Dispose()
        {
            UnityEngine.Object.Destroy(asset);
        }

        public InputBinding? bindingMask
        {
            get => asset.bindingMask;
            set => asset.bindingMask = value;
        }

        public ReadOnlyArray<InputDevice>? devices
        {
            get => asset.devices;
            set => asset.devices = value;
        }

        public ReadOnlyArray<InputControlScheme> controlSchemes => asset.controlSchemes;

        public bool Contains(InputAction action)
        {
            return asset.Contains(action);
        }

        public IEnumerator<InputAction> GetEnumerator()
        {
            return asset.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Enable()
        {
            asset.Enable();
        }

        public void Disable()
        {
            asset.Disable();
        }

        // Player
        private readonly InputActionMap m_Player;
        private IPlayerActions m_PlayerActionsCallbackInterface;
        private readonly InputAction m_Player_Click;
        private readonly InputAction m_Player_MouseMove;
        private readonly InputAction m_Player_RotateCameraLeft;
        private readonly InputAction m_Player_RotateCameraRight;
        private readonly InputAction m_Player_Zoom;
        private readonly InputAction m_Player_CameraMove;
        public struct PlayerActions
        {
            private @PlayerControls m_Wrapper;
            public PlayerActions(@PlayerControls wrapper) { m_Wrapper = wrapper; }
            public InputAction @Click => m_Wrapper.m_Player_Click;
            public InputAction @MouseMove => m_Wrapper.m_Player_MouseMove;
            public InputAction @RotateCameraLeft => m_Wrapper.m_Player_RotateCameraLeft;
            public InputAction @RotateCameraRight => m_Wrapper.m_Player_RotateCameraRight;
            public InputAction @Zoom => m_Wrapper.m_Player_Zoom;
            public InputAction @CameraMove => m_Wrapper.m_Player_CameraMove;
            public InputActionMap Get() { return m_Wrapper.m_Player; }
            public void Enable() { Get().Enable(); }
            public void Disable() { Get().Disable(); }
            public bool enabled => Get().enabled;
            public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
            public void SetCallbacks(IPlayerActions instance)
            {
                if (m_Wrapper.m_PlayerActionsCallbackInterface != null)
                {
                    @Click.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnClick;
                    @Click.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnClick;
                    @Click.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnClick;
                    @MouseMove.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMouseMove;
                    @MouseMove.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMouseMove;
                    @MouseMove.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMouseMove;
                    @RotateCameraLeft.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRotateCameraLeft;
                    @RotateCameraLeft.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRotateCameraLeft;
                    @RotateCameraLeft.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRotateCameraLeft;
                    @RotateCameraRight.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRotateCameraRight;
                    @RotateCameraRight.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRotateCameraRight;
                    @RotateCameraRight.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRotateCameraRight;
                    @Zoom.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnZoom;
                    @Zoom.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnZoom;
                    @Zoom.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnZoom;
                    @CameraMove.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCameraMove;
                    @CameraMove.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCameraMove;
                    @CameraMove.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnCameraMove;
                }
                m_Wrapper.m_PlayerActionsCallbackInterface = instance;
                if (instance != null)
                {
                    @Click.started += instance.OnClick;
                    @Click.performed += instance.OnClick;
                    @Click.canceled += instance.OnClick;
                    @MouseMove.started += instance.OnMouseMove;
                    @MouseMove.performed += instance.OnMouseMove;
                    @MouseMove.canceled += instance.OnMouseMove;
                    @RotateCameraLeft.started += instance.OnRotateCameraLeft;
                    @RotateCameraLeft.performed += instance.OnRotateCameraLeft;
                    @RotateCameraLeft.canceled += instance.OnRotateCameraLeft;
                    @RotateCameraRight.started += instance.OnRotateCameraRight;
                    @RotateCameraRight.performed += instance.OnRotateCameraRight;
                    @RotateCameraRight.canceled += instance.OnRotateCameraRight;
                    @Zoom.started += instance.OnZoom;
                    @Zoom.performed += instance.OnZoom;
                    @Zoom.canceled += instance.OnZoom;
                    @CameraMove.started += instance.OnCameraMove;
                    @CameraMove.performed += instance.OnCameraMove;
                    @CameraMove.canceled += instance.OnCameraMove;
                }
            }
        }
        public PlayerActions @Player => new PlayerActions(this);
        private int m_DefaultSchemeIndex = -1;
        public InputControlScheme DefaultScheme
        {
            get
            {
                if (m_DefaultSchemeIndex == -1) m_DefaultSchemeIndex = asset.FindControlSchemeIndex("Default");
                return asset.controlSchemes[m_DefaultSchemeIndex];
            }
        }
        public interface IPlayerActions
        {
            void OnClick(InputAction.CallbackContext context);
            void OnMouseMove(InputAction.CallbackContext context);
            void OnRotateCameraLeft(InputAction.CallbackContext context);
            void OnRotateCameraRight(InputAction.CallbackContext context);
            void OnZoom(InputAction.CallbackContext context);
            void OnCameraMove(InputAction.CallbackContext context);
        }
    }
}
