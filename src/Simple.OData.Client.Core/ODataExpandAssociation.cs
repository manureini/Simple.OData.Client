using System;
using System.Collections.Generic;
using System.Linq;

namespace Simple.OData.Client
{
    public class ODataExpandAssociation : IEquatable<ODataExpandAssociation>
    {
        public ODataExpandAssociation(string name, string typename = null)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException($"Parameter {nameof(name)} should not be null or empty.", nameof(name));
            Name = name;
            TypeName = typename;
        }

        public string Name { get; }

        public string TypeName { get; }

        public List<ODataExpandAssociation> ExpandAssociations { get; } = new List<ODataExpandAssociation>();

        public List<ODataOrderByColumn> OrderByColumns { get; } = new List<ODataOrderByColumn>();

        public ODataExpression FilterExpression { get; set; }

        private static ODataExpandAssociation GetNextAssociation(ref List<string> items)
        {
            string name;
            string typename = null;

            var firstItem = items.First();

            if (firstItem.Contains(".")) //When it contains a . assume that this is not a propertyname - it's the name of the type wich contains this property
            {
                typename = firstItem;
                items.RemoveAt(0);
                name = items.First();
            }
            else
            {
                name = firstItem;
            }

            items.RemoveAt(0);

            return new ODataExpandAssociation(name, typename);
        }

        public static ODataExpandAssociation From(string association)
        {
            if (string.IsNullOrEmpty(association))
                throw new ArgumentException($"Parameter {nameof(association)} should not be null or empty.", nameof(association));

            var items = association.Split('/').ToList();

            var expandAssociation = GetNextAssociation(ref items);
            var currentAssociation = expandAssociation;

            while (items.Count > 0)
            {
                currentAssociation.ExpandAssociations.Add(GetNextAssociation(ref items));
                currentAssociation = currentAssociation.ExpandAssociations.First();
            }

            return expandAssociation;
        }

        public ODataExpandAssociation Clone()
        {
            var clone = new ODataExpandAssociation(Name, TypeName);
            clone.ExpandAssociations.AddRange(ExpandAssociations.Select(a => a.Clone()));
            clone.FilterExpression = FilterExpression;
            clone.OrderByColumns.AddRange(OrderByColumns);
            return clone;
        }

        public bool Equals(ODataExpandAssociation other)
        {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Name == other.Name && TypeName == other.TypeName;
        }

        public override int GetHashCode()
        {
            int hashCode = 1521996100;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(TypeName);
            return hashCode;
        }
    }
}