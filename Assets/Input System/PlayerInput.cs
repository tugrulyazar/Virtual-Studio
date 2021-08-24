// GENERATED AUTOMATICALLY FROM 'Assets/Input System/PlayerInput.inputactions'

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Utilities;

public class @PlayerInput : IInputActionCollection, IDisposable
{
    public InputActionAsset asset { get; }
    public @PlayerInput()
    {
        asset = InputActionAsset.FromJson(@"{
    ""name"": ""PlayerInput"",
    ""maps"": [
        {
            ""name"": ""Player"",
            ""id"": ""c5a598cf-983b-424d-8345-5baa7a7a51ba"",
            ""actions"": [
                {
                    ""name"": ""Move"",
                    ""type"": ""Value"",
                    ""id"": ""e6db7ca5-e21d-4174-a4e2-48741db1042a"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Look"",
                    ""type"": ""Value"",
                    ""id"": ""0724f89e-1f26-47d0-80fa-bc1c6336054f"",
                    ""expectedControlType"": ""Vector2"",
                    ""processors"": """",
                    ""interactions"": """"
                },
                {
                    ""name"": ""Jump"",
                    ""type"": ""Button"",
                    ""id"": ""32794b02-f8df-4ab8-9e2c-27a4f9fd3660"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press""
                },
                {
                    ""name"": ""Run"",
                    ""type"": ""Button"",
                    ""id"": ""9246bac9-36e5-4c83-80aa-71d72d4cc66b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press""
                },
                {
                    ""name"": ""Toggle Camera"",
                    ""type"": ""Button"",
                    ""id"": ""b8be1f85-d988-4980-bca1-1172d069eea6"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press""
                },
                {
                    ""name"": ""Toggle Shoulder"",
                    ""type"": ""Button"",
                    ""id"": ""30bee7d7-2481-4b61-884f-ddfb5129b28a"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press""
                },
                {
                    ""name"": ""Ascend"",
                    ""type"": ""Value"",
                    ""id"": ""c0c9c13a-42c4-47bf-a615-b5d20939cefd"",
                    ""expectedControlType"": ""Axis"",
                    ""processors"": """",
                    ""interactions"": ""Press""
                },
                {
                    ""name"": ""Toggle Flight"",
                    ""type"": ""Button"",
                    ""id"": ""98149652-4751-4fff-bf4c-01cacc6e5f84"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press""
                },
                {
                    ""name"": ""Wave"",
                    ""type"": ""Button"",
                    ""id"": ""28080b31-1392-4fee-8ded-7accdfb8358b"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press""
                },
                {
                    ""name"": ""Dance"",
                    ""type"": ""Button"",
                    ""id"": ""b6c3c63d-f947-4d18-9e55-5a3e1fb6ce51"",
                    ""expectedControlType"": ""Button"",
                    ""processors"": """",
                    ""interactions"": ""Press""
                }
            ],
            ""bindings"": [
                {
                    ""name"": ""2D Vector"",
                    ""id"": ""f2353e93-ae4a-4f94-9f4a-4bdc7ed992b3"",
                    ""path"": ""2DVector"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Move"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""up"",
                    ""id"": ""1bc9b7cc-12b6-4ec8-8a19-dd6b7a173c71"",
                    ""path"": ""<Keyboard>/w"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""down"",
                    ""id"": ""43f3873e-f029-4821-8d0a-7125fed90703"",
                    ""path"": ""<Keyboard>/s"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""left"",
                    ""id"": ""09b8c2bc-3b62-4e66-8f44-e4bafc16c391"",
                    ""path"": ""<Keyboard>/a"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""right"",
                    ""id"": ""b82640cf-d7ed-4ae9-b5ea-e57aeae1b1f5"",
                    ""path"": ""<Keyboard>/d"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Move"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""31571e92-f09d-49a0-bc17-ea1d6c757c43"",
                    ""path"": ""<Mouse>/delta"",
                    ""interactions"": """",
                    ""processors"": ""InvertVector2(invertX=false),ScaleVector2(x=15,y=15)"",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Look"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""9f3657a1-d513-40ad-8d51-306f877562ce"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Jump"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""c334f78d-1785-4ce4-a0b8-d788b7ca25ce"",
                    ""path"": ""<Keyboard>/leftShift"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Run"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""1438f4e3-4caa-4d90-8773-ae979531a152"",
                    ""path"": ""<Keyboard>/f"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Toggle Camera"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""1D Axis"",
                    ""id"": ""d719b6cb-105c-446b-8031-d7a165cd83ed"",
                    ""path"": ""1DAxis"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": """",
                    ""action"": ""Ascend"",
                    ""isComposite"": true,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": ""negative"",
                    ""id"": ""eee4e443-0459-4708-835a-565cc1db00a1"",
                    ""path"": ""<Keyboard>/ctrl"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Ascend"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": ""positive"",
                    ""id"": ""43717469-9665-4e7d-87b8-b051f9bfa87e"",
                    ""path"": ""<Keyboard>/space"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Ascend"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": true
                },
                {
                    ""name"": """",
                    ""id"": ""9c5624e4-b09a-4ab0-a88c-85cbd45a708b"",
                    ""path"": ""<Keyboard>/tab"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Toggle Flight"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""efc15876-2da4-48ca-901d-03b59805c9c0"",
                    ""path"": ""<Keyboard>/h"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Wave"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""6694aebb-88c9-4918-bb2e-a82122d52f96"",
                    ""path"": ""<Keyboard>/j"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Dance"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                },
                {
                    ""name"": """",
                    ""id"": ""00d6fce8-e510-4098-a2d3-252d143b3e6c"",
                    ""path"": ""<Keyboard>/r"",
                    ""interactions"": """",
                    ""processors"": """",
                    ""groups"": ""Keyboard + Mouse"",
                    ""action"": ""Toggle Shoulder"",
                    ""isComposite"": false,
                    ""isPartOfComposite"": false
                }
            ]
        }
    ],
    ""controlSchemes"": [
        {
            ""name"": ""Keyboard + Mouse"",
            ""bindingGroup"": ""Keyboard + Mouse"",
            ""devices"": [
                {
                    ""devicePath"": ""<Keyboard>"",
                    ""isOptional"": false,
                    ""isOR"": false
                },
                {
                    ""devicePath"": ""<Mouse>"",
                    ""isOptional"": false,
                    ""isOR"": false
                }
            ]
        }
    ]
}");
        // Player
        m_Player = asset.FindActionMap("Player", throwIfNotFound: true);
        m_Player_Move = m_Player.FindAction("Move", throwIfNotFound: true);
        m_Player_Look = m_Player.FindAction("Look", throwIfNotFound: true);
        m_Player_Jump = m_Player.FindAction("Jump", throwIfNotFound: true);
        m_Player_Run = m_Player.FindAction("Run", throwIfNotFound: true);
        m_Player_ToggleCamera = m_Player.FindAction("Toggle Camera", throwIfNotFound: true);
        m_Player_ToggleShoulder = m_Player.FindAction("Toggle Shoulder", throwIfNotFound: true);
        m_Player_Ascend = m_Player.FindAction("Ascend", throwIfNotFound: true);
        m_Player_ToggleFlight = m_Player.FindAction("Toggle Flight", throwIfNotFound: true);
        m_Player_Wave = m_Player.FindAction("Wave", throwIfNotFound: true);
        m_Player_Dance = m_Player.FindAction("Dance", throwIfNotFound: true);
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
    private readonly InputAction m_Player_Move;
    private readonly InputAction m_Player_Look;
    private readonly InputAction m_Player_Jump;
    private readonly InputAction m_Player_Run;
    private readonly InputAction m_Player_ToggleCamera;
    private readonly InputAction m_Player_ToggleShoulder;
    private readonly InputAction m_Player_Ascend;
    private readonly InputAction m_Player_ToggleFlight;
    private readonly InputAction m_Player_Wave;
    private readonly InputAction m_Player_Dance;
    public struct PlayerActions
    {
        private @PlayerInput m_Wrapper;
        public PlayerActions(@PlayerInput wrapper) { m_Wrapper = wrapper; }
        public InputAction @Move => m_Wrapper.m_Player_Move;
        public InputAction @Look => m_Wrapper.m_Player_Look;
        public InputAction @Jump => m_Wrapper.m_Player_Jump;
        public InputAction @Run => m_Wrapper.m_Player_Run;
        public InputAction @ToggleCamera => m_Wrapper.m_Player_ToggleCamera;
        public InputAction @ToggleShoulder => m_Wrapper.m_Player_ToggleShoulder;
        public InputAction @Ascend => m_Wrapper.m_Player_Ascend;
        public InputAction @ToggleFlight => m_Wrapper.m_Player_ToggleFlight;
        public InputAction @Wave => m_Wrapper.m_Player_Wave;
        public InputAction @Dance => m_Wrapper.m_Player_Dance;
        public InputActionMap Get() { return m_Wrapper.m_Player; }
        public void Enable() { Get().Enable(); }
        public void Disable() { Get().Disable(); }
        public bool enabled => Get().enabled;
        public static implicit operator InputActionMap(PlayerActions set) { return set.Get(); }
        public void SetCallbacks(IPlayerActions instance)
        {
            if (m_Wrapper.m_PlayerActionsCallbackInterface != null)
            {
                @Move.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Move.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Move.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnMove;
                @Look.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnLook;
                @Look.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnLook;
                @Look.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnLook;
                @Jump.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @Jump.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @Jump.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnJump;
                @Run.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRun;
                @Run.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRun;
                @Run.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnRun;
                @ToggleCamera.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnToggleCamera;
                @ToggleCamera.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnToggleCamera;
                @ToggleCamera.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnToggleCamera;
                @ToggleShoulder.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnToggleShoulder;
                @ToggleShoulder.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnToggleShoulder;
                @ToggleShoulder.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnToggleShoulder;
                @Ascend.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAscend;
                @Ascend.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAscend;
                @Ascend.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnAscend;
                @ToggleFlight.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnToggleFlight;
                @ToggleFlight.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnToggleFlight;
                @ToggleFlight.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnToggleFlight;
                @Wave.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnWave;
                @Wave.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnWave;
                @Wave.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnWave;
                @Dance.started -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDance;
                @Dance.performed -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDance;
                @Dance.canceled -= m_Wrapper.m_PlayerActionsCallbackInterface.OnDance;
            }
            m_Wrapper.m_PlayerActionsCallbackInterface = instance;
            if (instance != null)
            {
                @Move.started += instance.OnMove;
                @Move.performed += instance.OnMove;
                @Move.canceled += instance.OnMove;
                @Look.started += instance.OnLook;
                @Look.performed += instance.OnLook;
                @Look.canceled += instance.OnLook;
                @Jump.started += instance.OnJump;
                @Jump.performed += instance.OnJump;
                @Jump.canceled += instance.OnJump;
                @Run.started += instance.OnRun;
                @Run.performed += instance.OnRun;
                @Run.canceled += instance.OnRun;
                @ToggleCamera.started += instance.OnToggleCamera;
                @ToggleCamera.performed += instance.OnToggleCamera;
                @ToggleCamera.canceled += instance.OnToggleCamera;
                @ToggleShoulder.started += instance.OnToggleShoulder;
                @ToggleShoulder.performed += instance.OnToggleShoulder;
                @ToggleShoulder.canceled += instance.OnToggleShoulder;
                @Ascend.started += instance.OnAscend;
                @Ascend.performed += instance.OnAscend;
                @Ascend.canceled += instance.OnAscend;
                @ToggleFlight.started += instance.OnToggleFlight;
                @ToggleFlight.performed += instance.OnToggleFlight;
                @ToggleFlight.canceled += instance.OnToggleFlight;
                @Wave.started += instance.OnWave;
                @Wave.performed += instance.OnWave;
                @Wave.canceled += instance.OnWave;
                @Dance.started += instance.OnDance;
                @Dance.performed += instance.OnDance;
                @Dance.canceled += instance.OnDance;
            }
        }
    }
    public PlayerActions @Player => new PlayerActions(this);
    private int m_KeyboardMouseSchemeIndex = -1;
    public InputControlScheme KeyboardMouseScheme
    {
        get
        {
            if (m_KeyboardMouseSchemeIndex == -1) m_KeyboardMouseSchemeIndex = asset.FindControlSchemeIndex("Keyboard + Mouse");
            return asset.controlSchemes[m_KeyboardMouseSchemeIndex];
        }
    }
    public interface IPlayerActions
    {
        void OnMove(InputAction.CallbackContext context);
        void OnLook(InputAction.CallbackContext context);
        void OnJump(InputAction.CallbackContext context);
        void OnRun(InputAction.CallbackContext context);
        void OnToggleCamera(InputAction.CallbackContext context);
        void OnToggleShoulder(InputAction.CallbackContext context);
        void OnAscend(InputAction.CallbackContext context);
        void OnToggleFlight(InputAction.CallbackContext context);
        void OnWave(InputAction.CallbackContext context);
        void OnDance(InputAction.CallbackContext context);
    }
}
