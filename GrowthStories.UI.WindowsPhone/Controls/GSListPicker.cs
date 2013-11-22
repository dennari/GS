
using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
namespace Growthstories.UI.WindowsPhone
{

    public class GSViewGrid : ContentControl
    {

        public GSViewGrid()
            : base()
        {
            DefaultStyleKey = typeof(GSViewGrid);
            VerticalContentAlignment = System.Windows.VerticalAlignment.Top;
            HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch;
            //SetBinding(TemplateProperty,)
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register("Title", typeof(string), typeof(GSViewGrid), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the Hint
        /// </summary>
        public string Title
        {
            get { return base.GetValue(TitleProperty) as string; }
            set
            {
                if (value != Title)
                    SetValue(TitleProperty, value);
            }
        }

        public static readonly DependencyProperty TopTitleProperty = DependencyProperty.Register("TopTitle", typeof(string), typeof(GSViewGrid), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the Hint
        /// </summary>
        public string TopTitle
        {
            get { return base.GetValue(TopTitleProperty) as string; }
            set
            {
                if (value != TopTitle)
                    SetValue(TopTitleProperty, value);
            }
        }

    }

    public class GSListPicker : ListPicker
    {
        public GSListPicker()
            : base()
        {
            DefaultStyleKey = typeof(ListPicker);
            //SetBinding(TemplateProperty,)
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //Get the mouse over animation in the control template
            var FocusedState = GetTemplateChild("Highlighted") as VisualState;

            if (FocusedState == null)
                return;

            var animation = FocusedState.Storyboard.Children[2] as ObjectAnimationUsingKeyFrames;
            if (animation != null)
                animation.KeyFrames[0].Value = Application.Current.Resources["GSAccentBrush"];
            //Storyboard.SetTarget(borderBrushAnimation,)
        }
    }

    public class GSToggleSwitch : ToggleSwitch
    {
        public GSToggleSwitch()
            : base()
        {
            //DefaultStyleKey = typeof(ListPicker);
            //SetBinding(TemplateProperty,)
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //Get the mouse over animation in the control template
            //var FocusedState = GetTemplateChild("Highlighted") as VisualState;

            //if (FocusedState == null)
            //    return;

            //var animation = FocusedState.Storyboard.Children[2] as ObjectAnimationUsingKeyFrames;
            //if (animation != null)
            //    animation.KeyFrames[0].Value = Application.Current.Resources["GSAccentBrush"];
            //Storyboard.SetTarget(borderBrushAnimation,)
        }
    }

    public class GSLabel : ContentControl
    {
        public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(GSLabel), new PropertyMetadata(
            new PropertyChangedCallback(OnLabelPropertyChanged)
        ));

        /// <summary>
        /// Gets or sets the Hint
        /// </summary>
        public string Label
        {
            get { return base.GetValue(LabelProperty) as string; }
            set
            {
                if (value != Label)
                    SetValue(LabelProperty, value);
            }
        }

        private static void OnLabelPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            //GSLabel label = sender as GSLabel;
            //string value = args.NewValue as string;

            //if (label != null && label.Label != value)
            //{
            //    label.Label = value;
            //}
        }
    }

    public class GSTextBox : PhoneTextBox
    {
        public GSTextBox()
            : base()
        {
            //DefaultStyleKey = typeof(ListPicker);
            //SetBinding(TemplateProperty,)
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //Get the mouse over animation in the control template
            var FocusedState = GetTemplateChild("Focused") as VisualState;

            if (FocusedState == null)
                return;

            //get rid of the storyboard it uses, so the mouse over does nothing
            var board = FocusedState.Storyboard;
            var animation = FocusedState.Storyboard.Children[1] as ObjectAnimationUsingKeyFrames;
            if (animation != null)
                animation.KeyFrames[0].Value = Application.Current.Resources["GSAccentBrush"];
            //Storyboard.SetTarget(borderBrushAnimation,)
        }
    }

    public class GSChatTextBox : ChatBubbleTextBox
    {
        public GSChatTextBox()
            : base()
        {
            //DefaultStyleKey = typeof(ListPicker);
            //SetBinding(TemplateProperty,)
            ChatBubbleDirection = Coding4Fun.Toolkit.Controls.ChatBubbleDirection.LowerRight;
            AcceptsReturn = true;
            TextWrapping = System.Windows.TextWrapping.Wrap;
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            //Get the mouse over animation in the control template
            var FocusedState = GetTemplateChild("Focused") as VisualState;

            if (FocusedState == null)
                return;

            //get rid of the storyboard it uses, so the mouse over does nothing
            var board = FocusedState.Storyboard;
            foreach (var i in new int[] { 1, 2, 3, 4, 5 })
            {
                var animation = FocusedState.Storyboard.Children[i] as ObjectAnimationUsingKeyFrames;
                if (animation != null)
                    animation.KeyFrames[0].Value = Application.Current.Resources["GSAccentBrush"];

            }

            Border B = null;
            if (this.MinHeight > 0)
            {
                B = GetTemplateChild("EnabledBorder") as Border;
                if (B != null) B.MinHeight = this.MinHeight;
                B = GetTemplateChild("DisabledOrReadonlyBorder") as Border;
                if (B != null) B.MinHeight = this.MinHeight;

            }

            //Storyboard.SetTarget(borderBrushAnimation,)
        }
    }



    public class GSLongListSelector : LongListSelector
    {
        public GSLongListSelector()
        {
            SelectionChanged += LongListSelector_SelectionChanged;
        }

        void LongListSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectedItem = base.SelectedItem;
        }

        public static readonly DependencyProperty SelectedItemProperty =
            DependencyProperty.Register(
                "SelectedItem",
                typeof(object),
                typeof(GSLongListSelector),
                new PropertyMetadata(null, OnSelectedItemChanged)
            );

        private static void OnSelectedItemChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var selector = (GSLongListSelector)d;
            selector.SetSelectedItem(e);
        }

        private void SetSelectedItem(DependencyPropertyChangedEventArgs e)
        {
            base.SelectedItem = e.NewValue;
        }

        public new object SelectedItem
        {
            get { return GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }
    }

}
