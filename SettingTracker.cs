using SecretHistories.Entities;
using SecretHistories.Fucine;
using SecretHistories.UI;

// Generic setting tracker, thanks to RobynTheDevil on Steam
public class SettingTracker<T> : ISettingSubscriber
{
  public T current { get; protected set; }

  public string settingId { get; protected set; }

  public TrackerUpdate<T> whenUpdated { get; protected set; }

  public TrackerUpdate<T> beforeUpdated { get; protected set; }

  public SettingTracker(
    string settingId,
    TrackerUpdate<T> whenUpdated = null,
    TrackerUpdate<T> beforeUpdated = null,
    bool start = true)
  {
    this.settingId = settingId;
    this.whenUpdated = whenUpdated;
    this.beforeUpdated = beforeUpdated;
    if (!start)
      return;
    this.Start();
  }

  public void Start()
  {
    var entityById = Watchman.Get<Compendium>().GetEntityById<Setting>(this.settingId);
    if (entityById == null)
    {
      NoonUtility.LogWarning($"Setting Missing: {(object)this.settingId}");
      return;
    }

    entityById.AddSubscriber((ISettingSubscriber) this);
    WhenSettingUpdated(entityById.CurrentValue);
  }

  public virtual void SetCurrent(object newValue)
  {
    try
    {
      this.current = (T) newValue;
    }
    catch
    {
      NoonUtility.LogWarning($"SettingTracker {(object)this.settingId}: Unable to set current to {newValue}");
    }
  }

  public virtual void CallWhen()
  {
    this.whenUpdated?.Invoke(this);
  }

  public virtual void CallBefore()
  {
    this.beforeUpdated?.Invoke(this);
  }

  public virtual void WhenSettingUpdated(object newValue)
  {
    this.SetCurrent(newValue);
    this.CallWhen();
  }

  public virtual void BeforeSettingUpdated(object newValue) => this.CallBefore();
}
