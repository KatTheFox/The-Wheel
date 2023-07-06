
    using SecretHistories.Fucine;

    public class KBNextCardSettingTracker: ISettingSubscriber
    {
        public void WhenSettingUpdated(object newValue)
        {
            string rawNV= newValue.ToString().Split('/')[1];
            TheWheel.KBNextCard =rawNV[0].ToString().ToUpper()+rawNV.Substring(1);
        }
        public void BeforeSettingUpdated(object newValue)
        {
        }
    }
