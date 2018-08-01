using System;
using System.Collections.Generic;

namespace Graphix
{
    /// <summary>
    /// The Status represents the State of the execution and ui
    /// </summary>
    public class Status
    {
        /// <summary>
        /// The Name of this Status
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// A List of subsequent <see cref="Status"/> that describe this status better
        /// </summary>
        public List<Status> SubStatus { get; private set; }

        /// <summary>
        /// The parent <see cref="Status"/>
        /// </summary>
        public Status Parent { get; set; }

        /// <summary>
        /// Compares if an object equals this object
        /// </summary>
        /// <param name="obj">other object</param>
        /// <returns>comparison result</returns>
        public override bool Equals(object obj)
        {
            if (obj is Status) return ((Status)obj) == this;
            return base.Equals(obj);
        }

        /// <summary>
        /// Calculates the hash code of this status
        /// </summary>
        /// <returns>hash code</returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// Convert this Status to a string representation
        /// </summary>
        /// <returns>string representation</returns>
        public override string ToString()
        {
            return (Parent != null ? Parent.ToString() + '|' : "") + Name;
        }

        /// <summary>
        /// Create a new status
        /// </summary>
        /// <param name="name">name of this status</param>
        public Status(string name = null)
        {
            Name = name;
            SubStatus = new List<Status>();
        }

        /// <summary>
        /// Find a status. (First key need to be this name)
        /// </summary>
        /// <param name="names">keys to the status</param>
        /// <returns>found status object</returns>
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

        /// <summary>
        /// Compare to instances of Status
        /// </summary>
        /// <param name="s1">first status</param>
        /// <param name="s2">second status</param>
        /// <returns>comparison result</returns>
        public static bool operator ==(Status s1, Status s2)
        {
            if (((object)s1) == null || ((object)s2) == null)
                return Object.Equals(s1, s2);
            if (s1.Name != s2.Name) return false;
            return s1?.Parent == s2?.Parent;
        }

        /// <summary>
        /// Compare to instances of Status
        /// </summary>
        /// <param name="s1">first status</param>
        /// <param name="s2">second status</param>
        /// <returns>comparison result</returns>
        public static bool operator !=(Status s1, Status s2)
        {
            if (((object)s1) == null || ((object)s2) == null)
                return !Object.Equals(s1, s2);
            if (s1.Name == s2.Name) return false;
            return s1?.Parent != s2?.Parent;
        }

        /// <summary>
        /// Checks of the current status is a subsequent or equal to the target
        /// </summary>
        /// <param name="current">the current value (null -> true)</param>
        /// <param name="target">the parent or same status (null -> false)</param>
        /// <returns>if the current is subsequent or equal</returns>
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

        /// <summary>
        /// checks if the root of both status is partialy the same.
        /// </summary>
        /// <param name="current">the current status</param>
        /// <param name="target">the target status</param>
        /// <param name="minLength">the minum length of the root that has to be equal</param>
        /// <returns>both Status has the same root</returns>
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
