using UnityEngine;

public class ValueTracker<T> : SettingTracker<T>
{
    public T[] values { get; set; }
    public ValueTracker(    
        string settingId,
        T[] values,
        TrackerUpdate<T> whenUpdated = null,
        TrackerUpdate<T> beforeUpdated = null,
        bool start = true)
        : base(settingId, whenUpdated, beforeUpdated, false)
    {
        this.values = values;
        if (!start)
            return;
        this.Start();
    }

    public override void SetCurrent(object newValue)
    {
        if (!(newValue is int a))
            a = 1;
        this.current = this.values[Mathf.Min(this.values.Length - 1, Mathf.Max(a, 0))];
    }
}