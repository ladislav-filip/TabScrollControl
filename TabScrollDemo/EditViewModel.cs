using TabScrollControl;

namespace TabScrollDemo
{
    [ViewType(typeof(EditView))]
    public class EditViewModel
    {
        public string Nazev { get; set; }

        public string Mesto { get; set; }

        public override string ToString()
        {
            return $"EditViewModel: {Nazev}";
        }
    }
}