#nullable enable

namespace DocumentStores.Primitives
{
    ///<summary>
    /// Represents an empty value.
    ///</summary>    
    public sealed class Unit // User struct instead!
    {
        ///<summary>
        /// Creates a new instance of the <see cref="Unit" /> class
        ///</summary>
        public static Unit Default => new Unit();
    }

}