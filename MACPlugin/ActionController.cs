using System;
using UnityEngine;
using Valve.VR;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BlastonCameraBehaviour
{
    class ActionController
    {
        private static ActionController __instance = null;

        public static ActionController Instance
        {
            get
            {
                if (__instance == null) __instance = new ActionController();
                return __instance;
            }
        }

        private ActionController() { }

        public DigitalAction aAction;
        public DigitalAction bAction;
        public DigitalAction xAction;
        public DigitalAction yAction;
        public DigitalAction grabPinchAction;
        public DigitalAction grabGripAction;

        private int lastFrameIndex;
        private List<VRActiveActionSet_t> inputActionSets = new List<VRActiveActionSet_t>();

        public class DigitalAction
        {
            public InputDigitalActionData_t data;
            public ulong handle;
            public bool priorState;
            public bool state;

            public bool IsEnded => !state && priorState != state;
            public bool IsStarted => state && priorState != state;
        }

        public void Initialize()
        {
            RegisterActionSet("/actions/vavscameraplugin");
            grabPinchAction = RegisterDigitalAction("/actions/vavscameraplugin/in/grabpinch");
            grabGripAction = RegisterDigitalAction("/actions/vavscameraplugin/in/grabgrip");
            aAction = RegisterDigitalAction("/actions/vavscameraplugin/in/testa");
            bAction = RegisterDigitalAction("/actions/vavscameraplugin/in/testb");
            xAction = RegisterDigitalAction("/actions/vavscameraplugin/in/testx");
            yAction = RegisterDigitalAction("/actions/vavscameraplugin/in/testy");
        }

        public void Update()
        {
            if (lastFrameIndex == Time.frameCount)
            {
                return;
            }

            lastFrameIndex = Time.frameCount;

            UpdateActionSets();
            UpdateDigitalAction(aAction);
            UpdateDigitalAction(bAction);
            UpdateDigitalAction(xAction);
            UpdateDigitalAction(yAction);
            UpdateDigitalAction(grabGripAction);
            UpdateDigitalAction(grabPinchAction);
        }

        bool RegisterActionSet(string path)
        {
            ulong handle = 0;
            var error = OpenVR.Input.GetActionSetHandle(path, ref handle);

            if (handle != OpenVR.k_ulInvalidActionHandle && error == EVRInputError.None)
            {
                var actionSet = new VRActiveActionSet_t
                {
                    ulActionSet = handle,
                    ulRestrictedToDevice = OpenVR.k_ulInvalidActionSetHandle,
                    nPriority = 0
                };
                inputActionSets.Add(actionSet);
                return true;
            }
            else
            {
                Debug.Log(error);
                return false;
            }
        }

        private DigitalAction RegisterDigitalAction(string name)
        {
            ulong handle = OpenVR.k_ulInvalidActionHandle;
            var error = OpenVR.Input.GetActionHandle(name, ref handle);

            if (handle != OpenVR.k_ulInvalidActionHandle && error == EVRInputError.None)
            {
                var digitalAction = new DigitalAction
                {
                    data = new InputDigitalActionData_t(),
                    handle = handle,
                };
                return digitalAction;
            }
            else
            {
                Debug.Log(error);
                return null;
            }
        }

        private bool UpdateActionSets()
        {
            var error = OpenVR.Input.UpdateActionState(inputActionSets.ToArray(), (uint) Marshal.SizeOf(typeof(VRActiveActionSet_t)));
            if (error == EVRInputError.None)
            {
                return true;
            }
            else
            {
                Debug.Log(error);
                return false;
            }
        }

        private bool UpdateDigitalAction(DigitalAction action)
        {
            var error = OpenVR.Input.GetDigitalActionData(action.handle, ref action.data, (uint) Marshal.SizeOf(typeof(InputDigitalActionData_t)), OpenVR.k_ulInvalidInputValueHandle);
            if (error == EVRInputError.None)
            {
                action.priorState = action.state;
                action.state = action.data.bState;
                return true;
            }
            else
            {
                Debug.Log(error);
                return false;
            }
        }
    }
}
