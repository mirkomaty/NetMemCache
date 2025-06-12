using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMemCache.UnitTests
{
    [Serializable]
    public class Model : IEquatable<Model>
    {
        public string Title { get; set; }
        public string ThumbUrl { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

        public bool Equals(Model other)
        {
            if (other == null) return false;

            return string.Equals(this.Title, other.Title)
                   && string.Equals(this.Description, other.Description)
                   && string.Equals(this.ThumbUrl, other.ThumbUrl)
                   && this.Price.Equals(other.Price);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals(obj as Model);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                // Note: String.GetHashCode gets a different HashCode for each instance of an application process.
                // This is not suitable for serialization or persistence purposes.
                // We use GetConstantHashCode to avoid problems with different HashCodes.
                var hash = (int)15257167;

                hash = (hash * 10786583) ^ (!string.IsNullOrEmpty(this.Title) ? this.Title.GetConstantHashCode() : 0);
                hash = (hash * 10786583) ^ (!string.IsNullOrEmpty(this.Description) ? this.Description.GetConstantHashCode() : 0);
                hash = (hash * 10786583) ^ (!string.IsNullOrEmpty(this.ThumbUrl) ? this.ThumbUrl.GetConstantHashCode() : 0);
                hash = (hash * 10786583) ^ this.Price.GetHashCode();

                return hash;
            }
        }
    }
}