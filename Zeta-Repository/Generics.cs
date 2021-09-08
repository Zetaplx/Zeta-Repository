using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Zeta.Generics
{
    /// <summary>
    /// A generic data storage unit. Can store properties (or external variable references via getters and setters) and fields (local/internal variables) of any object type.
    /// </summary>
    public class Rolodex : DynamicObject
    {
        Dictionary<string, (Func<object> getter, Action<object> setter)> Properties; // Effective references via getters and setters to external parameters
        Dictionary<string, object> Fields; // Localized parameters, are uneffected outside the Rolodex

        public delegate object Getter();
        public delegate void Setter(object obj);

        public Dictionary<string, (Func<object> getter, Action<object> setter)> GetProperties() => Properties;
        public Dictionary<string, object> GetFields() => Fields;

        public Rolodex()
        {
            Properties = new Dictionary<string, (Func<object> getter, Action<object> setter)>();
            Fields = new Dictionary<string, object>();
        }

        public object this[string name]
        {
            get
            {
                if (TryGetValue(name, out var output)) return output;
                throw new System.EntryPointNotFoundException("Unable to find object with name \"" + name + "\" in the Rolodex.");
            } 
            set
            {
                Push(name, value);
            }
        }

        /// <summary>
        /// Push an object into the Rolodex. If there exists a property or field with the given name, will attempt to insert the object into that variable.
        /// </summary>
        public bool Push<T>(string name, T obj)
        {
            if (Properties.TryGetValue(name, out var prop)) {
                if (prop.getter() is T) { prop.setter(obj); return true; }
                throw new InvalidCastException("Unable to Push property. Incorrect datatype.");
            }
            else if (Fields.TryGetValue(name, out var field)) { Fields[name] = obj; return true; }

            Fields.Add(name, obj);
            return false;
        }
        public bool Push(string name, object obj)
        {
            if (Properties.TryGetValue(name, out var prop)) { prop.setter(obj); return true; }
            else if (Fields.TryGetValue(name, out var field)) { Fields[name] = obj; return true; }
            Fields.Add(name, obj);
            return false;
        }
        public void PushAll(Rolodex dex)
        {
            foreach(var prop in dex.GetProperties())
            {
                Register(prop.Key, prop.Value.getter, prop.Value.setter);
            }
            foreach(var field in dex.GetFields())
            {
                Push(field.Key, field.Value);
            }
        }


        /// <summary>
        /// Pushes a property into the Rolodex. If a given field exists with the same name, replaces it with property and sets property value to be equal to the old field.
        /// </summary>
        public bool Register<T>(string name, Func<T> getter, Action<T> setter)
        {
            if (Properties.TryGetValue(name, out var prop)) { prop.getter = () => getter(); prop.setter = (d) => setter((T)d); return true; }
            else if(Fields.TryGetValue(name, out var field))
            {
                if(field is T)
                {
                    Fields.Remove(name);
                    Properties.Add(name, (() => getter(), prop.setter = (d) => setter((T)d)));
                    setter((T)field);
                    return true;
                }
                else
                {
                    throw new InvalidCastException("Unable to Push property. Existing field blocked push.");
                }
            }

            Properties.Add(name, (() => getter(), prop.setter = (d) => setter((T)d)));

            return false;
        }
        public bool Register(string name, Getter getter, Setter setter)
        {
            if (Properties.TryGetValue(name, out var prop)) { prop.getter = () => getter(); prop.setter = (d) => setter(d); return true; }
            else if (Fields.TryGetValue(name, out var field))
            {
                Fields.Remove(name);
                Properties.Add(name, (() => getter(), prop.setter = (d) => setter(d)));
                setter(field);
                return true;
            }

            Properties.Add(name, (() => getter(), prop.setter = (d) => setter(d)));

            return false;
        }


        /// <summary>
        /// Attempts to get a value of type T from the Rolodex. If successful, returns true and sets output to the value. Otherwise sets output to default and returns false.
        /// </summary>
        public bool TryGetValue<T>(string name, out T output)
        {
            if (Properties.TryGetValue(name, out var prop))
            {
                if (prop.getter() is T)
                {
                    output = (T)prop.getter();
                    return true;
                }
            } else if (Fields.TryGetValue(name, out var field))
            {
                if(field is T)
                {
                    output = (T)field;
                    return true;
                }
            }

            output = default;
            return false;
        }

        public bool TryGetValue(string name, out object output)
        {
            if (Properties.TryGetValue(name, out var prop))
            {
                output = prop.getter();
                return true;

            }
            else if (Fields.TryGetValue(name, out var field))
            {

                output = field;
                return true;

            }

            output = default;
            return false;
        }

        /// <summary>
        /// Retrieves value from Rolodex. If given value is not already present in datastore, generates new data with correct name and type set to default.
        /// </summary>
        public T Pull<T>(string name, T defaultValue = default)
        {
            if (TryGetValue<T>(name, out var output)) return output;
            Push(name, defaultValue);
            TryGetValue(name, out output);
            return output;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            Push(binder.Name, value);
            return true;
        }

        public dynamic AsDynamic() => (dynamic)this;
    }
}
