using System;
using UnityEngine.InputSystem;

public class KeybindTracker : SettingTracker<string>
{
    public InputAction inputAction;

    public override void CallWhen()
    {
        inputAction?.ChangeBinding(0).WithPath(current);
    }

    public KeybindTracker(
        string settingId,
        Action<InputAction.CallbackContext> onPerformed,
        TrackerUpdate<string> whenUpdated = null,
        TrackerUpdate<string> beforeUpdated = null,
        bool start = true)
        : base(settingId, whenUpdated, beforeUpdated, false)
    {
        if (!start)
            return;
        Start();
        inputAction = new InputAction(settingId,binding:current);
        inputAction.performed += onPerformed;
        inputAction.Enable();
        
    }
    public override void SetCurrent(object newValue)
    {
        current = (string) newValue;
    }
}