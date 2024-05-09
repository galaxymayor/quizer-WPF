using System;
using System.Collections.Generic;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace quizer_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow: Window
    {
        private readonly SolidColorBrush
            SolidBlue  = new(Color.FromRgb(0x05, 0x89, 0xfc)),
            SolidRed   = new(Color.FromRgb(0xd4, 0x38, 0x24)),
            SolidBlack = new(Color.FromRgb(0x00, 0x00, 0x00));
        private static bool inputBoxLock;
        private static bool inputBoxLockOnce;
        private static readonly char[] specialChars = ['[', '|', ']', '@'];
        public MainWindow()
        {
            InitializeComponent();
            inputBoxLock = false;
            inputBoxLockOnce = false;
        }


        private void Highlight(TextPointer start, TextPointer end)
        {
            if (inputBoxLock)
            {
                return;
            }
            try
            {
                inputBoxLock = true;
                TextPointer idx = start;
                TextRange toMark = new(start, end);
                toMark.ClearAllProperties();
                while (idx.CompareTo(end) == -1)
                {
                    var next = idx.GetNextInsertionPosition(LogicalDirection.Forward);
                    if (next == null)
                    {
                        return;
                    }
                    toMark.Select(idx, next);
                    if (!toMark.IsEmpty)
                    {
                        switch (toMark.Text[0])
                        {
                            case '[':
                            case '|':
                            case ']':
                                toMark.ApplyPropertyValue(TextElement.ForegroundProperty, SolidBlue);
                                break;
                            case '@':
                                toMark.ApplyPropertyValue(TextElement.ForegroundProperty, SolidRed);
                                break;
                            default:
                                //toMark.ApplyPropertyValue(TextElement.ForegroundProperty, SolidBlack);
                                break;
                        }
                    }
                    idx = next;
                    int seek = idx.GetTextInRun(LogicalDirection.Forward).IndexOfAny(specialChars);
                    if (seek == -1)
                    {
                        idx = idx.GetPositionAtOffset(idx.GetTextRunLength(LogicalDirection.Forward));
                    }
                    else
                    {
                        idx = idx.GetPositionAtOffset(seek);
                    }
                }
            }
            finally
            {
                inputBoxLock = false;
            }

        }

        private void mainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            TextRange trSource = new(inputBox.Document.ContentStart, inputBox.Document.ContentEnd);

            if (trSource.Text.Trim().Length == 0)
            {
                return;
            }
            var result = MessageBox.Show(this, "Are you sure you want to quit?", "Quit?", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);

            if (result != MessageBoxResult.Yes)
            {
                e.Cancel = true;
            }
        }

        private void inputBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = false;
            inputBoxLockOnce = e.Key == Key.Enter;
        }

        private void RichTextBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            bool ctrlPressed = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
            //var sender_ = (RichTextBox)sender;
            if (!ctrlPressed)
            {
                return;
            }
            if (e.Delta > 0 && textScale.Value < 3)
            {
                textScale.Value += 0.1;
            }
            else if (e.Delta < 0 && textScale.Value > 1)
            {
                textScale.Value -= 0.1;
            }
            e.Handled = true;
        }

        private void inputBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inputBoxLock)
            {
                return;
            }
            if (inputBoxLockOnce)
            {
                inputBoxLockOnce = false;
                return;
            }
            //int l = int.MaxValue, r = int.MinValue;
            foreach(TextChange c in e.Changes)
            {
                if (c.AddedLength <= 0)
                {
                    continue;
                }
                Highlight(inputBox.Document.ContentStart.GetPositionAtOffset(c.Offset), inputBox.Document.ContentStart.GetPositionAtOffset(c.Offset+c.AddedLength) ?? inputBox.Document.ContentEnd);

            }

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            TextRange trSource = new(inputBox.Document.ContentStart, inputBox.Document.ContentEnd);
            TextRange trQuestions = new(questionBox.Document.ContentStart, questionBox.Document.ContentEnd);
            TextRange trAnswers = new(answersBox.Document.ContentStart, answersBox.Document.ContentEnd);
            Interface.Result result = Interface.LinesToResult(trSource.Text.Split("\r\n"));
            trQuestions.Text = string.Join("\n--------\n", result.questionParts);
            trAnswers.Text = "";
            foreach (var anss in result.answersParts)
            {
                int idx = anss.Key - 1;
                foreach (var answer in anss.Value)
                {
                    trAnswers.Text += $"{++idx}.{answer}\n";
                }
            }

            if (result.messages.infos.Count > 0)
            {
                MessageBox.Show(string.Join('\n', result.messages.infos), "Information", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            if (result.messages.warnings.Count > 0)
            {
                MessageBox.Show(string.Join('\n', result.messages.warnings), "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if (result.messages.errors.Count > 0)
            {
                MessageBox.Show(string.Join('\n', result.messages.Item3), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
