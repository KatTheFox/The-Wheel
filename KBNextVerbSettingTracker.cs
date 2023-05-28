
    using SecretHistories.Fucine;

    public class KBNextVerbSettingTracker:ISettingSubscriber
    {
        public void WhenSettingUpdated(object newValue)
        {
            string rawNV= newValue.ToString().Split('/')[1];
            TheWheel.KBNextVerb =rawNV[0].ToString().ToUpper()+rawNV.Substring(1);
        }

        public void BeforeSettingUpdated(object newValue)
        {
        }
    }
