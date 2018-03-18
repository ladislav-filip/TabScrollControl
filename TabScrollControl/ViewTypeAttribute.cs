using System;

namespace TabScrollControl
{
    public class ViewTypeAttribute : Attribute
    {
        private readonly Type _type;

        public ViewTypeAttribute(Type type)
        {
            _type = type;
        }

        public object GetInstance()
        {
            return Activator.CreateInstance(_type);
        }
    }
}