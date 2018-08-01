using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Graphix
{
    public class Status
    {
        public string Name { get; set; }

        public List<Status> SubStatus { get; private set; }

        public Status Parent { get; set; }

        public override bool Equals(object obj)
        {
            if (obj is Status) return ((Status)obj) == this;
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return (Parent != null ? Parent.ToString() + '|' : "") + Name;
        }

        public Status(string name = null)
        {
            Name = name;
            SubStatus = new List<Status>();
        }

        public Status Find(params string[] names)
        {
            if (names.Length == 0) return null;
            if (names[0] != Name) return null;
            var status = this;
            for (int i = 1; i<names.Length; ++i)
            {
                status = status.SubStatus.Find((s) => s.Name == names[i]);
                if (status == null) return null;
            }
            return status;
        }

        public static bool operator ==(Status s1, Status s2)
        {
            if (((object)s1) == null || ((object)s2) == null)
                return Object.Equals(s1, s2);
            if (s1.Name != s2.Name) return false;
            return s1?.Parent == s2?.Parent;
        }

        public static bool operator !=(Status s1, Status s2)
        {
            if (((object)s1) == null || ((object)s2) == null)
                return !Object.Equals(s1, s2);
            if (s1.Name == s2.Name) return false;
            return s1?.Parent != s2?.Parent;
        }

        public static bool IsSubsetOrEqualFrom(Status current, Status target)
        {
            if (current == null) return true;
            if (target == null) return false;
            var cs = new Stack<string>();
            var ts = new Stack<string>();
            for (var s = current; s != null; s = s.Parent) cs.Push(s.Name);
            for (var s = target; s != null; s = s.Parent) ts.Push(s.Name);
            if (cs.Count > ts.Count) return false;
            while (cs.Count > 0)
                if (cs.Pop() != ts.Pop()) return false;
            return true;
        }

        public static bool HasSameBasePath(Status current, Status target, int minLength = 1)
        {
            if (current == null && target == null) return false;
            if (current == null || target == null) return true;
            if (minLength < 0) throw new ArgumentOutOfRangeException("minLength");
            var cs = new Stack<string>();
            var ts = new Stack<string>();
            for (var s = current; s != null; s = s.Parent) cs.Push(s.Name);
            for (var s = target; s != null; s = s.Parent) ts.Push(s.Name);
            if (minLength > cs.Count || minLength > ts.Count) return false;
            while (cs.Count > 0 && ts.Count > 0 && minLength > 0)
                if (cs.Pop() != ts.Pop()) return false;
                else minLength--;
            return true;
        }
    }
}
