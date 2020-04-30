namespace AspectedRouting.Typ
{
    public abstract class Type
    {
        protected Type(string name, bool isBuiltin)
        {
            Name = name;
            if (isBuiltin)
            {
                Typs.AddBuiltin(this);

            }
            
        }

        public string Name { get; }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return obj is Type t && t.Name.Equals(Name);
        }

        protected bool Equals(Type other)
        {
            return string.Equals(Name, other.Name);
        }

        public override int GetHashCode()
        {
            return (Name != null ? Name.GetHashCode() : 0);
        }
    }
}