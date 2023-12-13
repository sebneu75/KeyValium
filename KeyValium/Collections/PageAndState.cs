
namespace KeyValium.Collections
{
    internal struct PageAndState
    {
        /// <summary>
        /// creates a new instance 
        /// the refcount of page will be incremented
        /// </summary>
        /// <param name="page"></param>
        /// <param name="state"></param>
        internal PageAndState(AnyPage page, PageStates state)
        {
            Perf.CallCount();

            _page = page;
            _page?.AddRef();

            State= state;
        }

        internal PageStates State;

        private AnyPage _page;

        internal AnyPage Page
        {
            get
            {
                Perf.CallCount();

                return _page;
            }
            set
            {
                Perf.CallCount();

                value?.AddRef();
                _page?.Dispose();
                _page = value;
            }
        }
    }
}
