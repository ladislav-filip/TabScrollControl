using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace TabScrollControl
{
    public partial class TabScrollContainer
    {
        private List<Tuple<object, UserControl>> m_viewSections = new List<Tuple<object, UserControl>>();

        public static readonly DependencyProperty SekceAsTabProperty = DependencyProperty.Register("SekceAsTab", typeof(bool), typeof(TabScrollContainer), new PropertyMetadata(OnSekceAsTabChanged));

        public static readonly DependencyProperty SekceProperty = DependencyProperty.Register("Sekce", typeof(List<object>), typeof(TabScrollContainer), new PropertyMetadata(OnSekceChanged));

        public static readonly DependencyProperty IndexVisibleProperty = DependencyProperty.Register("IndexVisible", typeof(bool), typeof(TabScrollContainer), new PropertyMetadata(OnIndexVisibleChanged));

        public static readonly DependencyProperty IndextTitleProperty = DependencyProperty.Register("IndextTitle", typeof(string), typeof(TabScrollContainer), new PropertyMetadata(OnIndextTitleChanged));

        public TabScrollContainer()
        {
            InitializeComponent();

            IndexVisible = true;

            Loaded += (sender, args) =>
            {
                var isDesign = DesignerProperties.GetIsInDesignMode(this);
                if (isDesign)
                {
                    var sections = new List<object>()
                    {
                        "Sekce 1",
                        "Sekce 2",
                        "Sekce 3",
                        "Sekce 4",
                    };
                    Sekce = sections;
                }
            };
        }

        public void Init()
        {            
            TabMain.Items.Clear();
            StackMain.Children.Clear();
            PanelIndex.Children.Clear();
            m_viewSections.Clear();

            if (Sekce == null) return;
            
            m_viewSections = new List<Tuple<object, UserControl>>();

            foreach (var s in Sekce)
            {
                var btn = new Button();
                btn.Content = s.ToString();
                PanelIndex.Children.Add(btn);
                m_viewSections.Add(new Tuple<object, UserControl>(s, CreateUserControl(s)));
            }

            Redraw();
        }

        private void Redraw()
        {
            TabMain.Items.Clear();
            StackMain.Children.Clear();
            if (Sekce == null) return;

            if (SekceAsTab)
            {
                TabMain.Visibility = Visibility.Visible;
                ScrollMain.Visibility = Visibility.Collapsed;
                foreach (var s in Sekce)
                {
                    var tb = new TabItem { Header = s.ToString() };
                    tb.Content = m_viewSections.Single(p => p.Item1 == s).Item2;

                    TabMain.Items.Add(tb);
                }
            }
            else
            {
                TabMain.Visibility = Visibility.Collapsed;
                ScrollMain.Visibility = Visibility.Visible;
                foreach (var s in Sekce)
                {
                    var sp = new StackPanel();
                    sp.Orientation = Orientation.Vertical;
                    var tb = new TextBlock();
                    tb.Text = s.ToString();
                    sp.Children.Add(tb);
                    var br = new Border();
                    var exp = new Expander();
                    exp.IsExpanded = true;
                    exp.Header = s.ToString();
                    br.Child = exp;
                    exp.Content = m_viewSections.Single(p => p.Item1 == s).Item2;                    
                    sp.Children.Add(br);
                    StackMain.Children.Add(sp);
                }
            }
        }

        private static UserControl CreateUserControl(object s)
        {
            UserControl uc = null;
            var type = s.GetType();
            var attr = type.GetCustomAttributes(typeof(ViewTypeAttribute), false).FirstOrDefault();
            if (attr != null)
            {
                var attrType = (ViewTypeAttribute) attr;
                uc = (UserControl) attrType.GetInstance();
                uc.DataContext = s;
            }
            else
            {
                uc = new UserControl();
            }

            return uc;
        }

        public bool IndexVisible
        {
            get => (bool)GetValue(IndexVisibleProperty);
            set => SetValue(IndexVisibleProperty, value);
        }

        public string IndextTitle
        {
            get => (string)GetValue(IndextTitleProperty);
            set => SetValue(IndextTitleProperty, value);
        }

        public bool SekceAsTab
        {
            get => (bool)GetValue(SekceAsTabProperty);
            set => SetValue(SekceAsTabProperty, value);
        }

        public List<object> Sekce
        {
            get => (List<object>)GetValue(SekceProperty);
            set => SetValue(SekceProperty, value);
        }

        private static void OnIndextTitleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TabScrollContainer ctrl)) return;
            var text = (string)e.NewValue;
            ctrl.IndexTitle.Text = text;
        }

        private static void OnIndexVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TabScrollContainer ctrl)) return;
            var state = (bool) e.NewValue;
            ctrl.PanelLeft.Visibility = state ? Visibility.Visible : Visibility.Collapsed;
        }

        private static void OnSekceAsTabChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TabScrollContainer ctrl)) return;
            ctrl.Redraw();
        }

        private static void OnSekceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (!(d is TabScrollContainer ctrl)) return;
            ctrl.Init();
        }

    }
}
