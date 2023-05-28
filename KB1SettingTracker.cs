using SecretHistories.Fucine;

    public class KB1SettingTracker:ISettingSubscriber
    {
        public void WhenSettingUpdated(object newValue)
        {
            string rawNV= newValue.ToString().Split('/')[1];
            TheWheel.KB1Str =rawNV[0].ToString().ToUpper()+rawNV.Substring(1);
        }

        public void BeforeSettingUpdated(object newValue)
        {
        }
    }