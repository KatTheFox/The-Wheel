
    using SecretHistories.Fucine;

    public class KB10SettingTracker:ISettingSubscriber
    {
        public void WhenSettingUpdated(object newValue)
        {
            string rawNV= newValue.ToString().Split('/')[1];
            TheWheel.KB10Str =rawNV[0].ToString().ToUpper()+rawNV.Substring(1);
        }
        public void BeforeSettingUpdated(object newValue)
        {
        }
    }