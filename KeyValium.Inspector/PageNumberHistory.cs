using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KeyValium.Inspector
{
    internal class PageNumberHistory
    {
        public PageNumberHistory()
        {
        }

        private List<KvPagenumber> _pagenos = new List<KvPagenumber>();

        private int _current = -1;

        public void Clear()
        {
            _current = -1;
            _pagenos.Clear();
        }

        public void Add(KvPagenumber pageno)
        {
            if (_current >= 0)
            {
                if (pageno != _pagenos[_current])
                {
                    if (_current < _pagenos.Count - 1)
                    {
                        _pagenos.RemoveRange(_current + 1, _pagenos.Count - _current -1);
                    }

                    _pagenos.Add(pageno);
                    _current = _pagenos.Count - 1;
                }
            }
            else
            {
                _pagenos.Add(pageno);
                _current = _pagenos.Count - 1;
            }
        }

        public bool CanMoveForward()
        {
            return _current < _pagenos.Count - 1;
        }

        public bool CanMoveBackward()
        {
            return _current > 0;
        }

        public KvPagenumber? MoveForward()
        {
            if (CanMoveForward())
            {
                _current++;
                return _pagenos[_current];
            }

            return null;
        }

        public KvPagenumber? MoveBackward()
        {
            if (CanMoveBackward())
            {
                _current--;
                return _pagenos[_current];
            }

            return null;
        }
    }
}
