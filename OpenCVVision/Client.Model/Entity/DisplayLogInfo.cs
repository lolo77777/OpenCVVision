using DynamicData.Binding;

namespace Client.Model.Entity
{
    public class DisplayLogInfo : AbstractNotifyPropertyChanged
    {
        public DisplayLogInfo(int id, string msg, string datetime, string level)
        {
            Id = id;
            Msg = msg;
            Time = datetime;
            Level = level;
        }

        public DisplayLogInfo(LogData logData)
        {
            Id = logData.Id;
            Msg = logData.Msg;
            Time = logData.Time.ToShortTimeString();
            Level = logData.LogLevel.ToString();
        }

        private int _id = 0;

        public int Id
        {
            get => _id;
            set => SetAndRaise(ref _id, value);
        }

        private string _msg = string.Empty;

        public string Msg
        {
            get => _msg;
            set => SetAndRaise(ref _msg, value);
        }

        private string _dateTime = string.Empty;

        public string Time
        {
            get { return _dateTime; }
            set => SetAndRaise(ref _dateTime, value);
        }

        private string _level = string.Empty;

        public string Level
        {
            get => _level;
            set => SetAndRaise(ref _level, value);
        }
    }
}