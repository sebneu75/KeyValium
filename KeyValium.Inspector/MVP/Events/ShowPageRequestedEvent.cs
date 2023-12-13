using KeyValium.Inspector;
using Mad.MVP;

namespace KeyValium.Inspector.MVP.Events
{
    internal class ShowPageRequestedEvent : ContextEvent
    {
        public ShowPageRequestedEvent(KvPagenumber pageno)
        {
            PageNumber = pageno;
        }

        public KvPagenumber PageNumber
        {
            get;
            private set;
        }
    }
}
