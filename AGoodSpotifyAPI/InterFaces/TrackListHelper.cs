using AGoodSpotifyAPI.Classes;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AGoodSpotifyAPI.InterFaces
{
    public interface ITrackListHelper<T> : IEnumerator<T> where T : ITrack
    {
        public List<T> List { get;  }
    }

    public class TrackListHelper<T> : ITrackListHelper<T> where T : ITrack
    {
        private int _position = -1;
        private bool disposedValue;

        internal TrackListHelper(IEnumerable<T> list) => List = list is null ? new List<T>() : list.ToList();
        internal TrackListHelper() : this(new List<T>()) { }


        public List<T> List { get; private set; } = new List<T>();

        public ITrack Current => List[_position];

        object IEnumerator.Current => Current;

        List<T> ITrackListHelper<T>.List => List;

        T IEnumerator<T>.Current => List[_position];

        public bool MoveNext()
        {
            _position++;
            return _position < List.Count;
        }

        public void Reset()
        {
            _position = -1;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {

                }

                List = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public void AddItem(T t) => List.Add(t);
    }
}
