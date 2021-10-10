using AGoodSpotifyAPI.InterFaces;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics.CodeAnalysis;
using System.Data;

namespace AGoodSpotifyAPI.Classes
{
#pragma warning disable CS0659
    public class TrackList<Track> : IEnumerable<Track>, IDisposable, IList<Track>, ICollection<Track> where Track : ITrack
#pragma warning restore CS0659
    {
        private readonly TrackListHelper<Track> _helper;

        public int Count => _helper.List.Count;

        public bool IsReadOnly { get; set; } = false;

        public TrackList()                                                                     
        {
            _helper = new TrackListHelper<Track>();
        }
        public TrackList(IEnumerable<Track> tracks)
        {
            _helper = new TrackListHelper<Track>(tracks);
        }

       

        public Track this[int index]
        {
            get
            {
                int length = _helper.List.Count;
                if (index < 0 || index >= length) throw new IndexOutOfRangeException();

                return _helper.List[index];
            }

            set
            {
                int length = _helper.List.Count;
                if (index < 0 || index >= length) throw new IndexOutOfRangeException();

                _helper.List[index] = value;
            }
        }

#pragma warning disable
        public virtual async System.Threading.Tasks.Task Save(string userToken, string playlistName)
        {

            throw new NotImplementedException();
        }
#pragma warning restore


        #region Implentation
        public override bool Equals(object obj)
        {
            if (!(obj is TrackList<Track>)) return false;

            var o = obj as TrackList<Track>;

            var li = _helper.List;
            if (li.Count != o.Count) return false;
            var ids1 = (from t in li let id = t.Id orderby id select id).ToList();
            var ids2 = (from t in o let id = t.Id orderby id select id).ToList();

            for (int i = 0; i < ids1.Count; i++)
            {
                if (ids1[i] != ids2[i]) return false;
            }

            return true;
        }

        public void Dispose()
        {
            _helper.Dispose();
        }

        public IEnumerator<Track> GetEnumerator() => _helper;
        IEnumerator IEnumerable.GetEnumerator() => _helper;

        public virtual void Add(Track t)
        {
            if (IsReadOnly) throw new ReadOnlyException();
            _helper.List.Add(t);
        }

        public virtual void AddRange(IEnumerable<Track> tracks)
        {
            if (IsReadOnly) throw new ReadOnlyException();
            foreach (var item in tracks)
            {
                _helper.List.Add(item);
            }
        }

        public int IndexOf(Track item)
        {
            return _helper.List.IndexOf(item);
        }

        public virtual void Insert(int index, Track item)
        {
            if (IsReadOnly) throw new ReadOnlyException();
            _helper.List.Insert(index, item);

        }

        public virtual void RemoveAt(int index)
        {
            if (IsReadOnly) throw new ReadOnlyException();
            _helper.List.RemoveAt(index);
        }

        public virtual void Clear()
        {
            if (IsReadOnly) throw new ReadOnlyException();
            _helper.List.Clear();
        }

        public bool Contains(Track item) => _helper.List.Contains(item);

        public void CopyTo(Track[] array, int arrayIndex)
        {
            _helper.List.CopyTo(array, arrayIndex);
        }

        public virtual bool Remove(Track item) => IsReadOnly ? throw new ReadOnlyException() : _helper.List.Remove(item);

        public bool Equals([AllowNull] TrackList<Track> other)
        {
         /* if (this is null && other is null) return true;
            if (this is null && other != null) return false;
            if (this != null && other is null) return false;      */
            if (this is null || other is null) return this is null && other is null;
            if (other.Count != Count) return false;

            var t1 = (from t in this let id = t.Id orderby id select id).ToList();
            var t2 = (from t in other let id = t.Id orderby id select id).ToList();

            for (int i = 0; i < t1.Count; i++)
                if (t1[i] != t2[i]) 
                    return false;

            return true;
        }
        #endregion
    }
}
