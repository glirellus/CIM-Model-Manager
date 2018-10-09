using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CIMModelManager
{
    internal class MissingDescriptionInfo : IComparable, IComparable<MissingDescriptionInfo>, IEquatable<MissingDescriptionInfo>
    {
        public string Type { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        public int CompareTo(object obj)
        {
            MissingDescriptionInfo other = obj as MissingDescriptionInfo;
            if (other == null)
            {
                return 1;
            }

            return CompareTo(other);
        }

        public int CompareTo(MissingDescriptionInfo other)
        {
            int result = Type.CompareTo(other.Type);
            if (result == 0)
            {
                result = Path.CompareTo(other.Path);
                if (result == 0)
                {
                    result = Name.CompareTo(other.Name);
                }
            }
            return result;
        }

        public bool Equals(MissingDescriptionInfo other)
        {
            return Type == other.Type && Path == other.Path && Name == other.Name;
        }
    }
}