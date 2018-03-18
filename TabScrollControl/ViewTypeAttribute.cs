using System;

namespace TabScrollControl
{
    /// <summary>
    /// Atribut pro definici typu UIElementu zobrazujícího sekci v TabScrollContainer
    /// </summary>
    public class ViewTypeAttribute : Attribute
    {
        private readonly Type _type;

        /// <summary>
        /// Konstruktor
        /// </summary>
        /// <param name="type"></param>
        public ViewTypeAttribute(Type type)
        {
            // TODO: zde asi validace pro povolení pouze typy vycházejícího z UIElement
            _type = type;
        }

        /// <summary>
        /// Vrátí instanci UIElementu pro zobrazení
        /// </summary>
        /// <returns></returns>
        public object GetInstance()
        {
            return Activator.CreateInstance(_type);
        }
    }
}