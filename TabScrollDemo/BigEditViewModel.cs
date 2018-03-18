using TabScrollControl;

namespace TabScrollDemo
{
    [ViewType(typeof(BigEditView))]
    public class BigEditViewModel
    {
        public string Nazev { get; set; }

        public string Mesto { get; set; }

        public override string ToString()
        {
            return $"Big: {Nazev}";
        }
    }
}