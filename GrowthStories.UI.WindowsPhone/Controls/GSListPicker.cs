
using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
namespace Growthstories.UI.WindowsPhone
{
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


}
