using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.Layouts;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public class MouseControlComposite : InputBindingComposite<Vector2>
{
    [InputControl(layout = "Button")] public int enable;
    [InputControl(layout = "Axis")] public int up;
    [InputControl(layout = "Axis")] public int down;
    [InputControl(layout = "Axis")] public int left;
    [InputControl(layout = "Axis")] public int right;
    
    public Mode mode;

    public override Vector2 ReadValue(ref InputBindingCompositeContext context)
    {
        if (!context.ReadValue<bool>(enable)) return Vector2.zero;
        
        var mode = this.mode;

        if (mode == Mode.Analog)
        {
            var upValue = context.ReadValue<float>(up);
            var downValue = context.ReadValue<float>(down);
            var leftValue = context.ReadValue<float>(left);
            var rightValue = context.ReadValue<float>(right);

            return DpadControl.MakeDpadVector(upValue, downValue, leftValue, rightValue);
        }

        var upIsPressed = context.ReadValueAsButton(up);
        var downIsPressed = context.ReadValueAsButton(down);
        var leftIsPressed = context.ReadValueAsButton(left);
        var rightIsPressed = context.ReadValueAsButton(right);

        return DpadControl.MakeDpadVector(upIsPressed, downIsPressed, leftIsPressed, rightIsPressed,
            mode == Mode.DigitalNormalized);
    }
    
    public override float EvaluateMagnitude(ref InputBindingCompositeContext context)
    {
        var value = ReadValue(ref context);
        return value.magnitude;
    }
  
    public enum Mode
    {
        Analog = 2,
        DigitalNormalized = 0,
        Digital = 1
    }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Init() {} 
}